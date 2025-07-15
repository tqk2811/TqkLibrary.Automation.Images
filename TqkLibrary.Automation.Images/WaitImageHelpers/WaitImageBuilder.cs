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
    public abstract class WaitImageBuilder
    {
        internal WaitImageHelper WaitImageHelper { get; }
        internal WaitMode WaitMode { get; }
        internal string[] Finds { get; }

        internal WaitImageBuilder(WaitImageHelper waitImageHelper, WaitMode waitMode, params string[] finds)
        {
            this.Finds = finds ?? throw new ArgumentNullException(nameof(finds));
            if (finds.Length == 0) throw new ArgumentNullException(nameof(finds));
            this.WaitImageHelper = waitImageHelper ?? throw new ArgumentNullException(nameof(waitImageHelper));
            this.WaitMode = waitMode;
        }


        internal Func<Task<Bitmap>>? CaptureAsync { get; set; }
        internal Func<string, int, Bitmap>? Template { get; set; }
        internal int? Timeout { get; set; } = null;
        internal int? DelayStep { get; set; } = null;
        internal TapFlag Tapflag { get; set; } = TapFlag.First;
        internal bool IsThrow { get; set; } = false;
        internal bool IsResetTimeout { get; set; } = true;
        internal TapActionAsyncDelegate? TapCallbackAsync { get; set; }
        internal Func<Task>? BeforeFindAsync { get; set; }

        internal Task<Bitmap> GetCaptureAsync()
            => (CaptureAsync ?? WaitImageHelper.CaptureAsync ?? throw new InvalidOperationException("Capture is not set"))();
        internal Bitmap? GetTemplate(string name, int index)
            => (Template ?? WaitImageHelper.Template ?? throw new InvalidOperationException("Template is not set"))(name, index);
        internal int GetTimeout { get { return Timeout ?? WaitImageHelper.Timeout(); } }
        internal int GetDelayStep { get { return DelayStep ?? WaitImageHelper.DelayStep; } }
        internal ImageNamesFilter? ImageNamesFilter { get; set; }


        public abstract Task<WaitImageResult> StartAsync();
        protected void Check()
        {
            if (WaitImageHelper.Template is null &&
                Template is null)
                throw new InvalidOperationException($"Template must be set via {nameof(WaitImageHelperExtensions)}.{nameof(WaitImageHelperExtensions.WithImageTemplate)} or {nameof(WaitImageBuilderExtensions)}.{nameof(WaitImageBuilderExtensions.WithTemplateSource)}");

            if (WaitImageHelper.CaptureAsync is null &&
                this.CaptureAsync is null)
                throw new InvalidOperationException($"Capture must be set via {nameof(WaitImageHelperExtensions)}.{nameof(WaitImageHelperExtensions.WithCapture)} or {nameof(WaitImageBuilderExtensions)}.{nameof(WaitImageBuilderExtensions.WithCapture)}");
        }
    }

    public class WaitImageBuilder<TColor, TDepth> : WaitImageBuilder
            where TColor : struct, IColor
            where TDepth : new()
    {
        internal WaitImageBuilder(WaitImageHelper<TColor, TDepth> waitImageHelper, WaitMode waitMode, params string[] finds)
            : base(waitImageHelper, waitMode, finds)
        {
        }

        public override Task<WaitImageResult> StartAsync()
        {
            Check();
            return new WaitImageResult<TColor, TDepth>(this).StartAsync();
        }
    }

    public static class WaitImageBuilderExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayStep"></param>
        /// <returns></returns>
        public static T WithDelayStep<T>(this T t, int delayStep) where T : WaitImageBuilder
        {
            if (delayStep <= 0) throw new ArgumentException($"{nameof(delayStep)} must be large than 0");
            t.DelayStep = delayStep;
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public static T WithCapture<T>(this T t, Func<Bitmap> capture) where T : WaitImageBuilder
        {
            if (capture is null) throw new ArgumentNullException(nameof(capture));
            t.CaptureAsync = () => Task.FromResult(capture.Invoke());
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <returns></returns>
        public static T WithCapture<T>(this T t, Func<Task<Bitmap>> capture) where T : WaitImageBuilder
        {
            t.CaptureAsync = capture ?? throw new ArgumentNullException(nameof(capture));
            return t;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T AndTap<T>(this T t, TapFlag tapFlag, TapActionDelegate tapAction) where T : WaitImageBuilder
        {
            if (tapAction is null) throw new ArgumentNullException(nameof(tapAction));
            t.TapCallbackAsync = (x) => Task.FromResult(tapAction.Invoke(x));
            t.Tapflag = tapFlag;
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapActionAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T AndTap<T>(this T t, TapFlag tapFlag, TapActionAsyncDelegate tapActionAsync) where T : WaitImageBuilder
        {
            if (tapActionAsync is null) throw new ArgumentNullException(nameof(tapActionAsync));
            t.TapCallbackAsync = tapActionAsync;
            t.Tapflag = tapFlag;
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tapFlag"></param>
        /// <param name="tapBuilder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T AndTap<T>(this T t, TapFlag tapFlag, TapBuilder tapBuilder) where T : WaitImageBuilder
        {
            if (tapBuilder is null) throw new ArgumentNullException(nameof(tapBuilder));
            t.TapCallbackAsync = tapBuilder.HandlerAsync;
            t.Tapflag = tapFlag;
            return t;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFindAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T BeforeFind<T>(this T t, Func<Task> beforeFindAsync) where T : WaitImageBuilder
        {
            t.BeforeFindAsync = beforeFindAsync ?? throw new ArgumentNullException(nameof(beforeFindAsync));
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeFind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T BeforeFind<T>(this T t, Action beforeFind) where T : WaitImageBuilder
        {
            if (beforeFind is null) throw new ArgumentNullException(nameof(beforeFind));
            t.BeforeFindAsync = () =>
            {
                beforeFind.Invoke();
                return Task.CompletedTask;
            };
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T WithTemplateSource<T>(this T t, Func<string, int, Bitmap> template) where T : WaitImageBuilder
        {
            t.Template = template ?? throw new ArgumentNullException(nameof(template));
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static T WithTimeout<T>(this T t, int? timeout) where T : WaitImageBuilder
        {
            t.Timeout = timeout;
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isResetTimeout"></param>
        /// <returns></returns>
        public static T WithResetTimeout<T>(this T t, bool isResetTimeout) where T : WaitImageBuilder
        {
            t.IsResetTimeout = isResetTimeout;
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageNamesFilter"></param>
        /// <returns></returns>
        public static T WithImageNamesFilter<T>(this T t, ImageNamesFilter imageNamesFilter) where T : WaitImageBuilder
        {
            t.ImageNamesFilter = imageNamesFilter;
            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static T WithThrow<T>(this T t) where T : WaitImageBuilder
        {
            t.IsThrow = true;
            return t;
        }


    }
}
