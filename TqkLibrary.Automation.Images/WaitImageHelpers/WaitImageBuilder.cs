using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.TapHelpers;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.WaitImageHelpers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataResult"></param>
    /// <returns></returns>
    public delegate ActionShould TapActionDelegate(WaitImageDataResult dataResult);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataResult"></param>
    /// <returns></returns>
    public delegate Task<ActionShould> TapActionAsyncDelegate(WaitImageDataResult dataResult);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageNames"></param>
    /// <param name="lastFound"></param>
    /// <returns></returns>
    public delegate IEnumerable<string> ImageNamesFilter(IEnumerable<string> imageNames, string lastFound);

    /// <summary>
    /// 
    /// </summary>
    public class WaitImageBuilder
    {
        internal WaitImageHelper _WaitImageHelper { get; }
        internal WaitImageBuilder(WaitImageHelper waitImageHelper, WaitMode waitMode, params string[] finds)
        {
            this._Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this._WaitImageHelper = waitImageHelper ?? throw new ArgumentNullException(nameof(waitImageHelper));
            this.WaitMode = waitMode;
        }

        private int? _Timeout = null;
        private int? _DelayStep = null;
        private Func<Task<Bitmap>>? _CaptureAsync;
        Func<string, int, Bitmap>? _Template;

        internal string[] _Finds { get; }
        /// <summary>
        /// 
        /// </summary>
        public TapFlag Tapflag { get; private set; } = TapFlag.First;
        /// <summary>
        /// 
        /// </summary>
        public bool IsThrow { get; private set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public WaitMode WaitMode { get; }
        internal bool _IsResetTimeout { get; private set; } = true;
        internal TapActionAsyncDelegate? _TapCallbackAsync { get; private set; }
        internal Func<Task>? _BeforeFindAsync { get; private set; }

        internal Task<Bitmap> GetCaptureAsync()
            => (_CaptureAsync ?? _WaitImageHelper._CaptureAsync ?? throw new InvalidOperationException("Capture is not set"))();
        internal Bitmap GetTemplate(string name, int index)
            => (_Template ?? _WaitImageHelper._Template ?? throw new InvalidOperationException("Template is not set"))(name, index);
        internal int GetTimeout { get { return _Timeout.HasValue ? _Timeout.Value : _WaitImageHelper._Timeout(); } }
        internal int DelayStep { get { return _DelayStep.HasValue ? _DelayStep.Value : _WaitImageHelper.DelayStep; } }
        internal ImageNamesFilter? _ImageNamesFilter { get; private set; }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayStep"></param>
        /// <returns></returns>
        public WaitImageBuilder WithDelayStep(int delayStep)
        {
            if (delayStep <= 0) throw new ArgumentException($"{nameof(delayStep)} must be large than 0");
            this._DelayStep = delayStep;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public WaitImageBuilder WithCapture(Func<Bitmap> capture)
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
        public WaitImageBuilder WithCapture(Func<Task<Bitmap>> capture)
        {
            this._CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return this;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTap(TapFlag tapFlag, TapActionDelegate tapAction)
        {
            if (tapAction is null) throw new ArgumentNullException(nameof(tapAction));
            this._TapCallbackAsync = (x) => Task.FromResult(tapAction.Invoke(x));
            Tapflag = tapFlag;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapActionAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTap(TapFlag tapFlag, TapActionAsyncDelegate tapActionAsync)
        {
            if (tapActionAsync is null) throw new ArgumentNullException(nameof(tapActionAsync));
            this._TapCallbackAsync = tapActionAsync;
            Tapflag = tapFlag;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapBuilder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTap(TapFlag tapFlag, TapBuilder tapBuilder)
        {
            if (tapBuilder is null) throw new ArgumentNullException(nameof(tapBuilder));
            this._TapCallbackAsync = tapBuilder.HandlerAsync;
            Tapflag = tapFlag;
            return this;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFindAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder BeforeFind(Func<Task> beforeFindAsync)
        {
            this._BeforeFindAsync = beforeFindAsync ?? throw new ArgumentNullException(nameof(beforeFindAsync));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder BeforeFind(Action beforeFind)
        {
            if (beforeFind is null) throw new ArgumentNullException(nameof(beforeFind));
            this._BeforeFindAsync = () =>
            {
                beforeFind.Invoke();
                return Task.CompletedTask;
            };
            return this;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder WithTemplateSource(Func<string, int, Bitmap> template)
        {
            this._Template = template ?? throw new ArgumentNullException(nameof(template));
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WaitImageBuilder WithTimeout(int? timeout)
        {
            this._Timeout = timeout;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isResetTimeout"></param>
        /// <returns></returns>
        public WaitImageBuilder WithResetTimeout(bool isResetTimeout)
        {
            this._IsResetTimeout = isResetTimeout;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageNamesFilter"></param>
        /// <returns></returns>
        public WaitImageBuilder WithImageNamesFilter(ImageNamesFilter imageNamesFilter)
        {
            this._ImageNamesFilter = imageNamesFilter;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitImageBuilder WithThrow()
        {
            IsThrow = true;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<WaitImageResult> StartAsync()
        {
            Check();
            return new WaitImageResult<Bgr, byte>(this).StartAsync();
        }

        void Check()
        {
            if (_WaitImageHelper._Template is null &&
                _Template is null)
                throw new InvalidOperationException($"Template must be set via {nameof(WaitImageHelper)}.{nameof(WaitImageHelper.WithImageTemplate)} or {nameof(WaitImageBuilder)}.{nameof(WaitImageBuilder.WithTemplateSource)}");

            if (_WaitImageHelper._CaptureAsync is null &&
                this._CaptureAsync is null)
                throw new InvalidOperationException($"Capture must be set via {nameof(WaitImageHelper)}.{nameof(WaitImageHelper.WithCapture)} or {nameof(WaitImageBuilder)}.{nameof(WaitImageBuilder.WithCapture)}");
        }
    }
}
