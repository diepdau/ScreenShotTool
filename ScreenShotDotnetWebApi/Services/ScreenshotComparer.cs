namespace ScreenshotTool.Services
{
    public static class ScreenshotComparer
    {
            public static bool AreImagesSimilar(string imagePath1, string imagePath2, double threshold = 0.99)
            {
                using var img1 = new Emgu.CV.Mat(imagePath1, Emgu.CV.CvEnum.ImreadModes.Color);
                using var img2 = new Emgu.CV.Mat(imagePath2, Emgu.CV.CvEnum.ImreadModes.Color);

                if (img1.Size != img2.Size)
                    return false;

                using var diff = new Emgu.CV.Mat();
                Emgu.CV.CvInvoke.AbsDiff(img1, img2, diff);

                var diffGray = new Emgu.CV.Mat();
                Emgu.CV.CvInvoke.CvtColor(diff, diffGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                double totalPixels = diffGray.Total;
                int nonZeroPixels = Emgu.CV.CvInvoke.CountNonZero(diffGray);

                double similarity = 1.0 - (nonZeroPixels / totalPixels);

                return similarity >= threshold;
            }
        
    }

}
