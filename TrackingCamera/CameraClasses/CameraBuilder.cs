using TrackingCamera.BaseCameraClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraClasses
{
	/// <summary>
	/// Base class for a camera builder. Implemented for each concrete class of camera and used 
	/// by the CameraFactory to instantiate a given camera at run time.
	/// </summary>
	public abstract class CameraBuilder
	{
		/// <summary>
		/// Registers a builder for a concrete camera class
		/// </summary>
		/// <param name="key">The class name of the concrete camera class builder</param>
		/// <param name="builder">the concrete camera builder class</param>
		public virtual void RegisterBuilder(string key, CameraBuilder builder)
		{
			throw new NotImplementedException("Not yet implemented.");
		}
		
		/// <summary>
		/// Instantiates the actual camera class
		/// </summary>
		/// <param name="cameraConfig">config settings for the camera</param>
		/// <returns></returns>
		public virtual BaseCamera Build(CameraConfig cameraConfig)
		{
			throw new NotImplementedException("Not yet implemented.");
		}
	}
}
