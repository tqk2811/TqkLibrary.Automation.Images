using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.MvcHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers;

namespace TqkLibrary.Automation.Images.Test
{
    [TestClass]
    public class ImageMvcHelperTest
    {
        const string BasePath = ".\\Resources\\baseImage.png";
        const string SearchPath = ".\\Resources\\searchImage.png";

        // Capture/template load a fresh bitmap each call because WaitImageResult disposes them.
        static Bitmap LoadBase() => (Bitmap)Bitmap.FromFile(BasePath);
        static Bitmap LoadSearch(string name, int index) => index == 0 ? (Bitmap)Bitmap.FromFile(SearchPath) : null;

        static WaitImageHelperBgr CreateWaiter(int timeoutMs, Func<string, int, Bitmap> template, CancellationToken ct = default)
        {
            return (WaitImageHelperBgr)new WaitImageHelperBgr(ct)
                .WithCapture(LoadBase)
                .WithImageTemplate(template)
                .WithMatchRate(() => 0.9)
                .WithTimeout(() => timeoutMs);
        }

        class Recorder { public List<string> Calls { get; } = new List<string>(); }

        // ctor-injected dependency + flexible signatures (sync string[] and async Task<IEnumerable<string>>).
        class FlowController
        {
            readonly Recorder _recorder;
            public FlowController(Recorder recorder) { _recorder = recorder; }

            [ImageName("abc")]
            public string[] Abc(MvcContext context)
            {
                _recorder.Calls.Add($"abc:{context.ImageName}:{context.Index}:{context.Histories.Count}");
                return new[] { "def" };
            }

            [ImageName("def", "ghi")]
            public async Task<IEnumerable<string>> DefGhiAsync(MvcContext context, CancellationToken cancellationToken)
            {
                await Task.Yield();
                _recorder.Calls.Add($"defghi:{context.ImageName}:{context.Histories.Count}");
                return Array.Empty<string>();
            }
        }

        [TestMethod]
        public async Task Flow_Routes_AbcThenDef_Completes()
        {
            var services = new ServiceCollection()
                .AddSingleton<Recorder>()
                .BuildServiceProvider();
            var recorder = services.GetRequiredService<Recorder>();

            var waiter = CreateWaiter(5000, LoadSearch);

            MvcRunResult result = await waiter.Mvc()
                .WithServiceProvider(services)
                .AddController<FlowController>()
                .StartAsync("abc");

            Assert.AreEqual(MvcRunReason.Completed, result.Reason);
            Assert.AreEqual(2, result.Histories.Count);
            Assert.AreEqual("abc", result.Histories[0].ImageName);
            Assert.AreEqual(0, result.Histories[0].Index);
            Assert.AreEqual("def", result.Histories[1].ImageName);
            CollectionAssert.AreEqual(
                new[] { "abc:abc:0:0", "defghi:def:1" },
                recorder.Calls);
        }

        [TestMethod]
        public async Task Step_NotFound_ReturnsTimedOut()
        {
            var services = new ServiceCollection().AddSingleton<Recorder>().BuildServiceProvider();
            var waiter = CreateWaiter(400, (name, index) => null); // never found

            MvcRunResult result = await waiter.Mvc()
                .WithServiceProvider(services)
                .AddController<FlowController>()
                .StartAsync("abc");

            Assert.AreEqual(MvcRunReason.TimedOut, result.Reason);
            Assert.AreEqual(0, result.Histories.Count);
        }

        class DupA { [ImageName("x")] public string[] H(MvcContext c) => Array.Empty<string>(); }
        class DupB { [ImageName("x", "y")] public string[] H(MvcContext c) => Array.Empty<string>(); }

        [TestMethod]
        public async Task DuplicateImageName_Throws()
        {
            var waiter = CreateWaiter(5000, LoadSearch);
            var helper = waiter.Mvc().AddController<DupA>().AddController<DupB>();

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => helper.StartAsync("x"));
        }

        [TestMethod]
        public async Task EntryName_WithoutHandler_Throws()
        {
            var waiter = CreateWaiter(5000, LoadSearch);
            var helper = waiter.Mvc().AddController<FlowController>();

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => helper.StartAsync("unknown"));
        }
    }
}
