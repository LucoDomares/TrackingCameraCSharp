using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;

namespace TrackingCamera.BaseCameraInterfaces
{
	interface IOnvifCamera
	{
		string HttpPort { get; set; }
		Mictlanix.DotNet.Onvif.Device.Device Camera { get; set; }
		Mictlanix.DotNet.Onvif.Media.Media Media { get; set; }
		Mictlanix.DotNet.Onvif.Ptz.PTZ PtzController { get; set; }
		string OnvifPort { get; set; }
		//string OnvifWsdlPath { get; set; }
		string RtspPort { get; set; }
		string VideoStreamUri { get; set; }
	}
}
