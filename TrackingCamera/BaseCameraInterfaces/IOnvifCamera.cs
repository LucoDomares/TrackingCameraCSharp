using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;

namespace TrackingCamera.BaseCameraInterfaces
{
	/// <summary>
	/// Interface for a camera that supports Onvif protocol
	/// </summary>
	interface IOnvifCamera
	{
		int HttpPort { get; set; }

		Mictlanix.DotNet.Onvif.Device.Device Camera { get; set; }

		Mictlanix.DotNet.Onvif.Media.MediaClient MediaClient { get; set; }

		Mictlanix.DotNet.Onvif.Common.Profile MediaProfile { get; set; }

		Mictlanix.DotNet.Onvif.Ptz.PTZClient PtzController { get; set; }

		int OnvifPort { get; set; }

		int RtspPort { get; set; }

		string VideoStreamUri { get; set; }

	}
}
