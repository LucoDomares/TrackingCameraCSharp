using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using TrackingCamera.DetectorClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.BaseDetectorClasses
{
	public abstract class BaseDetectorFace : BaseDetector
	{
		public BaseDetectorFace(Double MinConfidence) : base(MinConfidence)
		{

		}

		public override Mat Detect(Mat frame, out FacesList facesList)
		{
			facesList = new FacesList();
			try
			{
				return GetFaces(frame, out facesList);
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
			}

			return null;
		}

		protected abstract Mat GetFaces(Mat frame, out FacesList facesList);
	}
}
