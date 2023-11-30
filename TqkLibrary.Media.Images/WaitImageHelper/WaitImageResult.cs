using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageResult
    {
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<Tuple<int, OpenCvFindResult>> Points { get { return _points; } }
        readonly List<Tuple<int, OpenCvFindResult>> _points = new List<Tuple<int, OpenCvFindResult>>();


        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<string> FindNames { get; private set; }
        readonly WaitImageBuilder waitImageBuilder;
        internal WaitImageResult(WaitImageBuilder waitImageBuilder)
        {
            this.waitImageBuilder = waitImageBuilder;
        }

        Task<Bitmap> CaptureAsync() => waitImageBuilder.GetCaptureAsync();


        internal async Task<WaitImageResult> StartAsync()
        {
            FindNames = waitImageBuilder._WaitImageHelper._GlobalNameFindFirst
                .Concat(waitImageBuilder._Finds)
                .Concat(waitImageBuilder._WaitImageHelper._GlobalNameFindLast)
                .ToArray();

            waitImageBuilder._WaitImageHelper.WriteLog((waitImageBuilder._IsLoop ? "WaitUntil: " : "FindImage: ") + string.Join(",", FindNames));
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(waitImageBuilder.GetTimeout))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitImageBuilder._WaitImageHelper.CancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, Rectangle> dict_crops = new Dictionary<string, Rectangle>();

                    using Bitmap bitmap_capture = await CaptureAsync().ConfigureAwait(false);
                    if (bitmap_capture == null) throw new NullReferenceException(nameof(bitmap_capture));

                    for (int i = 0; i < FindNames.Count; i++)
                    {
                        for (int j = 0; ; j++)
                        {
                            using Bitmap bitmap_template = waitImageBuilder.GetTemplate(FindNames[i], j);
                            if (bitmap_template == null) break;

                            Rectangle? crop = waitImageBuilder._WaitImageHelper?._Crop.Invoke(FindNames[i]);
                            if (crop.HasValue && !dict_crops.ContainsKey(FindNames[i]))
                            {
                                dict_crops.Add(FindNames[i], crop.Value);
                            }

                            if (waitImageBuilder._IsFirst)
                            {
                                OpenCvFindResult result = await FindOutPointAsync(bitmap_capture, bitmap_template, crop).ConfigureAwait(false);

                                if (result != null)
                                {
                                    _points.Add(new Tuple<int, OpenCvFindResult>(i, result));
                                    waitImageBuilder._WaitImageHelper.WriteLog($"Found: {FindNames[i]}{j} {result}");

                                    if (await TapAsync(i, result, FindNames).ConfigureAwait(false))
                                    {
                                        //reset to while
                                        if (waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(waitImageBuilder.GetTimeout);
                                        i = FindNames.Count;//break i
                                        break;//break j
                                    }
                                    else return this;
                                }
                            }
                            else
                            {
                                var points = await FindOutPointsAsync(bitmap_capture, bitmap_template, crop);
                                if (points.Count > 0)
                                {
                                    waitImageBuilder._WaitImageHelper.WriteLog($"Found: {FindNames[i]}{j} {points.Count} points ({string.Join("|", points)})");
                                    _points.AddRange(points.Select(x => new Tuple<int, OpenCvFindResult>(i, x)));
                                }
                            }
                        }
                    }

                    if (!waitImageBuilder._IsFirst && _points.Count > 0)
                    {
                        switch (waitImageBuilder._Tapflag)
                        {
                            case TapFlag.All:
                                {
                                    List<bool> results = new List<bool>();
                                    foreach (var point in _points)
                                    {
                                        results.Add(await TapAsync(point.Item1, point.Item2, FindNames).ConfigureAwait(false));
                                    }
                                    if (results.All(x => x))
                                    {
                                        if (waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(waitImageBuilder.GetTimeout);
                                        if (waitImageBuilder._IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = waitImageBuilder._WaitImageHelper._Random.Next(_points.Count);
                                    if (await TapAsync(_points[random_index].Item1, _points[random_index].Item2, FindNames).ConfigureAwait(false))
                                    {
                                        if (waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(waitImageBuilder.GetTimeout);
                                        if (waitImageBuilder._IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                        }
                    }

                    DrawDebugRectangle(bitmap_capture, dict_crops);

                    if (!waitImageBuilder._IsLoop) break;
                    await DoAsync().ConfigureAwait(false);
                    await Task.Delay(waitImageBuilder.DelayStep, waitImageBuilder._WaitImageHelper.CancellationToken);
                }
            }
            if (waitImageBuilder._IsThrow) throw new WaitImageTimeoutException(string.Join("|", FindNames));
            return this;
        }

        private Task<OpenCvFindResult> FindOutPointAsync(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop)
        {
            if (waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindOutPoint(mainBitmap, subBitmap, crop, waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindOutPoint(mainBitmap, subBitmap, crop, waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }
        private Task<List<OpenCvFindResult>> FindOutPointsAsync(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop)
        {
            if (waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindOutPoints(mainBitmap, subBitmap, crop, waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindOutPoints(mainBitmap, subBitmap, crop, waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }

        private Task<bool> TapAsync(int index, OpenCvFindResult point, IReadOnlyList<string> finds)
        {
            Task<bool> task = waitImageBuilder._TapCallbackAsync?.Invoke(index, point, finds);
            return task ?? Task.FromResult(false);
        }

        private Task DoAsync()
        {
            Task task = waitImageBuilder._WorkAsync?.Invoke();
            return task ?? Task.CompletedTask;
        }

        private void DrawDebugRectangle(Bitmap bitmap_capture, IReadOnlyDictionary<string, Rectangle> crops)
        {
            if (waitImageBuilder._WaitImageHelper._DrawDebugRectangle is not null &&
                waitImageBuilder._WaitImageHelper._FontFamilyDrawTextDebugRectangle is not null)
            {
                Bitmap bitmap = new Bitmap(bitmap_capture);
                Task.Run(() =>
                {
                    try
                    {
                        WaitImageHelper waitImageHelper = waitImageBuilder._WaitImageHelper;
                        using Pen pen = new Pen(waitImageHelper._ColorDrawDebugRectangle);
                        using Brush text_brush = new SolidBrush(waitImageHelper._ColorDrawDebugRectangle);
                        using Font font = new Font(waitImageHelper._FontFamilyDrawTextDebugRectangle, waitImageHelper._ColorDrawDebugFontEmSize);
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            foreach (var group in crops.GroupBy(x => x.Value))
                            {
                                graphics.DrawRectangle(pen, group.Key);
                            }
                            foreach (var group in crops.GroupBy(x => x.Value.Location))
                            {
                                graphics.DrawString(
                                    string.Join(",", group.Select(x => x.Key)),
                                    font,
                                    text_brush,
                                    new PointF(group.Key.X, group.Key.Y)
                                    );
                            }
                        }
                        waitImageHelper._DrawDebugRectangle.Invoke(bitmap);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex.GetType().FullName}: {ex.Message}, {ex.StackTrace}");
                    }
                    finally
                    {
                        bitmap?.Dispose();
                    }
                });
            }
        }
    }
}