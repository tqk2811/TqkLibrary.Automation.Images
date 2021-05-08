using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
  }
}
