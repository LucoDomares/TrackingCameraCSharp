using System;
using System.Collections.Generic;
using System.Text;

namespace TrackingCamera.CameraManagers
{
	public class BaseCameraManager
	{
		protected bool IsStopping { get; set; } = false;
		public bool IsStopped { get; set; } = false;

		public void Stop()
		{
			this.IsStopping = true;
		}

	}
}
