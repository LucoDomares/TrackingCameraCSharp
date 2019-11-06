using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace TrackingCamera.DetectorClasses
{
	public class Face 
	{
		public Mat Image { get; set; }
		public Point PosTopLeft { get; set;}
		public Point PosBottomRight { get; set; }
		public float Confidence { get; set; }

		public Face(Mat image, Point posTopLeft, Point posBottomRight, float confidence)
		{
			this.Image = image;
			this.PosTopLeft = posTopLeft;
			this.PosBottomRight = posBottomRight;
			this.Confidence = confidence;
		}
	}

	public class FacesList : List<Face>
	{
	}
}
