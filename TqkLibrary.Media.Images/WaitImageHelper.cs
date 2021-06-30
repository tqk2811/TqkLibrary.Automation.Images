using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
  public delegate void TapCallback(Point point);
  public class WaitImageHelper : IDisposable
  {
    public double Percent { get; set; } = 0.9;
    public int Timeout { get; set; } = 60000;
    public CancellationTokenSource CancellationTokenSource { get; }
    internal readonly Func<Bitmap> image;
    internal readonly Func<string, int, Bitmap> find;
    internal readonly TapCallback tap;
    internal readonly Random random = new Random(DateTime.Now.Millisecond);
    public WaitImageHelper(Func<Bitmap> image, Func<string, int, Bitmap> find, TapCallback tap)
    {
      this.image = image ?? throw new ArgumentNullException(nameof(image));
      this.find = find ?? throw new ArgumentNullException(nameof(find));
      this.tap = tap ?? throw new ArgumentNullException(nameof(tap));
      CancellationTokenSource = new CancellationTokenSource();
    }

    public WaitImageBuilder WaitUntil(params string[] finds)
    {
      return new WaitImageBuilder(this, finds);
    }

    public void Dispose()
    {
      CancellationTokenSource.Dispose();
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
      this.finds = finds ?? throw new ArgumentNullException(nameof(finds));
      if(finds.Length == 0) throw new ArgumentNullException(nameof(finds));
      this.waitImageHelper = waitImageHelper;
    }
    internal readonly string[] finds;
    internal bool IsTap = false;
    internal TapFlag tapflag = TapFlag.None;
    internal bool IsThrow = false;
    internal bool IsAny = true;

    public WaitImageBuilder AndTapFirst()
    {
      IsAny = true;
      IsTap = true;
      tapflag = TapFlag.First;
      return this;
    }

    public WaitImageBuilder AndTapRandom()
    {
      IsAny = false;
      IsTap = true;
      tapflag = TapFlag.Random;
      return this;
    }
    public WaitImageBuilder AndTapAll()
    {
      IsAny = false;
      IsTap = true;
      tapflag = TapFlag.All;
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
    readonly WaitImageBuilder waitImageBuilder;
    internal WaitImageResult(WaitImageBuilder waitImageBuilder)
    {
      this.waitImageBuilder = waitImageBuilder;
    }
    public int IndexArg { get; private set; } = -1;
    internal WaitImageResult Start()
    {
      using(CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(waitImageBuilder.waitImageHelper.Timeout))
      {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
          waitImageBuilder.waitImageHelper.CancellationTokenSource.Token.ThrowIfCancellationRequested();
          using Bitmap image = waitImageBuilder.waitImageHelper.image.Invoke();
          List<Point> points = new List<Point>();
          for (int i = 0; i < waitImageBuilder.finds.Length; i++)
          {
            for (int j = 0; ; j++)
            {
              using Bitmap find = waitImageBuilder.waitImageHelper.find(waitImageBuilder.finds[i], j);
              if (find == null) break;
              if (waitImageBuilder.IsAny)
              {
                Point? point = OpenCvHelper.FindOutPoint(image, find, waitImageBuilder.waitImageHelper.Percent);
                if (point != null)
                {
                  IndexArg = i;
                  return Tap(point.Value);
                }
              }
              else
              {
                points.AddRange(OpenCvHelper.FindOutPoints(image, find, waitImageBuilder.waitImageHelper.Percent));
              }
            }
          }

          if (!waitImageBuilder.IsAny && points.Count > 0) 
            switch (waitImageBuilder.tapflag)
            {
              case TapFlag.All:
                {
                  foreach(var point in points) Tap(point);
                  return this;
                }
              case TapFlag.Random:
                {
                  return Tap(points[waitImageBuilder.waitImageHelper.random.Next(points.Count)]);
                }
            }
        }
      }
      if (waitImageBuilder.IsThrow) throw new WaitImageTimeoutException(string.Join("|", waitImageBuilder.finds));
      return this;
    }


    private WaitImageResult Tap(Point point)
    {
      waitImageBuilder.waitImageHelper.tap?.Invoke(point);
      return this;
    }

    public class WaitImageTimeoutException: Exception
    {
      internal WaitImageTimeoutException(string message) : base(message)
      {
      }
    }
  }
}
