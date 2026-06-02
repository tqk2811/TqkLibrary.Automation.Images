// MVC image-routing demo for TqkLibrary.Automation.Images.
//
// The flow is a small state machine driven by found images:
//   "home" -> "menu" -> ("settings" | "popup") -> done
// Every template resolves to the same sample image, which is always present in the
// captured scene, so each state is "found" immediately and the router advances.

using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using TqkLibrary.Automation.Images.MvcHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers;

// Resolve relative to the app output folder so it works under `dotnet run` too.
string basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "baseImage.png");     // the captured scene
string searchPath = Path.Combine(AppContext.BaseDirectory, "Resources", "searchImage.png"); // the template (contained in the scene)

// 1) DI container: the controller and a handler parameter are resolved from here.
ServiceProvider services = new ServiceCollection()
    .AddSingleton<ITapService, ConsoleTapService>()
    .BuildServiceProvider();

// 2) A waiter providing capture/template/match-rate/timeout. Reused by the MVC router.
WaitImageHelperBgr waiter = new WaitImageHelperBgr()
    .WithCapture(() => (Bitmap)Bitmap.FromFile(basePath))
    .WithImageTemplate((name, index) => index == 0 ? (Bitmap)Bitmap.FromFile(searchPath) : null!)
    .WithMatchRate(() => 0.9)
    .WithTimeout(() => 5000) as WaitImageHelperBgr ?? throw new InvalidOperationException();
waiter.LogCallback += text => Console.WriteLine($"[waiter] {text}");

// 3) Run the MVC router starting from the "home" image.
Console.WriteLine("=== MVC image-routing demo ===");
MvcRunResult result = await waiter.Mvc()
    .WithServiceProvider(services)
    .AddController<AppFlowController>()
    .StartAsync("home");

Console.WriteLine();
Console.WriteLine($"Run finished: {result.Reason}");
Console.WriteLine("History:");
foreach (FindHistory h in result.Histories)
    Console.WriteLine($"  - {h.ImageName} (template #{h.Index}) at {h.Result.Point}, match {h.Result.Percent:0.000}");


// ===== Services =====
interface ITapService
{
    void Tap(Point point);
}

sealed class ConsoleTapService : ITapService
{
    public void Tap(Point point) => Console.WriteLine($"    [tap] -> {point}");
}


// ===== Controller: methods marked with [ImageName] are the routes =====
sealed class AppFlowController
{
    readonly ITapService _tap;

    // Constructor injection via IServiceProvider.
    public AppFlowController(ITapService tap) => _tap = tap;

    [ImageName("home")]
    public string[] Home(MvcContext context)
    {
        Console.WriteLine($"[home] found at {context.Result.Point}; tapping, then look for menu");
        _tap.Tap(context.Result.Point);
        return ["menu"]; // next image(s) to search
    }

    // Async signature with an optional CancellationToken parameter.
    [ImageName("menu")]
    public async Task<string[]> MenuAsync(MvcContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[menu] found (history so far: {context.Histories.Count}); opening settings/popup");
        await Task.Delay(100, cancellationToken);
        _tap.Tap(context.Result.Point);
        return ["settings", "popup"]; // whichever is found first wins
    }

    // One handler can claim multiple names; extra parameters are resolved from DI.
    [ImageName("settings", "popup")]
    public string[] SettingsOrPopup(MvcContext context, ITapService tap)
    {
        Console.WriteLine($"[settings/popup] matched '{context.ImageName}'; finishing flow");
        tap.Tap(context.Result.Point);
        return []; // empty => MvcRunReason.Completed
    }
}
