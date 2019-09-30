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
		readonly object ptzLock = new object();
		bool IsOnHighAlert { get; set; } = false;
		bool IsAutoTracking { get; set; } = false;
		int PanAmt { get; set; } = 0;
		int TiltAmt { get; set; } = 0;

		BaseOnvifPtzCamera Camera { get; set; }
		CameraConfig CameraConfig { get; set; }


		public OnvifCameraManager(BaseOnvifPtzCamera camera, CameraConfig cameraConfig)
		{
			this.Camera = camera;
			this.CameraConfig = cameraConfig;
		}

		public async void RunAsync()
		{
			// start asynchronous ptz Controller.
			await ManagePtzControl();

			ManageVideostream();

			// mark the process as stopped.
			this.IsStopped = true;
		}

		private async Task ManagePtzControl()
		{
			// loop indefinitely
			while (!this.IsStopping)
			{
				if ((this.Camera.PtzPanAmt == 0) || (this.Camera.PtzTiltAmt == 0))
				{
					// sleep for a while
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
		
		private void ManageVideostream ()
		{
			// loop indefinitely
			while (!this.IsStopping)
			{

			}
		}
	}
}
