using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using TqkLibrary.Media.Images;

namespace TqkLibrary.Media.Images.Test
{
  [TestClass]
  public class OpenCvHelperTest
  {
    [TestMethod]
    public void FindOutPoint()
    {
      var point = OpenCvHelper.FindOutPoint(Resource.baseImage, Resource.searchImage, 0.9);
    }

    [TestMethod]
    public void ThressHold()
    {
      var from = (Bitmap)Bitmap.FromFile(@"C:\Users\tqk28\Downloads\test.jpg");
      ImageUtil.ToGrayScale(from);
      var to = ImageUtil.Threshold(from, 0.1f);
      from.Save("D:\\ToGrayScale.png", ImageFormat.Png);
      to.Save("D:\\Threshold.png", ImageFormat.Png);
    }
  }
}
