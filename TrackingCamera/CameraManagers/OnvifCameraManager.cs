using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrackingCamera.BaseCameraClasses;
using TrackingCamera.CameraClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraManagers
{
	class OnvifCameraManager : BaseCameraManager
	{
		/// <summary>
		/// The locking object for Ptz controls.
		/// </summary>
		readonly object ptzLock = new object();

		/// <summary>
		/// Indicates if the system is on high alert.
		/// <para>This is usually <c>True</c> if faces have been detected in the video feed recently.</para>
		/// <para>Otherwise, if no feces have been found for a while, <c>False</c></para>
		/// </summary>
		bool IsOnHighAlert { get; set; } = false;

		/// <summary>
		/// Indicates if Ptz auto tracking of faces is enabled.
		/// </summary>
		bool IsAutoTracking { get; set; } = false;

		/// <summary>
		/// The amount to Pan the camera left/right. +/- values indicate left and right movement.
		/// </summary>
		int PanAmt { get; set; } = 0;

		/// <summary>
		/// The amount to Tilt the camera up/down. +/- values indicate up and down movement.
		/// </summary>
		int TiltAmt { get; set; } = 0;

		/// <summary>
		/// The camera being managed.
		/// </summary>
		BaseOnvifPtzCamera Camera { get; set; }

		/// <summary>
		/// The configuration of the camera.
		/// </summary>
		CameraConfig CameraConfig { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="camera">The camera being managed.</param>
		/// <param name="cameraConfig">The config of the camera being managed.</param>
		public OnvifCameraManager(BaseOnvifPtzCamera camera, CameraConfig cameraConfig)
		{
			this.Camera = camera;
			this.CameraConfig = cameraConfig;
		}

		/// <summary>
		/// Runs the manager Ansyncronously.
		/// </summary>
		public async void RunAsync()
		{
			// start asynchronous ptz Controller.
			await ManagePtzControl();

			// manage video stream - this is a blocking call.
			ManageVideostream();

			// if we get here, then managing of the video stream has stopped/completed.
			// mark the process as stopped.
			this.IsStopped = true;
		}

		/// <summary>
		/// Runs the Ptz controller Asyncronously.
		/// </summary>
		/// <returns>An Asyncronous <c>Task</c></returns>
		private async Task ManagePtzControl()
		{
			// loop indefinitely, controlling camera movement as described by PanAmt, TiltAmt.
			// todo: Implement ZoomAmt control.
			while (!this.IsStopping)
			{
				if ((this.Camera.PtzPanAmt == 0) || (this.Camera.PtzTiltAmt == 0))
				{
					// nothing to do, sleep for a while
					await Task.Delay(1200);
				}
				else
				{
					if (!this.Camera.IsTargetCentered())
					{
						lock (this.ptzLock)
						{
							this.Camera.SetPan(this.PanAmt);
							this.Camera.SetTilt(this.TiltAmt);
						}

						// execute the pan/tilt
						this.Camera.ExecutePanTilt();
					}
					else
					{
						if (!IsOnHighAlert)
						{
							// sleep for a while
							await Task.Delay(300);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// The Video Stream Manager. This is a non Ansyncronous method.
		/// </summary>
		private void ManageVideostream ()
		{
			// loop indefinitely
			while (!this.IsStopping)
			{
				// Todo: Port the CV2 Python code across to here using the .Net wrapper.
			}
		}
	}
}
