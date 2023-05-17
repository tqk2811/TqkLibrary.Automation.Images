using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageHelper
    {
        /// <summary>
        /// Default 500
        /// </summary>
        public int DelayStep { get; set; } = 500;
        /// <summary>
        /// 
        /// </summary>
        public event Action<string> LogCallback;
        /// <summary>
        /// Default: false
        /// </summary>
        public bool FindInThreadPool { get; set; } = false;



        internal Random _Random { get; } = new Random(DateTime.Now.Millisecond);
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



        internal Func<Task<Bitmap>> _CaptureAsync { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public WaitImageHelper WithCapture(Func<Bitmap> capture)
        {
            if (capture is null) throw new ArgumentNullException(nameof(capture));
            this._CaptureAsync = () => Task.FromResult(capture.Invoke());
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public WaitImageHelper WithCapture(Func<Task<Bitmap>> capture)
        {
            this._CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return this;
        }



        internal Func<string, int, Bitmap> _Template { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithImageTemplate(Func<string, int, Bitmap> template)
        {
            this._Template = template ?? throw new ArgumentNullException(nameof(template));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageTemplateHelper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithImageTemplate(ImageTemplateHelper imageTemplateHelper)
        {
            if (imageTemplateHelper is null) throw new ArgumentNullException(nameof(imageTemplateHelper));
            this._Template = imageTemplateHelper.GetImage;
            return this;
        }


        internal Func<string, Rectangle?> _Crop { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crop"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithCrop(Func<string, Rectangle?> crop)
        {
            this._Crop = crop ?? throw new ArgumentNullException(nameof(crop));
            return this;
        }


        internal Func<double> _MatchRate { get; private set; } = () => 0.95;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchRate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithMatchRate(Func<double> matchRate)
        {
            this._MatchRate = matchRate ?? throw new ArgumentNullException(nameof(matchRate));
            return this;
        }



        internal Func<int> _Timeout { get; private set; } = () => 20000;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithTimeout(Func<int> timeout)
        {
            this._Timeout = timeout ?? throw new ArgumentNullException(nameof(timeout));
            return this;
        }



        internal IEnumerable<string> _GlobalNameFindFirst { get; private set; } = Enumerable.Empty<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalNameFindFirst"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithGlobalNameFindFirst(IEnumerable<string> globalNameFindFirst)
        {
            this._GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalNameFindFirst"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithGlobalNameFindFirst(params string[] globalNameFindFirst)
        {
            this._GlobalNameFindFirst = globalNameFindFirst ?? throw new ArgumentNullException(nameof(globalNameFindFirst));
            return this;
        }



        internal IEnumerable<string> _GlobalNameFindLast { get; private set; } = Enumerable.Empty<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalNameFindLast"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithGlobalNameFindLast(IEnumerable<string> globalNameFindLast)
        {
            this._GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalNameFindLast"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithGlobalNameFindLast(params string[] globalNameFindLast)
        {
            this._GlobalNameFindLast = globalNameFindLast ?? throw new ArgumentNullException(nameof(globalNameFindLast));
            return this;
        }


        internal Action<Bitmap> _DrawDebugRectangle { get; private set; }
        internal FontFamily _FontFamilyDrawTextDebugRectangle { get; private set; }
        internal Color _ColorDrawDebugRectangle { get; private set; }
        internal float _ColorDrawDebugFontEmSize { get; private set; } = 8.0f;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawDebugRectangle"></param>
        /// <param name="fontFamily"></param>
        /// <param name="color"></param>
        /// <param name="fontEmSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper WithDrawDebugRectangle(Action<Bitmap> drawDebugRectangle, FontFamily fontFamily = null, Color? color = null, float fontEmSize = 8.0f)
        {
            this._DrawDebugRectangle = drawDebugRectangle ?? throw new ArgumentNullException(nameof(drawDebugRectangle));
            this._FontFamilyDrawTextDebugRectangle = fontFamily;
            this._ColorDrawDebugFontEmSize = fontEmSize;

            if (color.HasValue) this._ColorDrawDebugRectangle = color.Value;
            else this._ColorDrawDebugRectangle = Color.Red;

            if (this._FontFamilyDrawTextDebugRectangle is null)
            {
                using InstalledFontCollection installedFonts = new InstalledFontCollection();
                this._FontFamilyDrawTextDebugRectangle = installedFonts.Families.FirstOrDefault();
            }
            return this;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="finds"></param>
        /// <returns></returns>
        public WaitImageBuilder WaitUntil(params string[] finds)
        {
            Check();
            return new WaitImageBuilder(this, finds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="finds"></param>
        /// <returns></returns>
        public WaitImageBuilder FindImage(params string[] finds)
        {
            Check();
            return new WaitImageBuilder(this, finds) { _IsLoop = false };
        }


        void Check()
        {
        }
    }
}
