using TrackingCamera.BaseCameraClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TrackingCamera.CameraManagers
{
	public class CameraManagerBuilder
	{
		public virtual void RegisterBuilder(string key, CameraManagerBuilder builder)
		{
			throw new NotImplementedException("Not yet implemented.");
		}

		public virtual BaseCameraManager Build(Helpers.CameraConfig cameraConfig)
		{
			throw new NotImplementedException("Not yet implemented.");
		}
	}
}
