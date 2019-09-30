using System.Collections.Generic;
using System.Collections;

namespace TrackingCamera.CameraClasses
{

	public static class StaticCameraFactory
	{

		public static void RegisterAllCameraClasses(CameraFactory factory)
		{
			factory.RegisterBuilder("vstarcam_C7823WIP", new Vstarcam_C7823WIPCameraBuilder());
			//factory.register_builder("easyN_A110", easyN_A110CameraBuilder());
			//factory.register_builder("HIKVISION_camera", HIKVISION_cameraCameraBuilder());
		}

		public class CameraFactory
		{

			public Dictionary<string, CameraBuilder> Builders { get; set; }

			public CameraFactory()
			{
				this.Builders = new Dictionary<string, CameraBuilder>
				{
				};
			}

			public void RegisterBuilder(string key, CameraBuilder builder)
			{
				this.Builders.Add(key, builder);
			}

			public virtual object CreateCamera(string key, string onvifWsdlPath, Dictionary<string, string> settings, Hashtable kwargs)
			{
				var builder = this.Builders[key];
				if (builder == null)
				{
					throw new System.ArgumentNullException(key);
				}
				return builder.Build(onvifWsdlPath, settings);
			}
		}
	}
}

