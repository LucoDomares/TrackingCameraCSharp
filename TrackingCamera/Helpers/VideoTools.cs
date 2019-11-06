using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace TrackingCamera.Helpers
{
	static class VideoTools
	{
		public static Mat ResizeIplTo(Mat sourceImage, int width, int height)
		{
			Mat newImage = new Mat();
			Cv2.Resize(sourceImage, newImage, new Size(width, height), 0, 0, InterpolationFlags.Linear);

			return newImage;
		}

		public static bool FlipBool(bool value)
		{
			return !value;
		}
	}
}
