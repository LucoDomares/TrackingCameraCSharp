using TrackingCamera.BaseCameraClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingCamera.CameraClasses
{
	public class CameraBuilder
	{
		public virtual void register_builder(string key, CameraBuilder builder)
		{
			throw new NotImplementedException("Not yet implemented.");
		}

		public virtual BaseCamera Build(string onvif_wsdl_path, Dictionary<string, string> settings)
		{
			throw new NotImplementedException("Not yet implemented.");
		}
	}
}
