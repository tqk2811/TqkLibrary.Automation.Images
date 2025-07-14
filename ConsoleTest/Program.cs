// See https://aka.ms/new-console-template for more information
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using TqkLibrary.Automation.Images;


using Bitmap baseBitmap = (Bitmap)Bitmap.FromFile(".\\Resources\\baseImage.png");
using Bitmap searchBitmap = (Bitmap)Bitmap.FromFile(".\\Resources\\searchImage.png");

using var baseImage = baseBitmap.ToImage<Gray, byte>();
using var searchImage = searchBitmap.ToImage<Gray, byte>();
AkazeMatchTemplate(baseImage, searchImage, 0.99);






static void OrbMatchTemplate<TColor, TDepth>(
    Image<TColor, TDepth> imgScene,
    Image<TColor, TDepth> imgTemplate,
    double LowesRatioThreshold = 0.75
    )
    where TColor : struct, IColor
    where TDepth : new()
{
    using ORB orb = new ORB(5000);
    using VectorOfKeyPoint kpTemplate = new VectorOfKeyPoint();
    using VectorOfKeyPoint kpScene = new VectorOfKeyPoint();

    using Mat descTemplate = new Mat();
    using Mat descScene = new Mat();

    orb.DetectAndCompute(imgTemplate, null, kpTemplate, descTemplate, false);
    orb.DetectAndCompute(imgScene, null, kpScene, descScene, false);

    Console.WriteLine($"Template Keypoints: {kpTemplate.Size}");
    Console.WriteLine($"Scene Keypoints: {kpScene.Size}");

    using BFMatcher matcher = new BFMatcher(DistanceType.Hamming, crossCheck: false);
    using VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
    matcher.KnnMatch(descTemplate, descScene, matches, 2);

    using VectorOfDMatch goodMatches = new VectorOfDMatch();
    for (int i = 0; i < matches.Size; i++)
    {
        var matchSet = matches[i];
        if (matchSet.Size >= 2)
        {
            var m1 = matchSet[0];
            var m2 = matchSet[1];
            if (m1.Distance < LowesRatioThreshold * m2.Distance)
            {
                goodMatches.Push(new[] { m1 });
            }
        }
    }

    // Nếu đủ match tốt, tính homography
    if (goodMatches.Size >= 4)
    {
        var srcPoints = new PointF[goodMatches.Size];
        var dstPoints = new PointF[goodMatches.Size];

        for (int i = 0; i < goodMatches.Size; i++)
        {
            var match = goodMatches[i];
            srcPoints[i] = kpTemplate[match.QueryIdx].Point;
            dstPoints[i] = kpScene[match.TrainIdx].Point;
        }

        Mat homography = CvInvoke.FindHomography(srcPoints, dstPoints, RobustEstimationAlgorithm.Ransac, 5);

        if (!homography.IsEmpty)
        {
            // Góc template
            PointF[] corners = new PointF[]
            {
                        new PointF(0, 0),
                        new PointF(imgTemplate.Width, 0),
                        new PointF(imgTemplate.Width, imgTemplate.Height),
                        new PointF(0, imgTemplate.Height)
            };

            // Transform sang ảnh scene
            PointF[] sceneCorners = CvInvoke.PerspectiveTransform(corners, homography);

            // Vẽ khung
            for (int i = 0; i < 4; i++)
            {
                Point pt1 = Point.Round(sceneCorners[i]);
                Point pt2 = Point.Round(sceneCorners[(i + 1) % 4]);
                CvInvoke.Line(imgScene, pt1, pt2, new MCvScalar(0, 255, 0), 2);
            }

            // Hiển thị
            CvInvoke.Imshow("ORB Match", imgScene);
            CvInvoke.WaitKey();
        }
        else
        {
            System.Console.WriteLine("Không tìm được homography.");
        }
    }
    else
    {
        System.Console.WriteLine("Không đủ match tốt.");
    }
}
static void AkazeMatchTemplate<TColor, TDepth>(
    Image<TColor, TDepth> imgScene,
    Image<TColor, TDepth> imgTemplate,
    double LowesRatioThreshold = 0.75
    )
    where TColor : struct, IColor
    where TDepth : new()
{
    using AKAZE akaze = new AKAZE();
    using VectorOfKeyPoint kpTemplate = new VectorOfKeyPoint();
    using VectorOfKeyPoint kpScene = new VectorOfKeyPoint();

    using Mat descTemplate = new Mat();
    using Mat descScene = new Mat();

    akaze.DetectAndCompute(imgTemplate, null, kpTemplate, descTemplate, false);
    akaze.DetectAndCompute(imgScene, null, kpScene, descScene, false);

    Console.WriteLine($"Template Keypoints: {kpTemplate.Size}");
    Console.WriteLine($"Scene Keypoints: {kpScene.Size}");

    using BFMatcher matcher = new BFMatcher(DistanceType.Hamming, crossCheck: false);
    using VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
    matcher.KnnMatch(descTemplate, descScene, matches, 2);

    using VectorOfDMatch goodMatches = new VectorOfDMatch();
    for (int i = 0; i < matches.Size; i++)
    {
        var matchSet = matches[i];
        if (matchSet.Size >= 2)
        {
            var m1 = matchSet[0];
            var m2 = matchSet[1];
            if (m1.Distance < LowesRatioThreshold * m2.Distance)
            {
                goodMatches.Push(new[] { m1 });
            }
        }
    }

    // Nếu đủ match tốt, tính homography
    if (goodMatches.Size >= 4)
    {
        var srcPoints = new PointF[goodMatches.Size];
        var dstPoints = new PointF[goodMatches.Size];

        for (int i = 0; i < goodMatches.Size; i++)
        {
            var match = goodMatches[i];
            srcPoints[i] = kpTemplate[match.QueryIdx].Point;
            dstPoints[i] = kpScene[match.TrainIdx].Point;
        }

        Mat homography = CvInvoke.FindHomography(srcPoints, dstPoints, RobustEstimationAlgorithm.Ransac, 5);

        if (!homography.IsEmpty)
        {
            // Góc template
            PointF[] corners = new PointF[]
            {
                        new PointF(0, 0),
                        new PointF(imgTemplate.Width, 0),
                        new PointF(imgTemplate.Width, imgTemplate.Height),
                        new PointF(0, imgTemplate.Height)
            };

            // Transform sang ảnh scene
            PointF[] sceneCorners = CvInvoke.PerspectiveTransform(corners, homography);

            // Vẽ khung
            for (int i = 0; i < 4; i++)
            {
                Point pt1 = Point.Round(sceneCorners[i]);
                Point pt2 = Point.Round(sceneCorners[(i + 1) % 4]);
                CvInvoke.Line(imgScene, pt1, pt2, new MCvScalar(0, 255, 0), 2);
            }

            // Hiển thị
            CvInvoke.Imshow("ORB Match", imgScene);
            CvInvoke.WaitKey();
        }
        else
        {
            System.Console.WriteLine("Không tìm được homography.");
        }
    }
    else
    {
        System.Console.WriteLine($"Không đủ match tốt. {goodMatches.Size}");
    }
}