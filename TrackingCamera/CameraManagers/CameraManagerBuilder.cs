using TrackingCamera.BaseCameraClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TrackingCamera.CameraManagers
{
	/// <summary>
	/// The Camera Manager Builder.
	/// </summary>
	public class CameraManagerBuilder
	{
		/// <summary>
		/// Registers a Camera Manager Builder for use.
		/// </summary>
		/// <param name="key">The unique identifier of the Manager Builder. This is the Class Name of the Factory.</param>
		/// <param name="builder">The Builder class itself.</param>
		public virtual void RegisterBuilder(string key, CameraManagerBuilder builder)
		{
			throw new NotImplementedException("Not yet implemented.");
		}

		/// <summary>
		/// Instantiates a Builder Manager ready for use.
		/// </summary>
		/// <param name="cameraConfig">The config of the camera to be built and managed</param>
		/// <returns></returns>
		public virtual BaseCameraManager Build(Helpers.CameraConfig cameraConfig)
		{
			throw new NotImplementedException("Not yet implemented.");
		}
	}
}
