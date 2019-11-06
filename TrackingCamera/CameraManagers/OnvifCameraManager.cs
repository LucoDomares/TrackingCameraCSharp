using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using TrackingCamera.BaseCameraClasses;
using TrackingCamera.CameraClasses;
using TrackingCamera.DetectorClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraManagers
{
	/// <summary>
	/// Camera Manager class for cameras that support ONVIF protocol to control PTZ functions
	/// </summary>
	class OnvifCameraManager : BaseCameraManager
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="camera">The camera being managed.</param>
		/// <param name="cameraConfig">The config of the camera being managed.</param>
		public OnvifCameraManager(BaseOnvifPtzCamera camera, CameraConfig cameraConfig): base(camera, cameraConfig)
		{
			// todo: does this class really need to exist?
		}
	}
}
