using System.Collections.Generic;
using System.Collections;
using TrackingCamera.Helpers;
using TrackingCamera.BaseCameraClasses;

namespace TrackingCamera.CameraClasses
{
	/// <summary>
	/// The static camera factory class
	/// </summary>
	public static class StaticCameraFactory
	{
		// The Factory
		public static readonly CameraFactory Factory = RegisterAllCameraClasses();

		/// <summary>
		/// Registers each concrete camera builder.
		/// </summary>
		/// <returns></returns>
		public static CameraFactory RegisterAllCameraClasses()
		{
			CameraFactory factory = new CameraFactory();

			factory.RegisterBuilder("Vstarcam_C7823WIP", new Vstarcam_C7823WIPCameraBuilder());
			//factory.register_builder("easyN_A110", easyN_A110CameraBuilder());
			//factory.register_builder("HIKVISION_camera", HIKVISION_cameraCameraBuilder());

			return factory;
		}

		/// <summary>
		/// The Camera factory class
		/// </summary>
		public class CameraFactory
		{

			// The list of camera builders.
			public Dictionary<string, CameraBuilder> Builders { get; set; }

			/// <summary>
			/// Constructor
			/// </summary>
			public CameraFactory()
			{
				this.Builders = new Dictionary<string, CameraBuilder>
				{
				};
			}

			/// <summary>
			/// Registers a builder for a concrete camera class
			/// </summary>
			/// <param name="key">The class name of the concrete camera class builder</param>
			/// <param name="builder">the concrete camera builder class</param>
			public void RegisterBuilder(string key, CameraBuilder builder)
			{
				this.Builders.Add(key, builder);
			}

			/// <summary>
			/// Instantiates a concrete class of camera
			/// </summary>
			/// <param name="cameraConfig"></param>
			/// <returns></returns>
			public virtual BaseCamera CreateCamera(CameraConfig cameraConfig)
			{
				CameraBuilder builder = this.Builders[cameraConfig.CameraClass];
				if (builder == null)
				{
					throw new System.ArgumentNullException(cameraConfig.CameraClass);
				}
				return builder.Build(cameraConfig);
			}
		}
	}
}

