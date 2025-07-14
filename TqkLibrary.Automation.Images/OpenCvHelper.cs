using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace TqkLibrary.Automation.Images
{
    /// <summary>
    /// 
    /// </summary>
    public static class OpenCvHelper
    {
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

            Size subBitmapSize = subBitmap.Size;
            if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                return null;

            using Image<Bgr, byte> source = mainBitmap.ToImage<Bgr, byte>();
            using Image<Bgr, byte> template = subBitmap.ToImage<Bgr, byte>();
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

            Size subBitmapSize = subBitmap.Size;
            if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                return results;


            using Image<Bgr, byte> source = mainBitmap.ToImage<Bgr, byte>();
            using Image<Bgr, byte> template = subBitmap.ToImage<Bgr, byte>();
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










        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TColor"></typeparam>
        /// <typeparam name="TDepth"></typeparam>
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="percent"></param>
        /// <param name="findAll"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static List<OpenCvFindResult> FindOutPoints<TColor, TDepth>(
            Image<TColor, TDepth> image,
            Image<TColor, TDepth> template,
            double percent = 0.9,
            bool findAll = false
            )
            where TColor : struct, IColor
            where TDepth : new()
        {
            List<OpenCvFindResult> results = new List<OpenCvFindResult>();

            if (image is null)
                throw new ArgumentNullException(nameof(image));
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            if (template.Width > image.Width || template.Height > image.Height)
                return results;

            using Image<TColor, TDepth>? clone = findAll ? image.Clone() : null;
            while (findAll)
            {
                using Image<Gray, float> match = (findAll ? clone! : image).MatchTemplate(template, TemplateMatchingType.CcoeffNormed);

                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                match.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > percent)
                {
                    Rectangle rect = new Rectangle(maxLocations[0], template.Size);
                    if (findAll)
                    {
                        clone!.Draw(rect, new TColor(), -1);
                    }
                    Point center = new Point(maxLocations[0].X + template.Width / 2, maxLocations[0].Y + template.Height / 2);
                    results.Add(new OpenCvFindResult()
                    {
                        Point = center,
                        Percent = maxValues[0]
                    });
                }
                else break;
            }
            return results;
        }



    }
}
