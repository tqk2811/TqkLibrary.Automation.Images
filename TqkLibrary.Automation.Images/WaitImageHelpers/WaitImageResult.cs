using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Automation.Images.WaitImageHelpers.Enums;

namespace TqkLibrary.Automation.Images.WaitImageHelpers
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

            _waitImageBuilder._WaitImageHelper.WriteLog($"{_waitImageBuilder.WaitMode.ToString()} : {string.Join(",", FindNames)}");
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
                        _waitImageBuilder._WaitImageHelper.WriteLog($"{_waitImageBuilder.WaitMode.ToString()} (filter): {string.Join(",", findNamesFilter)}");
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
                            using Bitmap bitmap_capture_crop = crop.HasValue ? bitmap_capture.CropImage(crop.Value) : bitmap_capture;

                            if (_waitImageBuilder._Tapflag == TapFlag.First)
                            {
                                OpenCvFindResult? result = await FindTemplateAsync(bitmap_capture_crop, bitmap_template).ConfigureAwait(false);

                                if (result != null)
                                {
                                    if (crop.HasValue)
                                    {
                                        result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
                                    }
                                    WaitImageDataResult dataResult = new WaitImageDataResult(findNamesFilter[i], result);
                                    _points.Add(dataResult);
                                    _waitImageBuilder._WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {result}");
                                    _lastFound = findNamesFilter[i];
                                    ActionShould tapActionShould = await TapAsync(dataResult).ConfigureAwait(false);
                                    if (tapActionShould == ActionShould.Continue)
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
                                var results = await FindTemplatesAsync(bitmap_capture_crop, bitmap_template);
                                if (results.Count > 0)
                                {
                                    if (crop.HasValue)
                                    {
                                        foreach (var result in results)
                                        {
                                            result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
                                        }
                                    }

                                    _waitImageBuilder._WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {results.Count} points ({string.Join("|", results)})");
                                    _lastFound = findNamesFilter[i];

                                    _points.AddRange(results.Select(x => new WaitImageDataResult(findNamesFilter[i], x)));
                                }
                            }
                        }
                    }

                    if (_waitImageBuilder._Tapflag != TapFlag.First && _points.Count > 0)
                    {
                        switch (_waitImageBuilder._Tapflag)
                        {
                            case TapFlag.All:
                                {
                                    List<ActionShould> results = new List<ActionShould>();
                                    foreach (var point in _points)
                                    {
                                        results.Add(await TapAsync(point).ConfigureAwait(false));
                                    }
                                    if (results.All(x => x == ActionShould.Continue))
                                    {
                                        if (_waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        if (_waitImageBuilder.WaitMode == WaitMode.WaitUntil) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = _waitImageBuilder._WaitImageHelper._Random.Next(_points.Count);
                                    ActionShould tapActionShould = await TapAsync(_points[random_index]).ConfigureAwait(false);
                                    if (tapActionShould == ActionShould.Continue)
                                    {
                                        if (_waitImageBuilder._ResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        if (_waitImageBuilder.WaitMode == WaitMode.WaitUntil) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                        }
                    }

                    DrawDebugRectangle(bitmap_capture, dict_crops);

                    if (_waitImageBuilder.WaitMode == WaitMode.FindImage) break;
                    await DoAsync().ConfigureAwait(false);
                    await _waitImageBuilder._WaitImageHelper._delay(_waitImageBuilder.DelayStep, _waitImageBuilder._WaitImageHelper.CancellationToken);
                }
            }
            if (_waitImageBuilder._IsThrow) throw new WaitImageTimeoutException(string.Join("|", FindNames));
            return this;
        }

        private Task<OpenCvFindResult?> FindTemplateAsync(Bitmap mainBitmap, Bitmap subBitmap)
        {
            if (_waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindTemplate(mainBitmap, subBitmap, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindTemplate(mainBitmap, subBitmap, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }
        private Task<List<OpenCvFindResult>> FindTemplatesAsync(Bitmap mainBitmap, Bitmap subBitmap)
        {
            if (_waitImageBuilder._WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindTemplates(mainBitmap, subBitmap, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindTemplates(mainBitmap, subBitmap, _waitImageBuilder._WaitImageHelper._MatchRate.Invoke()));
            }
        }

        private Task<ActionShould> TapAsync(WaitImageDataResult waitImageDataResult)
        {
            Task<ActionShould>? task = _waitImageBuilder._TapCallbackAsync?.Invoke(waitImageDataResult);
            return task ?? Task.FromResult(ActionShould.Break);
        }

        private Task DoAsync()
        {
            Task? task = _waitImageBuilder._BeforeFindAsync?.Invoke();
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