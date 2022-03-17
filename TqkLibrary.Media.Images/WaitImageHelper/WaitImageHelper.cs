using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageHelper : IDisposable
    {
        /// <summary>
        /// Default 500
        /// </summary>
        public int DelayStep { get; set; } = 500;
        /// <summary>
        /// 
        /// </summary>
        public event Action<string> LogCallback;
        internal Func<Bitmap> Capture { get; }
        internal Func<string, int, Bitmap> Find { get; }
        internal Func<double> Percent { get; }
        internal Func<int> Timeout { get; }
        internal Func<string, Rectangle?> Crop { get; }

        /// <summary>
        /// 
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        internal readonly Random random = new Random(DateTime.Now.Millisecond);
        internal void WriteLog(string text)
        {
            LogCallback?.Invoke(text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capture"></param>
        /// <param name="find"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <param name="timeout"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitImageHelper(
            Func<Bitmap> capture,
            Func<string, int, Bitmap> find,
            Func<string, Rectangle?> crop,
            Func<double> percent,
            Func<int> timeout)
        {
            this.Capture = capture ?? throw new ArgumentNullException(nameof(capture));
            this.Find = find ?? throw new ArgumentNullException(nameof(find));
            this.Crop = crop ?? throw new ArgumentNullException(nameof(crop));
            this.Percent = percent ?? throw new ArgumentNullException(nameof(percent));
            this.Timeout = timeout ?? throw new ArgumentNullException(nameof(timeout));
        }
        /// <summary>
        /// 
        /// </summary>
        ~WaitImageHelper()
        {
            CancellationTokenSource.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            CancellationTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="finds"></param>
        /// <returns></returns>
        public WaitImageBuilder WaitUntil(params string[] finds)
        {
            return new WaitImageBuilder(this, finds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="finds"></param>
        /// <returns></returns>
        public WaitImageBuilder FindImage(params string[] finds)
        {
            return new WaitImageBuilder(this, finds) { IsLoop = false };
        }

    }
}
