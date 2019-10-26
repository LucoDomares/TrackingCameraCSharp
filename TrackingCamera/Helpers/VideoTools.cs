using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace TrackingCamera.Helpers
{
	static class VideoTools
	{
		public static IplImage ResizeIplTo(IplImage sourceImage, int width, int height)
		{
			IplImage newImage = new IplImage(new OpenCvSharp.CvSize(width, height),
											 sourceImage.Depth, sourceImage.NChannels);

			sourceImage.Resize(newImage, Interpolation.Linear);

			return newImage;
		}

		public static bool FlipBool(bool value)
		{
			return !value;
		}
	}
}
