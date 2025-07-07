using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TqkLibrary.Automation.Images
{
    /// <summary>
    /// 
    /// </summary>
    public static class ImageUtil
    {
        private static readonly int TransparentARGB = Color.Transparent.ToArgb();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Bitmap Resize(this Bitmap source, int newWidth, int newHeight)
        {
            var newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            newImage.MakeTransparent();
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                source.SetResolution(graphics.DpiX, graphics.DpiY);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(source, 0, 0, newWidth, newHeight);
                return newImage;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Bitmap Resize(this Bitmap source, double percent)
        {
            if (percent <= 0) throw new ArgumentException(nameof(percent));
            int newWidth = (int)(source.Width * percent);
            int newHeight = (int)(source.Height * percent);
            var newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            newImage.MakeTransparent();
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                source.SetResolution(graphics.DpiX, graphics.DpiY);

                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(source, 0, 0, newWidth, newHeight);
                return newImage;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start_x"></param>
        /// <param name="start_y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CropImageByPercent(this Bitmap source, double start_x, double start_y, double width, double height)
          => CropImage(source, new Rectangle()
          {
              Location = new Point()
              {
                  X = (int)(source.Width * start_x),
                  Y = (int)(source.Height * start_y),
              },
              Size = new Size()
              {
                  Width = (int)(source.Width * width),
                  Height = (int)(source.Height * height),
              }
          });
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Bitmap CropImage(this Bitmap source, Rectangle rect)
        {
            var newImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            newImage.MakeTransparent();
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                using var bitmap_lock = source.LockHepler();
                source.SetResolution(graphics.DpiX, graphics.DpiY);
                graphics.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);
            }
            return newImage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Bitmap CropImage(this Bitmap source, Size size, Point location) => source.CropImage(new Rectangle() { Size = size, Location = location });
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="child"></param>
        /// <param name="pos"></param>
        public static void DrawChild(this Bitmap target, Bitmap child, Point pos)
        {
            //var newImage = new Bitmap(target.Width, target.Height, PixelFormat.Format24bppRgb);
            //newImage.MakeTransparent();
            //using (Graphics graphics = Graphics.FromImage(newImage))
            //{
            //  target.SetResolution(graphics.DpiX, graphics.DpiY);
            //  graphics.DrawImage(target, 0, 0, target.Width, target.Height);
            //  graphics.DrawImage(child, pos.X, pos.Y, child.Width, child.Height);
            //}
            //return newImage;
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.DrawChild(child, pos);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="point"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="opacity"></param>
        public static void DrawText(this Bitmap target, Point point, string text, Font font, Color color, int opacity = 128)
        {
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.DrawText(point, text, font, color, opacity);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Bitmap RedrawCropToCenterWidth(this Bitmap source, Rectangle rectangle)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);
            result.MakeTransparent();
            using (Bitmap crop = source.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(result))
                {
                    int center = source.Height / 2;
                    int Y_start = center - rectangle.Height / 2;
                    graphics.DrawImage(crop, 0, Y_start, source.Width, crop.Height);
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Point GetCenter(this Rectangle rectangle)
        {
            return new Point(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static Point ImageCenterImage(Size source, Size child)
        {
            var center_source = new Point(source.Width / 2, source.Height / 2);
            var center_child = new Point(child.Width / 2, child.Height / 2);
            return new Point(center_source.X - center_child.X, center_source.Y - center_child.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Bitmap ToBitMap(this byte[] buffer)
        {
            MemoryStream memoryStream = new MemoryStream(buffer);
            return (Bitmap)Bitmap.FromStream(memoryStream);
        }

        #region Graphics Extension
        /// <summary>
        /// 
        /// </summary>
        public static void DrawText(this Graphics graphics, Point point, string text, Font font, Color color, int opacity = 128)
        {
            graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(opacity, color)), point);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void DrawChild(this Graphics graphics, Bitmap child, Point pos)
        {
            graphics.DrawImage(child, pos.X, pos.Y, child.Width, child.Height);
        }

        #endregion Graphics Extension

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static Bitmap Threshold(this Bitmap bitmap, float percent)
        {
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetThreshold(percent);
            Bitmap bmp = new Bitmap(bitmap);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttr);
            return bmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Bmp"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void ToGrayScale(this Bitmap Bmp, double r = .299, double g = .587, double b = .114)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(r * c.R + g * c.G + b * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap MakeGrayscale3(this Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            using (ImageAttributes attributes = new ImageAttributes())
            {

                //set the color matrix attribute
                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }

            return newBitmap;
        }
    }
}