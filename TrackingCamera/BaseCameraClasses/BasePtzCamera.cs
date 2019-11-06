#region Using Clauses
using System.Collections;
using System;
using OpenCvSharp;
using TrackingCamera.Helpers;
using System.Collections.Concurrent;
#endregion

namespace TrackingCamera.BaseCameraClasses
{
	/// <summary>
	/// The base camera class from which all cameras descend.
	/// </summary>
	public abstract class BasePtzCamera: BaseCamera
	{
		public int PtzPanAmt { get; set; }

		public int PtzTiltAmt { get; set; }

		public int PtzZoomAmt { get; set; }

		public int PtzTrackingThreshold { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="CameraIpAddress">the Ip address of the camera (without http://)</param>
		/// <param name="UserName">the user name to log in with</param>
		/// <param name="Password">the password to log in with</param>
		/// <param name="CameraName">the user friendly name of the camera</param>
		public BasePtzCamera(string CameraIpAddress, string UserName, string Password, string CameraName): base (CameraIpAddress, UserName, Password, CameraName)
		{
			
		}

		/// <summary>
		/// Is the current target (face) approximately centered in the video frame?
		/// </summary>
		/// <returns><c>True</c> if the target is centered, <c>False</c> otherwise.</returns>
		public bool IsTargetCentered()
		{
			if ((this.PtzPanAmt == 0) & (this.PtzTiltAmt == 0))
			{
				return true;
			}
			else if ((Math.Abs(this.PtzPanAmt) < this.PtzTrackingThreshold) & (Math.Abs(this.PtzTiltAmt) < this.PtzTrackingThreshold))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
