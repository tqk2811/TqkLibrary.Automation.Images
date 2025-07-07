using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;

namespace TqkLibrary.Automation.Images
{
    /// <summary>
    /// 
    /// </summary>
    public static class OpenCvHelper
    {
        static Image<Bgr, byte> ToImageLockHelper(this Bitmap bitmap)
        {
            using var lock_obj = bitmap.LockHepler();
            return bitmap.ToImage<Bgr, byte>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindOutPoint(Bitmap mainBitmap, Bitmap subBitmap, double percent = 0.9)
        {
            if (subBitmap == null || mainBitmap == null)
                return null;

            Size subBitmapSize;
            using (var main = mainBitmap.LockHepler())
            {
                using var sub = subBitmap.LockHepler();
                subBitmapSize = subBitmap.Size;
                if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                    return null;
            }

            using Image<Bgr, byte> source = mainBitmap.ToImageLockHelper();
            using Image<Bgr, byte> template = subBitmap.ToImageLockHelper();
            Point? resPoint = null;
            double currentMax = 0;

            using (Image<Gray, float> match = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                match.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                for (int i = 0; i < maxValues.Length; i++)
                {
                    if (maxValues[i] > percent && maxValues[i] > currentMax)
                    {
                        currentMax = maxValues[i];
                        resPoint = maxLocations[i];
                    }
                }
            }
            if (resPoint != null)
            {
                return new OpenCvFindResult()
                {
                    Point = new Point(resPoint.Value.X + subBitmapSize.Width / 2, resPoint.Value.Y + subBitmapSize.Height / 2),//center
                    Percent = currentMax
                };
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindOutPoint(Bitmap mainBitmap, Bitmap subBitmap, Rectangle crop, double percent = 0.9)
        {
            using Bitmap bm_crop = mainBitmap.CropImage(crop);
            OpenCvFindResult? result = FindOutPoint(bm_crop, subBitmap, percent);
            if (result != null)
            {
                Point subpoint = result.Point;//that was center crop
                subpoint.X += crop.X;
                subpoint.Y += crop.Y;
                result.Point = subpoint;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindOutPoint(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop, double percent = 0.9)
        {
            if (crop == null) return FindOutPoint(mainBitmap, subBitmap, percent);
            else return FindOutPoint(mainBitmap, subBitmap, crop.Value, percent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static List<OpenCvFindResult> FindOutPoints(Bitmap mainBitmap, Bitmap subBitmap, double percent = 0.9)
        {
            List<OpenCvFindResult> results = new List<OpenCvFindResult>();
            if (subBitmap == null || mainBitmap == null)
                return results;

            Size subBitmapSize;
            using (var main = mainBitmap.LockHepler())
            {
                using var sub = subBitmap.LockHepler();
                subBitmapSize = subBitmap.Size;
                if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                    return results;
            }


            using Image<Bgr, byte> source = mainBitmap.ToImageLockHelper();
            using Image<Bgr, byte> template = subBitmap.ToImageLockHelper();
            while (true)
            {
                using (Image<Gray, float> match = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    match.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if (maxValues[0] > percent)
                    {
                        Rectangle rect = new Rectangle(maxLocations[0], template.Size);
                        source.Draw(rect, new Bgr(Color.Blue), -1);
                        Point center = new Point(maxLocations[0].X + subBitmapSize.Width / 2, maxLocations[0].Y + subBitmapSize.Height / 2);
                        results.Add(new OpenCvFindResult()
                        {
                            Point = center,
                            Percent = maxValues[0]
                        });
                    }
                    else break;
                }
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static List<OpenCvFindResult> FindOutPoints(Bitmap mainBitmap, Bitmap subBitmap, Rectangle crop, double percent = 0.9)
        {
            using Bitmap bm_crop = mainBitmap.CropImage(crop);
            List<OpenCvFindResult> results = FindOutPoints(bm_crop, subBitmap, percent);
            for (int i = 0; i < results.Count; i++)
            {
                Point temp = results[i].Point;//that was center crop
                temp.X += crop.X;
                temp.Y += crop.Y;
                results[i].Point = temp;
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static List<OpenCvFindResult> FindOutPoints(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop, double percent = 0.9)
        {
            if (crop == null) return FindOutPoints(mainBitmap, subBitmap, percent);
            else return FindOutPoints(mainBitmap, subBitmap, crop.Value, percent);
        }
        //public static Bitmap CropNonTransparent(Bitmap bitmap)
        //{
        //  using Image<Bgra, byte> imageIn = bitmap.ToImage<Bgra, byte>();
        //  using Mat mat = new Mat(/*imageIn.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1*/);
        //  CvInvoke.FindNonZero(imageIn, mat);
        //  return mat.ToBitmap();
        //}
    }
}
