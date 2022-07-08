using System;
using System.Collections.Generic;
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
        public IEnumerable<Tuple<int, OpenCvFindResult>> Points { get { return _points; } }
        readonly List<Tuple<int, OpenCvFindResult>> _points = new List<Tuple<int, OpenCvFindResult>>();
        readonly WaitImageBuilder waitImageBuilder;
        internal WaitImageResult(WaitImageBuilder waitImageBuilder)
        {
            this.waitImageBuilder = waitImageBuilder;
        }

        internal async Task<WaitImageResult> StartAsync()
        {
            var advFinds = waitImageBuilder.waitImageHelper.GlobalImageNameFind?.Invoke();
            var _finds = waitImageBuilder.Finds.ToList();
            if (advFinds != null) _finds.AddRange(advFinds);
            var finds = _finds.ToArray();

            waitImageBuilder.waitImageHelper.WriteLog((waitImageBuilder.IsLoop ? "WaitUntil: " : "FindImage: ") + string.Join(",", finds));
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(
                waitImageBuilder.Timeout.HasValue ? waitImageBuilder.Timeout.Value : waitImageBuilder.waitImageHelper.Timeout.Invoke()))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitImageBuilder.waitImageHelper.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    using Bitmap image = waitImageBuilder.waitImageHelper.Capture.Invoke();
                    if (image == null) throw new NullReferenceException(nameof(image));
                    for (int i = 0; i < finds.Length; i++)
                    {
                        for (int j = 0; ; j++)
                        {
                            using Bitmap find = waitImageBuilder.waitImageHelper.ImageFind?.Invoke(finds[i], j);
                            if (find == null) break;
                            Rectangle? crop = waitImageBuilder.waitImageHelper.Crop?.Invoke(finds[i]);
                            if (waitImageBuilder.IsFirst)
                            {
                                OpenCvFindResult result = null;
                                if (waitImageBuilder.waitImageHelper.FindInThreadPool) result = await Task.Run(() => OpenCvHelper.FindOutPoint(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke()));
                                else result = OpenCvHelper.FindOutPoint(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke());
                                if (result != null)
                                {
                                    _points.Add(new Tuple<int, OpenCvFindResult>(i, result));
                                    waitImageBuilder.waitImageHelper.WriteLog($"Found: {finds[i]}{j} {result}");

                                    if (await TapAsync(i, result, finds))
                                    {
                                        //reset to while
                                        if (waitImageBuilder.ResetTimeout) cancellationTokenSource.CancelAfter(
                                            waitImageBuilder.Timeout.HasValue ? waitImageBuilder.Timeout.Value : waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        i = finds.Length;//break i
                                        break;//break j
                                    }
                                    else return this;
                                }
                            }
                            else
                            {
                                var points = OpenCvHelper.FindOutPoints(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke());
                                if (points.Count > 0)
                                {
                                    waitImageBuilder.waitImageHelper.WriteLog($"Found: {finds[i]}{j} {points.Count} points ({string.Join("|", points)})");
                                    _points.AddRange(points.Select(x => new Tuple<int, OpenCvFindResult>(i, x)));
                                }
                            }

                        }
                    }

                    if (!waitImageBuilder.IsFirst && _points.Count > 0)
                    {
                        switch (waitImageBuilder.Tapflag)
                        {
                            case TapFlag.All:
                                {
                                    List<bool> results = new List<bool>();
                                    foreach (var point in _points)
                                    {
                                        results.Add(await TapAsync(point.Item1, point.Item2, finds));
                                    }
                                    if (results.All(x => x))
                                    {
                                        if (waitImageBuilder.ResetTimeout) cancellationTokenSource.CancelAfter(
                                            waitImageBuilder.Timeout.HasValue ? waitImageBuilder.Timeout.Value : waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        if (waitImageBuilder.IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = waitImageBuilder.waitImageHelper.random.Next(_points.Count);
                                    if (await TapAsync(_points[random_index].Item1,
                                        _points[random_index].Item2,
                                        finds))
                                    {
                                        if (waitImageBuilder.ResetTimeout) cancellationTokenSource.CancelAfter(
                                            waitImageBuilder.Timeout.HasValue ? waitImageBuilder.Timeout.Value : waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        if (waitImageBuilder.IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                        }
                    }

                    if (!waitImageBuilder.IsLoop) break;
                    await DoAsync();
                    await Task.Delay(waitImageBuilder.waitImageHelper.DelayStep, waitImageBuilder.waitImageHelper.CancellationTokenSource.Token);
                }
            }
            if (waitImageBuilder.IsThrow) throw new WaitImageTimeoutException(string.Join("|", finds));
            return this;
        }


        private async Task<bool> TapAsync(int index, OpenCvFindResult point, string[] finds)
        {
            if (waitImageBuilder.TapCallback != null)
            {
                return waitImageBuilder.TapCallback.Invoke(index, point, finds);
            }
            else if (waitImageBuilder.TapCallbackAsync != null)
            {
                return await waitImageBuilder.TapCallbackAsync.Invoke(index, point, finds).ConfigureAwait(false);
            }
            return false;
        }

        private async Task DoAsync()
        {
            if (waitImageBuilder.Work != null)
            {
                waitImageBuilder.Work.Invoke();
            }
            else if (waitImageBuilder.WorkAsync != null)
            {
                await waitImageBuilder.WorkAsync.Invoke().ConfigureAwait(false);
            }
        }
    }
}
