using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.TapHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.WaitImageHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WaitImageHelper
    {
        /// <summary>
        /// Default 500
        /// </summary>
        public int DelayStep { get; set; } = 500;
        /// <summary>
        /// 
        /// </summary>
        public event Action<string>? LogCallback;
        /// <summary>
        /// Default: false
        /// </summary>
        public bool FindInThreadPool { get; set; } = false;



        internal Random _Random { get; } = new Random(DateTime.Now.GetHashCode());
        internal void WriteLog(string text)
        {
            LogCallback?.Invoke(text);
        }



        internal CancellationToken CancellationToken { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public WaitImageHelper(CancellationToken cancellationToken = default)
        {
            this.CancellationToken = cancellationToken;
        }



        public Func<Task<Bitmap>>? CaptureAsync { get; internal set; }
        public Func<string, int, Bitmap?>? Template { get; internal set; }
        public Func<string, Rectangle?>? Crop { get; internal set; }
        public Func<double> MatchRate { get; internal set; } = () => 0.95;
        public Func<int> Timeout { get; internal set; } = () => 20000;
        public IEnumerable<string> GlobalNameFindFirst { get; internal set; } = Enumerable.Empty<string>();
        public Func<int, CancellationToken, Task> Delay { get; internal set; } = Task.Delay;
        public IEnumerable<string> GlobalNameFindLast { get; internal set; } = Enumerable.Empty<string>();



        public Action<Bitmap>? DrawDebugRectangle { get; internal set; }
        public FontFamily? FontFamilyDrawTextDebugRectangle { get; internal set; }
        public Color ColorDrawDebugRectangle { get; internal set; } = Color.Red;
        public float ColorDrawDebugFontEmSize { get; internal set; } = 8.0f;

        protected void Check()
        {
        }
    }

    public class WaitImageHelper<TColor, TDepth> : WaitImageHelper
            where TColor : struct, IColor
            where TDepth : new()
    {
        public WaitImageHelper(CancellationToken cancellationToken = default) : base(cancellationToken)
        {

        }
        public WaitImageBuilder<TColor, TDepth> WaitUntil(params string[] finds)
        {
            Check();
            return new WaitImageBuilder<TColor, TDepth>(this, WaitMode.WaitUntil, finds);
        }
        public WaitImageBuilder<TColor, TDepth> WaitUntil(TapFlag tapFlag, TapBuilder tapBuilder)
        {
            Check();
            return new WaitImageBuilder<TColor, TDepth>(this, WaitMode.WaitUntil, tapBuilder.Names.ToArray())
                .AndTap(tapFlag, tapBuilder);
        }
        public WaitImageBuilder<TColor, TDepth> FindImage(params string[] finds)
        {
            Check();
            return new WaitImageBuilder<TColor, TDepth>(this, WaitMode.FindImage, finds);
        }
        public WaitImageBuilder<TColor, TDepth> FindImage(TapFlag tapFlag, TapBuilder tapBuilder)
        {
            Check();
            return new WaitImageBuilder<TColor, TDepth>(this, WaitMode.FindImage, tapBuilder.Names.ToArray())
                .AndTap(tapFlag, tapBuilder);
        }
    }

    public static class WaitImageHelperExtensions
    {
        public static T WithCapture<T>(this T t, Func<Bitmap> capture) where T : WaitImageHelper
        {
            if (capture is null) throw new ArgumentNullException(nameof(capture));
            t.CaptureAsync = () => Task.FromResult(capture.Invoke());
            return t;
        }
        public static T WithCapture<T>(this T t, Func<Task<Bitmap>> capture) where T : WaitImageHelper
        {
            t.CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return t;
        }


        public static T WithImageTemplate<T>(this T t, Func<string, int, Bitmap> template) where T : WaitImageHelper
        {
            t.Template = template ?? throw new ArgumentNullException(nameof(template));
            return t;
        }
        public static T WithImageTemplate<T>(this T t, ImageTemplateHelper imageTemplateHelper) where T : WaitImageHelper
        {
            if (imageTemplateHelper is null) throw new ArgumentNullException(nameof(imageTemplateHelper));
            t.Template = imageTemplateHelper.GetImage;
            return t;
        }


        public static T WithCrop<T>(this T t, Func<string, Rectangle?> crop) where T : WaitImageHelper
        {
            t.Crop = crop ?? throw new ArgumentNullException(nameof(crop));
            return t;
        }

        public static T WithMatchRate<T>(this T t, Func<double> matchRate) where T : WaitImageHelper
        {
            t.MatchRate = matchRate ?? throw new ArgumentNullException(nameof(matchRate));
            return t;
        }

        public static T WithTimeout<T>(this T t, Func<int> timeout) where T : WaitImageHelper
        {
            t.Timeout = timeout ?? throw new ArgumentNullException(nameof(timeout));
            return t;
        }

        public static T WithGlobalNameFindFirst<T>(this T t, IEnumerable<string> globalNameFindFirst) where T : WaitImageHelper
        {
            t.GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return t;
        }
        public static T WithGlobalNameFindFirst<T>(this T t, params string[] globalNameFindFirst) where T : WaitImageHelper
        {
            t.GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return t;
        }

        public static T WithCustomDelay<T>(this T t, Func<int, CancellationToken, Task> delay) where T : WaitImageHelper
        {
            t.Delay = delay ?? throw new ArgumentNullException(nameof(delay));
            return t;
        }

        public static T WithGlobalNameFindLast<T>(this T t, IEnumerable<string> globalNameFindLast) where T : WaitImageHelper
        {
            t.GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return t;
        }
        public static T WithGlobalNameFindLast<T>(this T t, params string[] globalNameFindLast) where T : WaitImageHelper
        {
            t.GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return t;
        }


        public static T WithDrawDebugRectangle<T>(this T t, Action<Bitmap> drawDebugRectangle, FontFamily? fontFamily = null, Color? color = null, float fontEmSize = 8.0f) where T : WaitImageHelper
        {
            t.DrawDebugRectangle = drawDebugRectangle ?? throw new ArgumentNullException(nameof(drawDebugRectangle));
            t.FontFamilyDrawTextDebugRectangle = fontFamily;
            t.ColorDrawDebugFontEmSize = fontEmSize;

            t.ColorDrawDebugRectangle = color ?? Color.Red;

            if (t.FontFamilyDrawTextDebugRectangle is null)
            {
                using InstalledFontCollection installedFonts = new InstalledFontCollection();
                t.FontFamilyDrawTextDebugRectangle = installedFonts.Families.FirstOrDefault();
            }
            return t;
        }

    }

    public class WaitImageHelperBgr : WaitImageHelper<Bgr, byte>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public WaitImageHelperBgr(CancellationToken cancellationToken = default) : base(cancellationToken)
        {
        }
    }
}
