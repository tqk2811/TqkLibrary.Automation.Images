using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    public class WaitImageHelper : IDisposable
    {
        public int DelayStep { get; set; } = 500;
        public event Action<string> LogCallback;
        internal Func<Bitmap> Capture { get; }
        internal Func<string, int, Bitmap> Find { get; }
        internal Func<double> Percent { get; }
        internal Func<int> Timeout { get; }
        internal Func<string, Rectangle?> Crop { get; }
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        internal readonly Random random = new Random(DateTime.Now.Millisecond);
        internal void WriteLog(string text)
        {
            LogCallback?.Invoke(text);
        }
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
        ~WaitImageHelper()
        {
            CancellationTokenSource.Dispose();
        }

        public WaitImageBuilder WaitUntil(params string[] finds)
        {
            return new WaitImageBuilder(this, finds);
        }
        public WaitImageBuilder FindImage(params string[] finds)
        {
            return new WaitImageBuilder(this, finds) { IsLoop = false };
        }


        public void Dispose()
        {
            CancellationTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
