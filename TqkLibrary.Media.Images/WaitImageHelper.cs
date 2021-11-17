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
    internal enum TapFlag
    {
        None,
        First,
        Random,
        All
    }

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
        internal Action<int, Point> TapCallback { get; private set; } = null;

        public WaitImageBuilder AndTapFirst(Action<int, Point> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = true;
            Tapflag = TapFlag.First;
            return this;
        }

        public WaitImageBuilder AndTapRandom(Action<int, Point> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.Random;
            return this;
        }
        public WaitImageBuilder AndTapAll(Action<int, Point> tapCallback)
        {
            this.TapCallback = tapCallback ?? throw new ArgumentNullException(nameof(tapCallback));
            IsFirst = false;
            Tapflag = TapFlag.All;
            return this;
        }

        public WaitImageBuilder WithThrow()
        {
            IsThrow = true;
            return this;
        }

        public WaitImageResult Build()
        {
            return new WaitImageResult(this).Start();
        }
    }

    public class WaitImageResult
    {
        public int IndexArg { get; private set; } = -1;
        public Point? Point { get { return _points.FirstOrDefault(); } }
        public IEnumerable<Point> Points { get { return _points; } }



        readonly List<Point> _points = new List<Point>();
        readonly WaitImageBuilder waitImageBuilder;
        internal WaitImageResult(WaitImageBuilder waitImageBuilder)
        {
            this.waitImageBuilder = waitImageBuilder;
        }

        internal WaitImageResult Start()
        {
            waitImageBuilder.waitImageHelper.WriteLog((waitImageBuilder.IsLoop ? "WaitUntil: " : "FindImage: ") + string.Join(",", waitImageBuilder.Finds));
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(waitImageBuilder.waitImageHelper.Timeout.Invoke()))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitImageBuilder.waitImageHelper.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    using Bitmap image = waitImageBuilder.waitImageHelper.Capture.Invoke();
                    if (image == null) throw new NullReferenceException(nameof(image));
                    for (int i = 0; i < waitImageBuilder.Finds.Length; i++)
                    {
                        for (int j = 0; ; j++)
                        {
                            using Bitmap find = waitImageBuilder.waitImageHelper.Find?.Invoke(waitImageBuilder.Finds[i], j);
                            if (find == null) break;
                            Rectangle? crop = waitImageBuilder.waitImageHelper.Crop?.Invoke(waitImageBuilder.Finds[i]);
                            if (waitImageBuilder.IsFirst)
                            {
                                Point? point = OpenCvHelper.FindOutPoint(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke());
                                if (point != null)
                                {
                                    IndexArg = i;
                                    waitImageBuilder.waitImageHelper.WriteLog($"Found: {waitImageBuilder.Finds[i]}{j}");
                                    return Tap(i, point.Value);
                                }
                            }
                            else
                            {
                                var points = OpenCvHelper.FindOutPoints(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke());
                                waitImageBuilder.waitImageHelper.WriteLog($"Found: {waitImageBuilder.Finds[i]}{j} {points.Count} points");
                                _points.AddRange(points);
                            }
                        }
                        if (!waitImageBuilder.IsLoop) break;
                    }

                    if (!waitImageBuilder.IsFirst && _points.Count > 0)
                        switch (waitImageBuilder.Tapflag)
                        {
                            case TapFlag.All:
                                {

                                    foreach (var point in _points) Tap(-1, point);
                                    return this;
                                }
                            case TapFlag.Random:
                                {
                                    return Tap(-1, _points[waitImageBuilder.waitImageHelper.random.Next(_points.Count)]);
                                }
                        }
                }
            }
            if (waitImageBuilder.IsThrow) throw new WaitImageTimeoutException(string.Join("|", waitImageBuilder.Finds));
            return this;
        }


        private WaitImageResult Tap(int index, Point point)
        {
            waitImageBuilder.TapCallback?.Invoke(index, point);
            return this;
        }
    }
    public class WaitImageTimeoutException : Exception
    {
        internal WaitImageTimeoutException(string message) : base(message)
        {
        }
    }
}
