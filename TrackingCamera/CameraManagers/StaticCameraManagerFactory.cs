using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TrackingCamera.CameraClasses;

namespace TrackingCamera.CameraManagers
{

	public static class StaticCameraManagerFactory
	{

		public static void RegisterAllCameraManagers(CameraManagerFactory factory)
		{
			factory.RegisterBuilder("Vstarcam_C7823WIP", new Vstarcam_C7823WIPManagerBuilder());
			//factory.register_builder("easyN_A110", easyN_A110CameraBuilder());
			//factory.register_builder("HIKVISION_camera", HIKVISION_cameraCameraBuilder());
		}

		public class CameraManagerFactory
		{

			public Dictionary<string, CameraManagerBuilder> Builders { get; set; }

			public CameraManagerFactory()
			{
				this.Builders = new Dictionary<string, CameraManagerBuilder>
				{
				};
			}

			public void RegisterBuilder(string key, CameraManagerBuilder builder)
			{
				this.Builders.Add(key, builder);
			}

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

