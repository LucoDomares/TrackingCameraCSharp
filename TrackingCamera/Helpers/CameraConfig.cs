using System;
using System.Collections.Generic;
using System.Text;

namespace TrackingCamera.Helpers
{
	[Serializable]
	public class CameraConfig
	{
		public string CameraClass { get; set; }
		public string CameraName { get; set; }
		public int HttpPort { get; set; }
		public string IpAddress { get; set; }
		public bool IsActive { get; set; }
		public double MinConfidence { get; set; }
		public int OnvifPort { get; set; }
		public string Password { get; set; }
		public int RtspPort { get; set; }
		public string UserName { get; set; }

	}
}
