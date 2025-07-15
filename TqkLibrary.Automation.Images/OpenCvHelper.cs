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
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="subBitmap"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindTemplate(Bitmap image, Bitmap template, double percent = 0.9)
        {
            if (template == null || image == null)
                return null;

            Size subBitmapSize = template.Size;
            if (template.Width > image.Width || template.Height > image.Height)
                return null;

            using Image<Bgr, byte> sourceBgr = image.ToImage<Bgr, byte>();
            using Image<Bgr, byte> templateBgr = template.ToImage<Bgr, byte>();

            return FindTemplates(sourceBgr, templateBgr, percent).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static OpenCvFindResult? FindTemplate(Bitmap image, Bitmap template, Rectangle? crop, double percent = 0.9)
        {
            using Bitmap? bitmapCrop = crop.HasValue ? image.CropImage(crop.Value) : null;
            var result = FindTemplate(bitmapCrop ?? image, template, percent);
            if (result is not null && crop.HasValue)
            {
                result.Point = new Point(result.Point.X + crop.Value.X, result.Point.Y + crop.Value.Y);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static IReadOnlyList<OpenCvFindResult> FindTemplates(Bitmap image, Bitmap template, double percent = 0.9)
        {
            List<OpenCvFindResult> results = new List<OpenCvFindResult>();
            if (template == null || image == null)
                return results;

            Size subBitmapSize = template.Size;
            if (template.Width > image.Width || template.Height > image.Height)
                return results;

            using Image<Bgr, byte> imageBgr = image.ToImage<Bgr, byte>();
            using Image<Bgr, byte> templateBgr = template.ToImage<Bgr, byte>();

            return FindTemplates(imageBgr, templateBgr, percent, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="crop"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static IReadOnlyList<OpenCvFindResult> FindTemplates(Bitmap image, Bitmap template, Rectangle? crop, double percent = 0.9)
        {
            using Bitmap? bitmapCrop = crop.HasValue ? image.CropImage(crop.Value) : null;
            IReadOnlyList<OpenCvFindResult> results = FindTemplates(bitmapCrop ?? image, template, percent);
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
        /// <param name="templateMatchingType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IReadOnlyList<OpenCvFindResult> FindTemplates<TColor, TDepth>(
            Image<TColor, TDepth> image,
            Image<TColor, TDepth> template,
            double percent = 0.9,
            bool findAll = false,
            TemplateMatchingType templateMatchingType = TemplateMatchingType.CcoeffNormed
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
                using Image<Gray, float> match = (findAll ? clone! : image).MatchTemplate(template, templateMatchingType);

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
