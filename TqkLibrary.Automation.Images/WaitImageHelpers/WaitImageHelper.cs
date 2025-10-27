using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections;
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
    public class WaitImageHelper<TColor, TDepth>
            where TColor : struct, IColor
            where TDepth : new()
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



        public Func<Task<Bitmap>>? CaptureAsync { get; private set; }
        public Func<string, int, Bitmap?>? Template { get; private set; }
        public Func<string, Rectangle?>? Crop { get; private set; }
        public Func<double> MatchRate { get; private set; } = () => 0.95;
        public Func<int> Timeout { get; private set; } = () => 20000;
        public IEnumerable<string> GlobalNameFindFirst { get; private set; } = Enumerable.Empty<string>();
        public Func<int, CancellationToken, Task> Delay { get; private set; } = Task.Delay;
        public IEnumerable<string> GlobalNameFindLast { get; private set; } = Enumerable.Empty<string>();



        public Action<Bitmap>? DrawDebugRectangle { get; private set; }
        public FontFamily? FontFamilyDrawTextDebugRectangle { get; private set; }
        public Color ColorDrawDebugRectangle { get; private set; } = Color.Red;
        public float ColorDrawDebugFontEmSize { get; private set; } = 8.0f;

        protected void Check()
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










        public WaitImageHelper<TColor, TDepth> WithCapture(Func<Bitmap> capture) 
        {
            if (capture is null) throw new ArgumentNullException(nameof(capture));
            this.CaptureAsync = () => Task.FromResult(capture.Invoke());
            return this;
        }
        public WaitImageHelper<TColor, TDepth> WithCapture(Func<Task<Bitmap>> capture) 
        {
            this.CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return this;
        }


        public WaitImageHelper<TColor, TDepth> WithImageTemplate(Func<string, int, Bitmap> template) 
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
            return this;
        }
        public WaitImageHelper<TColor, TDepth> WithImageTemplate(ImageTemplateHelper imageTemplateHelper) 
        {
            if (imageTemplateHelper is null) throw new ArgumentNullException(nameof(imageTemplateHelper));
            this.Template = imageTemplateHelper.GetImage;
            return this;
        }


        public WaitImageHelper<TColor, TDepth> WithCrop(Func<string, Rectangle?> crop) 
        {
            this.Crop = crop ?? throw new ArgumentNullException(nameof(crop));
            return this;
        }

        public WaitImageHelper<TColor, TDepth> WithMatchRate(Func<double> matchRate) 
        {
            this.MatchRate = matchRate ?? throw new ArgumentNullException(nameof(matchRate));
            return this;
        }

        public WaitImageHelper<TColor, TDepth> WithTimeout(Func<int> timeout) 
        {
            this.Timeout = timeout ?? throw new ArgumentNullException(nameof(timeout));
            return this;
        }

        public WaitImageHelper<TColor, TDepth> WithGlobalNameFindFirst(IEnumerable<string> globalNameFindFirst) 
        {
            this.GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return this;
        }
        public WaitImageHelper<TColor, TDepth> WithGlobalNameFindFirst(params string[] globalNameFindFirst) 
        {
            this.GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return this;
        }

        public WaitImageHelper<TColor, TDepth> WithCustomDelay(Func<int, CancellationToken, Task> delay) 
        {
            this.Delay = delay ?? throw new ArgumentNullException(nameof(delay));
            return this;
        }

        public WaitImageHelper<TColor, TDepth> WithGlobalNameFindLast(IEnumerable<string> globalNameFindLast) 
        {
            this.GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return this;
        }
        public WaitImageHelper<TColor, TDepth> WithGlobalNameFindLast(params string[] globalNameFindLast) 
        {
            this.GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return this;
        }


        public WaitImageHelper<TColor, TDepth> WithDrawDebugRectangle(Action<Bitmap> drawDebugRectangle, FontFamily? fontFamily = null, Color? color = null, float fontEmSize = 8.0f) 
        {
            this.DrawDebugRectangle = drawDebugRectangle ?? throw new ArgumentNullException(nameof(drawDebugRectangle));
            this.FontFamilyDrawTextDebugRectangle = fontFamily;
            this.ColorDrawDebugFontEmSize = fontEmSize;

            this.ColorDrawDebugRectangle = color ?? Color.Red;

            if (this.FontFamilyDrawTextDebugRectangle is null)
            {
                using InstalledFontCollection installedFonts = new InstalledFontCollection();
                this.FontFamilyDrawTextDebugRectangle = installedFonts.Families.FirstOrDefault();
            }
            return this;
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
