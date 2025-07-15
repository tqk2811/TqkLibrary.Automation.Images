using Emgu.CV;
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
    public abstract class WaitImageResult
    {
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<WaitImageDataResult> Points { get { return _points; } }
        protected readonly List<WaitImageDataResult> _points = new List<WaitImageDataResult>();
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<string> FindNames { get; protected set; } = new List<string>();


        protected readonly WaitImageBuilder _waitImageBuilder;
        internal WaitImageResult(WaitImageBuilder waitImageBuilder)
        {
            this._waitImageBuilder = waitImageBuilder ?? throw new ArgumentNullException(nameof(waitImageBuilder));
        }
        protected Task<Bitmap> CaptureAsync() => _waitImageBuilder.GetCaptureAsync();
        protected string _lastFound = string.Empty;


        abstract internal Task<WaitImageResult> StartAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageResult<TColor, TDepth> : WaitImageResult
            where TColor : struct, IColor
            where TDepth : new()
    {
        internal WaitImageResult(WaitImageBuilder waitImageBuilder) : base(waitImageBuilder)
        {

        }

        internal override async Task<WaitImageResult> StartAsync()
        {
            FindNames = _waitImageBuilder.WaitImageHelper.GlobalNameFindFirst
                .Concat(_waitImageBuilder.Finds)
                .Concat(_waitImageBuilder.WaitImageHelper.GlobalNameFindLast)
                .ToArray();

            _waitImageBuilder.WaitImageHelper.WriteLog($"{_waitImageBuilder.WaitMode.ToString()} : {string.Join(",", FindNames)}");
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(_waitImageBuilder.GetTimeout))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    _waitImageBuilder.WaitImageHelper.CancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, Rectangle> dict_crops = new Dictionary<string, Rectangle>();

                    using Bitmap bitmap_capture = await CaptureAsync().ConfigureAwait(false);
                    if (bitmap_capture == null) throw new NullReferenceException(nameof(bitmap_capture));

                    var findNamesFilter = _waitImageBuilder.ImageNamesFilter?.Invoke(FindNames, _lastFound).ToList() ?? FindNames;
                    if (!Enumerable.SequenceEqual(findNamesFilter, FindNames))
                    {
                        _waitImageBuilder.WaitImageHelper.WriteLog($"{_waitImageBuilder.WaitMode.ToString()} (filter): {string.Join(",", findNamesFilter)}");
                    }

                    using Image<TColor, TDepth> image_capture = bitmap_capture.ToImage<TColor, TDepth>();
                    for (int i = 0; i < findNamesFilter.Count; i++)
                    {
                        Rectangle? crop = _waitImageBuilder.WaitImageHelper.Crop?.Invoke(findNamesFilter[i]);
                        if (crop.HasValue && !dict_crops.ContainsKey(findNamesFilter[i]))
                        {
                            dict_crops.Add(findNamesFilter[i], crop.Value);
                        }
                        using Image<TColor, TDepth> image_capture_crop = crop.HasValue ? image_capture.Copy(crop.Value) : image_capture;


                        for (int j = 0; ; j++)
                        {
                            using Bitmap bitmap_template = _waitImageBuilder.GetTemplate(findNamesFilter[i], j);
                            if (bitmap_template == null) break;
                            using Image<TColor, TDepth> image_template = bitmap_template.ToImage<TColor, TDepth>();



                            if (_waitImageBuilder.Tapflag == TapFlag.First)
                            {
                                OpenCvFindResult? result = await FindTemplateAsync(image_capture_crop, image_template).ConfigureAwait(false);

                                if (result != null)
                                {
                                    if (crop.HasValue)
                                    {
                                        result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
                                    }
                                    WaitImageDataResult dataResult = new WaitImageDataResult(findNamesFilter[i], result);
                                    _points.Add(dataResult);
                                    _waitImageBuilder.WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {result}");
                                    _lastFound = findNamesFilter[i];
                                    ActionShould tapActionShould = await TapAsync(dataResult).ConfigureAwait(false);
                                    if (tapActionShould == ActionShould.Continue)
                                    {
                                        //reset to while
                                        if (_waitImageBuilder.IsResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        i = findNamesFilter.Count;//break i
                                        break;//break j
                                    }
                                    else return this;
                                }
                            }
                            else
                            {
                                var results = await FindTemplatesAsync(image_capture_crop, image_template);
                                if (results.Count > 0)
                                {
                                    if (crop.HasValue)
                                    {
                                        foreach (var result in results)
                                        {
                                            result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
                                        }
                                    }

                                    _waitImageBuilder.WaitImageHelper.WriteLog($"Found: {findNamesFilter[i]}{j} {results.Count} points ({string.Join("|", results)})");
                                    _lastFound = findNamesFilter[i];

                                    _points.AddRange(results.Select(x => new WaitImageDataResult(findNamesFilter[i], x)));
                                }
                            }
                        }
                    }

                    if (_waitImageBuilder.Tapflag != TapFlag.First && _points.Count > 0)
                    {
                        switch (_waitImageBuilder.Tapflag)
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
                                        if (_waitImageBuilder.IsResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
                                        if (_waitImageBuilder.WaitMode == WaitMode.WaitUntil) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = _waitImageBuilder.WaitImageHelper._Random.Next(_points.Count);
                                    ActionShould tapActionShould = await TapAsync(_points[random_index]).ConfigureAwait(false);
                                    if (tapActionShould == ActionShould.Continue)
                                    {
                                        if (_waitImageBuilder.IsResetTimeout) cancellationTokenSource.CancelAfter(_waitImageBuilder.GetTimeout);
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
                    await _waitImageBuilder.WaitImageHelper.Delay(_waitImageBuilder.GetDelayStep, _waitImageBuilder.WaitImageHelper.CancellationToken);
                }
            }
            if (_waitImageBuilder.IsThrow) throw new WaitImageTimeoutException(string.Join("|", FindNames));
            return this;
        }

        private async Task<OpenCvFindResult?> FindTemplateAsync(Image<TColor, TDepth> image, Image<TColor, TDepth> template)
        {
            var results = await FindTemplatesAsync(image, template, false);
            return results.FirstOrDefault();
        }
        private Task<IReadOnlyList<OpenCvFindResult>> FindTemplatesAsync(Image<TColor, TDepth> image, Image<TColor, TDepth> template, bool findAll = true)
        {
            if (_waitImageBuilder.WaitImageHelper.FindInThreadPool)
            {
                return Task.Run(() => OpenCvHelper.FindTemplates(image, template, _waitImageBuilder.WaitImageHelper.MatchRate.Invoke(), findAll));
            }
            else
            {
                return Task.FromResult(OpenCvHelper.FindTemplates(image, template, _waitImageBuilder.WaitImageHelper.MatchRate.Invoke(), findAll));
            }
        }

        private Task<ActionShould> TapAsync(WaitImageDataResult waitImageDataResult)
        {
            Task<ActionShould>? task = _waitImageBuilder.TapCallbackAsync?.Invoke(waitImageDataResult);
            return task ?? Task.FromResult(ActionShould.Break);
        }

        private Task DoAsync()
        {
            Task? task = _waitImageBuilder.BeforeFindAsync?.Invoke();
            return task ?? Task.CompletedTask;
        }

        private void DrawDebugRectangle(Bitmap bitmap_capture, IReadOnlyDictionary<string, Rectangle> crops)
        {
            if (_waitImageBuilder.WaitImageHelper.DrawDebugRectangle is not null &&
                _waitImageBuilder.WaitImageHelper.FontFamilyDrawTextDebugRectangle is not null)
            {
                Bitmap bitmap = new Bitmap(bitmap_capture);
                Task.Run(() =>
                {
                    try
                    {
                        WaitImageHelper waitImageHelper = _waitImageBuilder.WaitImageHelper;
                        using Pen pen = new Pen(waitImageHelper.ColorDrawDebugRectangle);
                        using Brush text_brush = new SolidBrush(waitImageHelper.ColorDrawDebugRectangle);
                        using Font font = new Font(waitImageHelper.FontFamilyDrawTextDebugRectangle, waitImageHelper.ColorDrawDebugFontEmSize);
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
                        waitImageHelper.DrawDebugRectangle.Invoke(bitmap);
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