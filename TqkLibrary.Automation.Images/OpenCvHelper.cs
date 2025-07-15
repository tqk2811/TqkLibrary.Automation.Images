using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
        public static OpenCvFindResult? FindTemplate(Bitmap mainBitmap, Bitmap subBitmap, double percent = 0.9)
        {
            if (subBitmap == null || mainBitmap == null)
                return null;

            Size subBitmapSize = subBitmap.Size;
            if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                return null;

            using Image<Bgr, byte> source = mainBitmap.ToImage<Bgr, byte>();
            using Image<Bgr, byte> template = subBitmap.ToImage<Bgr, byte>();

            return FindTemplates(source, template, percent).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindTemplate(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop, double percent = 0.9)
        {
            using Bitmap? bitmapCrop = crop.HasValue ? mainBitmap.CropImage(crop.Value) : null;
            var result = FindTemplate(bitmapCrop ?? mainBitmap, subBitmap, percent);
            if (result is not null && crop.HasValue)
            {
                result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static List<OpenCvFindResult> FindTemplates(Bitmap mainBitmap, Bitmap subBitmap, double percent = 0.9)
        {
            List<OpenCvFindResult> results = new List<OpenCvFindResult>();
            if (subBitmap == null || mainBitmap == null)
                return results;

            Size subBitmapSize = subBitmap.Size;
            if (subBitmap.Width > mainBitmap.Width || subBitmap.Height > mainBitmap.Height)
                return results;

            using Image<Bgr, byte> source = mainBitmap.ToImage<Bgr, byte>();
            using Image<Bgr, byte> template = subBitmap.ToImage<Bgr, byte>();

            return FindTemplates(source, template, percent, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBitmap"></param>
        /// <param name="subBitmap"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static List<OpenCvFindResult> FindTemplates(Bitmap mainBitmap, Bitmap subBitmap, Rectangle? crop, double percent = 0.9)
        {
            using Bitmap? bitmapCrop = crop.HasValue ? mainBitmap.CropImage(crop.Value) : null;
            List<OpenCvFindResult> results = FindTemplates(bitmapCrop ?? mainBitmap, subBitmap, percent);
            if (crop.HasValue)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    results[i].Point = new Point(results[i].Point.X + crop.Value.X, results[i].Point.Y + crop.Value.Y);
                }
            }
            return results;
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
        public static List<OpenCvFindResult> FindTemplates<TColor, TDepth>(
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
