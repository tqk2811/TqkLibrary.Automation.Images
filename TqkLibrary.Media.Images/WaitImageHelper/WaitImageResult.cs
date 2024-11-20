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
        public IReadOnlyList<WaitImageDataResult> Points { get { return _points; } }
        readonly List<WaitImageDataResult> _points = new List<WaitImageDataResult>();


        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<string> FindNames { get; private set; } = new List<string>();
        readonly WaitImageBuilder _waitImageBuilder;
        internal WaitImageResult(WaitImageBuilder waitImageBuilder)
        {
            this._waitImageBuilder = waitImageBuilder ?? throw new ArgumentNullException(nameof(waitImageBuilder));
        }

        Task<Bitmap> CaptureAsync() => _waitImageBuilder.GetCaptureAsync();
        private string _lastFound = string.Empty;

        internal async Task<WaitImageResult> StartAsync()
        {
            FindNames = _waitImageBuilder._WaitImageHelper._GlobalNameFindFirst
                .Concat(_waitImageBuilder._Finds)
                .Concat(_waitImageBuilder._WaitImageHelper._GlobalNameFindLast)
                .ToArray();

            _waitImageBuilder._WaitImageHelper.WriteLog($"{(_waitImageBuilder._IsLoop ? "WaitUntil" : "FindImage")} : {string.Join(",", FindNames)}");
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(_waitImageBuilder.GetTimeout))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    _waitImageBuilder._WaitImageHelper.CancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, Rectangle> dict_crops = new Dictionary<string, Rectangle>();

                    using Bitmap bitmap_capture = await CaptureAsync().ConfigureAwait(false);
                    if (bitmap_capture == null) throw new NullReferenceException(nameof(bitmap_capture));

                    var findNamesFilter = _waitImageBuilder._ImageNamesFilter?.Invoke(FindNames, _lastFound).ToList() ?? FindNames;
                    if (!Enumerable.SequenceEqual(findNamesFilter, FindNames))
                    {
                        _waitImageBuilder._WaitImageHelper.WriteLog($"{(_waitImageBuilder._IsLoop ? "WaitUntil" : "FindImage")} (filter): {string.Join(",", findNamesFilter)}");
                    }

                    for (int i = 0; i < findNamesFilter.Count; i++)
                    {
                        for (int j = 0; ; j++)
                        {
                            using Bitmap bitmap_template = _waitImageBuilder.GetTemplate(findNamesFilter[i], j);
                            if (bitmap_template == null) break;

                            Rectangle? crop = _waitImageBuilder._WaitImageHelper._Crop?.Invoke(findNamesFilter[i]);
                            if (crop.HasValue && !dict_crops.ContainsKey(findNamesFilter[i]))
                            {
                                dict_crops.Add(findNamesFilter[i], crop.Value);
                            }

                            if (_waitImageBuilder._IsFirst)
                            {
                                OpenCvFindResult result = await FindOutPointAsync(bitmap_capture, bitmap_template, crop).ConfigureAwait(false);

                                if (result != null)
                                {
                                    WaitImageDataResult dataResult = new WaitImageDataResult(findNamesFilter[i], result);
                                    _points.Add(dataResult);
                                    _waitImageBuilder._WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {result}");
                                    _lastFound = findNamesFilter[i];

                                    if (await TapAsync(dataResult).ConfigureAwait(false))
                                    {
                                        //reset to while
                                        if (_waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        i = findNamesFilter.Count;//break i
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
                                    _waitImageBuilder._WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {points.Count} points ({string.Join("|", points)})");
                                    _lastFound = findNamesFilter[i];

                                    _points.AddRange(points.Select(x => new WaitImageDataResult(findNamesFilter[i], x)));
                                }
                            }
                        }
                    }

                    if (!_waitImageBuilder._IsFirst && _points.Count > 0)
                    {
                        switch (_waitImageBuilder._Tapflag)
                        {
                            case TapFlag.All:
                                {
                                    List<bool> results = new List<bool>();
                                    foreach (var point in _points)
                                    {
                                        results.Add(await TapAsync(point).ConfigureAwait(false));
                                    }
                                    if (results.All(x => x))
                                    {
                                        if (_waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        if (_waitImageBuilder._IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = _waitImageBuilder._WaitImageHelper._Random.Next(_points.Count);
                                    if (await TapAsync(_points[random_index]).ConfigureAwait(false))
                                    {
                                        if (_waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        if (_waitImageBuilder._IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                        }
                    }

                    DrawDebugRectangle(bitmap_capture, dict_crops);

                    if (!_waitImageBuilder._IsLoop) break;
                    await DoAsync().ConfigureAwait(false);
                    await _waitImageBuilder._WaitImageHelper._delay(_waitImageBuilder.DelayStep, _waitImageBuilder._WaitImageHelper.CancellationToken);
                }
            }
            if (_waitImageBuilder._IsThrow) throw new WaitImageTimeoutException(string.Join("|", FindNames));
            return this;
        }

        private Task<OpenCvFindResult> FindOutPointAsync(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop)
        {
            if (_waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindOutPoint(mainBitmap, subBitmap, crop, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindOutPoint(mainBitmap, subBitmap, crop, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }
        private Task<List<OpenCvFindResult>> FindOutPointsAsync(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop)
        {
            if (_waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindOutPoints(mainBitmap, subBitmap, crop, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindOutPoints(mainBitmap, subBitmap, crop, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }

        private Task<bool> TapAsync(WaitImageDataResult waitImageDataResult)
        {
            Task<bool> task = _waitImageBuilder._TapCallbackAsync?.Invoke(waitImageDataResult);
            return task ?? Task.FromResult(false);
        }

        private Task DoAsync()
        {
            Task task = _waitImageBuilder._WorkAsync?.Invoke();
            return task ?? Task.CompletedTask;
        }

        private void DrawDebugRectangle(Bitmap bitmap_capture, IReadOnlyDictionary<string, Rectangle> crops)
        {
            if (_waitImageBuilder._WaitImageHelper._DrawDebugRectangle is not null &&
                _waitImageBuilder._WaitImageHelper._FontFamilyDrawTextDebugRectangle is not null)
            {
                Bitmap bitmap = new Bitmap(bitmap_capture);
                Task.Run(() =>
                {
                    try
                    {
                        WaitImageHelper waitImageHelper = _waitImageBuilder._WaitImageHelper;
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