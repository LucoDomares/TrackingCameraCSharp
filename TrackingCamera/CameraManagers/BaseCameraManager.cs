using System;
using System.Collections.Generic;
using System.Text;

namespace TrackingCamera.CameraManagers
{
	/// <summary>
	/// The Base Camera Manager class.
	/// </summary>
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
