using Emgu.CV;
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
    public class WaitImageBuilder<TColor, TDepth>
            where TColor : struct, IColor
            where TDepth : new()
    {
        internal WaitImageHelper<TColor, TDepth> WaitImageHelper { get; }
        internal WaitMode WaitMode { get; }
        internal string[] Finds { get; }

        internal WaitImageBuilder(WaitImageHelper<TColor, TDepth> waitImageHelper, WaitMode waitMode, params string[] finds)
        {
            this.Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this.WaitImageHelper = waitImageHelper ?? throw new ArgumentNullException(nameof(waitImageHelper));
            this.WaitMode = waitMode;
        }


        internal Func<Task<Bitmap>>? CaptureAsync { get; private set; }
        internal Func<string, int, Bitmap>? Template { get; private set; }
        internal int? Timeout { get; private set; } = null;
        internal int? DelayStep { get; private set; } = null;
        internal TapFlag Tapflag { get; private set; } = TapFlag.First;
        internal bool IsThrow { get; private set; } = false;
        internal bool IsResetTimeout { get; private set; } = true;
        internal TapActionAsyncDelegate? TapCallbackAsync { get; private set; }
        internal Func<Task>? BeforeFindAsync { get; private set; }
        internal ImageNamesFilter? ImageNamesFilter { get; private set; }




        internal Task<Bitmap> GetCaptureAsync()
            => (CaptureAsync ?? WaitImageHelper.CaptureAsync ?? throw new InvalidOperationException("Capture is not set"))();
        internal Bitmap? GetTemplate(string name, int index)
            => (Template ?? WaitImageHelper.Template ?? throw new InvalidOperationException("Template is not set"))(name, index);
        internal int GetTimeout { get { return Timeout ?? WaitImageHelper.Timeout(); } }
        internal int GetDelayStep { get { return DelayStep ?? WaitImageHelper.DelayStep; } }


        protected void Check()
        {
            if (WaitImageHelper.Template is null &&
                Template is null)
                throw new InvalidOperationException($"Template must be set via {nameof(WaitImageHelper<TColor,TDepth>)}.{nameof(WaitImageHelper<TColor, TDepth>.WithImageTemplate)} or {nameof(WaitImageBuilder<TColor, TDepth>)}.{nameof(WaitImageBuilder<TColor, TDepth>.WithTemplateSource)}");

            if (WaitImageHelper.CaptureAsync is null &&
                this.CaptureAsync is null)
                throw new InvalidOperationException($"Capture must be set via {nameof(WaitImageHelper<TColor, TDepth>)}.{nameof(WaitImageHelper<TColor, TDepth>.WithCapture)} or {nameof(WaitImageBuilder<TColor, TDepth>)}.{nameof(WaitImageBuilder<TColor, TDepth>.WithCapture)}");
        }

        public Task<WaitImageResult<TColor, TDepth>> StartAsync()
        {
            Check();
            return new WaitImageResult<TColor, TDepth>(this).StartAsync();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayStep"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithDelayStep(int delayStep) 
        {
            if (delayStep <= 0) throw new ArgumentException($"{nameof(delayStep)} must be large than 0");
            this.DelayStep = delayStep;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithCapture(Func<Bitmap> capture) 
        {
            if (capture is null) throw new ArgumentNullException(nameof(capture));
            this.CaptureAsync = () => Task.FromResult(capture.Invoke());
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithCapture(Func<Task<Bitmap>> capture) 
        {
            this.CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return this;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder<TColor, TDepth> AndTap(TapFlag tapFlag, TapActionDelegate tapAction) 
        {
            if (tapAction is null) throw new ArgumentNullException(nameof(tapAction));
            this.TapCallbackAsync = (x) => Task.FromResult(tapAction.Invoke(x));
            this.Tapflag = tapFlag;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapActionAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder<TColor, TDepth> AndTap(TapFlag tapFlag, TapActionAsyncDelegate tapActionAsync) 
        {
            if (tapActionAsync is null) throw new ArgumentNullException(nameof(tapActionAsync));
            this.TapCallbackAsync = tapActionAsync;
            this.Tapflag = tapFlag;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapBuilder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder<TColor, TDepth> AndTap(TapFlag tapFlag, TapBuilder tapBuilder) 
        {
            if (tapBuilder is null) throw new ArgumentNullException(nameof(tapBuilder));
            this.TapCallbackAsync = tapBuilder.HandlerAsync;
            this.Tapflag = tapFlag;
            return this;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFindAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder<TColor, TDepth> BeforeFind(Func<Task> beforeFindAsync) 
        {
            this.BeforeFindAsync = beforeFindAsync ?? throw new ArgumentNullException(nameof(beforeFindAsync));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageBuilder<TColor, TDepth> BeforeFind(Action beforeFind) 
        {
            if (beforeFind is null) throw new ArgumentNullException(nameof(beforeFind));
            this.BeforeFindAsync = () =>
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
        public WaitImageBuilder<TColor, TDepth> WithTemplateSource(Func<string, int, Bitmap> template) 
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithTimeout(int? timeout) 
        {
            this.Timeout = timeout;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isResetTimeout"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithResetTimeout(bool isResetTimeout) 
        {
            this.IsResetTimeout = isResetTimeout;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageNamesFilter"></param>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithImageNamesFilter(ImageNamesFilter imageNamesFilter) 
        {
            this.ImageNamesFilter = imageNamesFilter;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitImageBuilder<TColor, TDepth> WithThrow() 
        {
            this.IsThrow = true;
            return this;
        }


    }
}
