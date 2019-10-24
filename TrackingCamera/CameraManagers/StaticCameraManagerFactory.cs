using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TrackingCamera.CameraClasses;

namespace TrackingCamera.CameraManagers
{
	/// <summary>
	/// Static class for all camera factories. This is where each type of factory is registered
	/// </summary>
	public static class StaticCameraManagerFactory
	{
		/// <summary>
		/// Registers the available factories.
		/// </summary>
		/// <param name="factory"></param>
		public static void RegisterAllCameraManagers(CameraManagerFactory factory)
		{
			factory.RegisterBuilder("Vstarcam_C7823WIP", new Vstarcam_C7823WIPManagerBuilder());
			// todo: implement the rest of the camera classes.
			//factory.register_builder("easyN_A110", easyN_A110CameraBuilder());
			//factory.register_builder("HIKVISION_camera", HIKVISION_cameraCameraBuilder());
		}


		public class CameraManagerFactory
		{
			/// <summary>
			/// The list of Camera Manager Builders.
			/// </summary>
			public Dictionary<string, CameraManagerBuilder> Builders { get; set; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public CameraManagerFactory()
			{
				this.Builders = new Dictionary<string, CameraManagerBuilder>
				{
				};
			}

			/// <summary>
			/// Registers a Camera Manager Builder for use.
			/// </summary>
			/// <param name="key">The unique identifier of the Manager Builder. This is the Class Name of the Factory.</param>
			/// <param name="builder">The Builder class itself.</param>
			public void RegisterBuilder(string key, CameraManagerBuilder builder)
			{
				this.Builders.Add(key, builder);
			}

			/// <summary>
			/// Creates a Camera Manager using the <c>Helpers.CameraConfig</c> supplied.
			/// </summary>
			/// <param name="cameraConfig">The list of camera configuration details to build and manage the camera with.</param>
			/// <returns></returns>
			public virtual BaseCameraManager CreateCameraManager(Helpers.CameraConfig cameraConfig)
			{
				CameraManagerBuilder builder = this.Builders[cameraConfig.CameraClass];
				if (builder == null)
				{
					throw new System.ArgumentNullException(cameraConfig.CameraClass);
				}
				return builder.Build(cameraConfig);
			}
		}
	}
}

