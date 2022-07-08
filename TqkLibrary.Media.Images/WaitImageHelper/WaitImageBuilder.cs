using System;
using System.Drawing;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageBuilder
    {
        internal readonly WaitImageHelper waitImageHelper;
        internal WaitImageBuilder(WaitImageHelper waitImageHelper, params string[] finds)
        {
            this.Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this.waitImageHelper = waitImageHelper;
        }

        internal string[] Finds { get; }
        internal TapFlag Tapflag { get; private set; } = TapFlag.None;
        internal bool IsThrow { get; private set; } = false;
        internal bool IsFirst { get; private set; } = true;
        internal bool IsLoop { get; set; } = true;
        internal bool ResetTimeout { get; set; } = true;
        internal int? Timeout { get; set; } = null;


        private Func<int, OpenCvFindResult, string[], bool> _TapCallback = null;
        internal Func<int, OpenCvFindResult, string[], bool> TapCallback
        {
            get { return _TapCallback; }
            private set { _TapCallback = value; _TapCallbackAsync = null; }
        }

        private Func<int, OpenCvFindResult, string[], Task<bool>> _TapCallbackAsync = null;
        internal Func<int, OpenCvFindResult, string[], Task<bool>> TapCallbackAsync
        {
            get { return _TapCallbackAsync; }
            private set { _TapCallbackAsync = value; _TapCallback = null; }
        }


        Func<Task> _WorkAsync = null;
        internal Func<Task> WorkAsync
        {
            get { return _WorkAsync; }
            private set { _WorkAsync = value; _Work = null; }
        }

        Action _Work = null;
        internal Action Work
        {
            get { return _Work; }
            private set { _Work = value; _WorkAsync = null; }
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
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = true;
            Tapflag = TapFlag.First;
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
            this.TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            IsFirst = true;
            Tapflag = TapFlag.First;
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
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.Random;
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
            this.TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            IsFirst = false;
            Tapflag = TapFlag.Random;
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
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.All;
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
            this.TapCallbackAsync = tapCallbackAsync ?? throw new ArgumentNullException(nameof(tapCallbackAsync));
            IsFirst = false;
            Tapflag = TapFlag.All;
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
            this.WorkAsync = workAsync ?? throw new ArgumentNullException(nameof(workAsync));
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
            this.Work = work ?? throw new ArgumentNullException(nameof(work));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WaitImageBuilder WithTimeout(int? timeout)
        {
            this.Timeout = timeout;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isResetTimeout"></param>
        /// <returns></returns>
        public WaitImageBuilder WithResetTimeout(bool isResetTimeout)
        {
            this.ResetTimeout = isResetTimeout;
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
            return new WaitImageResult(this).StartAsync();
        }
    }
}
