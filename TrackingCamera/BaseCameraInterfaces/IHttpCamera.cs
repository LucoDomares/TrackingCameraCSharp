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
	/// Interface for a camera that supports Http Protocol
	/// </summary>
	interface IHttpCamera
	{
		int HttpPort { get; set; }
	
		object PtzController { get; set; }

		string RtspPort { get; set; }

		string VideoStreamUri { get; set; }

	}
}
