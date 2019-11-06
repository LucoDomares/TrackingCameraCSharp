using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using TrackingCamera.DetectorClasses;

namespace TrackingCamera.BaseDetectorClasses
{
	public abstract class BaseDetector
	{
		protected Double MinConfidence { get; set; }
		protected string Detector { get; set; }
		protected string Embedder { get; set; }
		protected string LabelEncoder { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public BaseDetector(Double MinConfidence)
		{
			this.MinConfidence = MinConfidence;
		}

		public abstract Mat Detect(Mat frame, out FacesList facesList);
	}
}
