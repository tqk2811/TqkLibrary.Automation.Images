using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using TqkLibrary.Automation.Images;

namespace TqkLibrary.Automation.Images.Test
{
    [TestClass]
    public class OpenCvHelperTest
    {
        [TestMethod]
        public void FindOutPoint()
        {
            using Bitmap baseImage = (Bitmap)Bitmap.FromFile(".\\Resources\\baseImage.png");
            using Bitmap searchImage = (Bitmap)Bitmap.FromFile(".\\Resources\\searchImage.png");
            var point = OpenCvHelper.FindOutPoints(baseImage, searchImage, 0.9);
        }

        [TestMethod]
        public void ThressHold()
        {
            var from = (Bitmap)Bitmap.FromFile(".\\Resources\\baseImage.png");
            ImageUtil.ToGrayScale(from);
            var to = ImageUtil.Threshold(from, 0.1f);
            from.Save(".\\ToGrayScale.png", ImageFormat.Png);
            to.Save(".\\Threshold.png", ImageFormat.Png);
        }
    }
}
