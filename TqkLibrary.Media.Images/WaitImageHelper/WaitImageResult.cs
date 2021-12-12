using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    public class WaitImageResult
    {
        public IEnumerable<Tuple<int, Point>> Points { get { return _points; } }
        readonly List<Tuple<int, Point>> _points = new List<Tuple<int, Point>>();
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
                                    _points.Add(new Tuple<int, Point>(i, point.Value));
                                    waitImageBuilder.waitImageHelper.WriteLog($"Found: {waitImageBuilder.Finds[i]}{j} {point.Value}");

                                    if (Tap(i, point.Value, waitImageBuilder.Finds))
                                    {
                                        //reset to while
                                        cancellationTokenSource.CancelAfter(waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        i = waitImageBuilder.Finds.Length;
                                        break;
                                    }
                                    else return this;
                                }
                            }
                            else
                            {
                                var points = OpenCvHelper.FindOutPoints(image, find, crop, waitImageBuilder.waitImageHelper.Percent.Invoke());
                                if(points.Count > 0)
                                {
                                    waitImageBuilder.waitImageHelper.WriteLog($"Found: {waitImageBuilder.Finds[i]}{j} {points.Count} points ({string.Join("|",points)})");
                                    _points.AddRange(points.Select(x => new Tuple<int, Point>(i, x)));
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
                                    if(_points.Select(x => Tap(x.Item1, x.Item2, waitImageBuilder.Finds)).All(x => x))
                                    {
                                        cancellationTokenSource.CancelAfter(waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        if (waitImageBuilder.IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                            case TapFlag.Random:
                                {
                                    int random_index = waitImageBuilder.waitImageHelper.random.Next(_points.Count);
                                    if(Tap(_points[random_index].Item1,
                                        _points[random_index].Item2,
                                        waitImageBuilder.Finds))
                                    {
                                        cancellationTokenSource.CancelAfter(waitImageBuilder.waitImageHelper.Timeout.Invoke());
                                        if (waitImageBuilder.IsLoop) _points.Clear();
                                        break;
                                    }
                                    else return this;
                                }
                        }
                    }

                    if (!waitImageBuilder.IsLoop) break;
                    Task.Delay(waitImageBuilder.waitImageHelper.DelayStep).Wait(waitImageBuilder.waitImageHelper.CancellationTokenSource.Token);
                }
            }
            if (waitImageBuilder.IsThrow) throw new WaitImageTimeoutException(string.Join("|", waitImageBuilder.Finds));
            return this;
        }


        private bool Tap(int index, Point point, string[] finds)
        {
            return waitImageBuilder.TapCallback?.Invoke(index, point, finds) == true;
        }
    }
}
