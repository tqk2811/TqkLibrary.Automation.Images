using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.TapHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.Test
{
    [TestClass]
    public class WaitImageHelperTest
    {
        class MyTapHelper : TapHelper
        {
            public override Task<Size> GetScreenSizeAsync(CancellationToken cancellationToken = default)
            {
                throw new System.NotImplementedException();
            }

            public override Task TapAsync(Point point, CancellationToken cancellationToken = default)
            {
                throw new System.NotImplementedException();
            }
        }
        static void TestBuild()
        {
            MyTapHelper tapHelper = new MyTapHelper();
            

            WaitImageHelperBgr waiter = new WaitImageHelperBgr()
                .WithCapture(() => new Bitmap(""))
                .WithCrop((name) => new Rectangle())
                .WithCustomDelay(Task.Delay)
                .WithGlobalNameFindFirst("abc")
                .WithGlobalNameFindLast("def")
                .WithMatchRate(() => 0.95)
                .WithTimeout(() => 30000)
                .WithImageTemplate((name,index) => new Bitmap(""));

            waiter.WaitUntil("abc", "def")
                .WithThrow()
                .AndTap(TapFlag.First, tapHelper.Build().Tap("").Break())
                .WithTimeout(1000)
                .WithDelayStep(100)
                .WithResetTimeout(true)
                .WithCapture(() => Task.FromResult(new Bitmap("")));
        }
    }
}
