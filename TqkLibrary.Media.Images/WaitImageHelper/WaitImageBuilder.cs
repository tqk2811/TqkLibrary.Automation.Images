using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageBuilder
    {
        internal WaitImageHelper _WaitImageHelper { get; }
        internal WaitImageBuilder(WaitImageHelper waitImageHelper, params string[] finds)
        {
            this._Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this._WaitImageHelper = waitImageHelper;
        }

        private int? _Timeout = null;
        private Func<Task<Bitmap>> _CaptureAsync;

        internal string[] _Finds { get; }
        internal TapFlag _Tapflag { get; private set; } = TapFlag.None;
        internal bool _IsThrow { get; private set; } = false;
        internal bool _IsFirst { get; private set; } = true;
        internal bool _IsLoop { get; set; } = true;
        internal bool _ResetTimeout { get; private set; } = true;
        internal Func<int, OpenCvFindResult, string[], Task<bool>> _TapCallbackAsync { get; private set; }
        internal Func<Task> _WorkAsync { get; private set; }


        internal Task<Bitmap> GetCaptureAsync() => (_CaptureAsync ?? _WaitImageHelper._CaptureAsync)();
        internal int GetTimeout { get { return _Timeout.HasValue ? _Timeout.Value : _WaitImageHelper._Timeout(); } }


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
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapFirst(Func<int, OpenCvFindResult, string[], bool> tapCallback)
        {
            if (tapCallback is null) throw new ArgumentNullException(nameof(tapCallback));
            this._TapCallbackAsync = (i, r, imgs) => Task.FromResult(tapCallback.Invoke(i, r, imgs));
            _IsFirst = true;
            _Tapflag = TapFlag.First;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallbackAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapFirst(Func<int, OpenCvFindResult, string[], Task<bool>> tapCallbackAsync)
        {
            this._TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            _IsFirst = true;
            _Tapflag = TapFlag.First;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapRandom(Func<int, OpenCvFindResult, string[], bool> tapCallback)
        {
            if (tapCallback is null) throw new ArgumentNullException(nameof(tapCallback));
            this._TapCallbackAsync = (i, r, imgs) => Task.FromResult(tapCallback.Invoke(i, r, imgs));
            _IsFirst = false;
            _Tapflag = TapFlag.Random;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallbackAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapRandom(Func<int, OpenCvFindResult, string[], Task<bool>> tapCallbackAsync)
        {
            this._TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            _IsFirst = false;
            _Tapflag = TapFlag.Random;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallback">bool (index,point,finds)<br>
        /// </br>index: found at index of finds<br>
        /// </br>point: point found<br>
        /// </br>return: if true, continue find. Else return</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapAll(Func<int, OpenCvFindResult, string[], bool> tapCallback)
        {
            if (tapCallback is null) throw new ArgumentNullException(nameof(tapCallback));
            this._TapCallbackAsync = (i, r, imgs) => Task.FromResult(tapCallback.Invoke(i, r, imgs));
            _IsFirst = false;
            _Tapflag = TapFlag.All;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapCallbackAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder AndTapAll(Func<int, OpenCvFindResult, string[], Task<bool>> tapCallbackAsync)
        {
            this._TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            _IsFirst = false;
            _Tapflag = TapFlag.All;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder Do(Func<Task> workAsync)
        {
            this._WorkAsync = workAsync ?? throw new ArgumentNullException(nameof(workAsync));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder Do(Action work)
        {
            if (work is null) throw new ArgumentNullException(nameof(work));
            this._WorkAsync = () =>
            {
                work.Invoke();
                return Task.CompletedTask;
            };
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
            this._ResetTimeout = isResetTimeout;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitImageBuilder WithThrow()
        {
            _IsThrow = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<WaitImageResult> StartAsync()
        {
            Check();
            return new WaitImageResult(this).StartAsync();
        }

        void Check()
        {
            if (_WaitImageHelper._CaptureAsync is null &&
                this._CaptureAsync is null)
                throw new InvalidOperationException($"Capture must be set via {nameof(WaitImageHelper)}.{nameof(WaitImageHelper.WithCapture)} or {nameof(WaitImageBuilder)}.{nameof(WaitImageBuilder.WithCapture)}");
        }
    }
}
