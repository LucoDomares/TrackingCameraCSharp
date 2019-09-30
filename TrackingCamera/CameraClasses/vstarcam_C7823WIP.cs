using TrackingCamera.BaseCameraClasses;
using TrackingCamera.BaseCameraInterfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using TrackingCamera.CameraManagers;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraClasses
{
	/// <summary>
	/// Implements the concrete camera class for a VStarcam model C7823WIP
	/// </summary>
	public class Vstarcam_C7823WIP : BaseOnvifPtzCamera, IOnvifCamera
	{
		// the Video Stream
		new public object VideoStreamer;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cameraIpAddress"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="cameraName"></param>
		/// <param name="onvifPort"></param>
		public Vstarcam_C7823WIP(
			string cameraIpAddress,
			string username,
			string password,
			string cameraName,
			int onvifPort)
			: base(cameraIpAddress, username, password, cameraName, onvifPort)
		{
			
		}

		// overrides

		public override void OpenVideoImpl()
		{
			if (this.VideoStreamer != null)
			{
				this.CloseVideo();
			}
			var addressFull = "";
			if (this.VideoStreamUri != null)
			{
				string addressPrefix = "http://";
				string addressSuffix = "/livestream.cgi?";
				string addressUserSuffix = "user=";
				string addressPasswordSuffix = "&pwd=";
				string AddressFinalSuffix = "&streamid=0";
				addressFull = string.Format("{0}{1}:{2}{3}{4}{5}{6}{7}{8}", addressPrefix, this.CameraIpAddress, this.HttpPort.ToString(), addressSuffix, addressUserSuffix, this.UserName, addressPasswordSuffix, this.Password, AddressFinalSuffix);
			}

			this.VideoStreamer = new CvCapture(addressFull);

			Task.Delay(500);
		}
	}

	public class Vstarcam_C7823WIPCameraBuilder : CameraBuilder
	{

		public Vstarcam_C7823WIP Instance;

		public Vstarcam_C7823WIPCameraBuilder()
		{
			this.Instance = null;
		}

		public override BaseCamera Build(CameraConfig cameraConfig)
		{
			if (this.Instance == null)
			{
				this.Instance = new Vstarcam_C7823WIP(cameraConfig.IpAddress, cameraConfig.UserName, cameraConfig.Password, cameraConfig.CameraName, cameraConfig.OnvifPort);
			}
			return this.Instance;
		}
	}


	public class Vstarcam_C7823WIPManagerBuilder : CameraManagerBuilder
	{
		public Thread Instance;

		public Vstarcam_C7823WIPManagerBuilder()
		{
			this.Instance = null;
		}

		public override BaseCameraManager Build(Helpers.CameraConfig cameraConfig)
		{
			// todo: this whole method can be generic and move to base class
			BaseCamera camera = StaticCameraFactory.Factory.CreateCamera(cameraConfig);
			OnvifCameraManager manager = new OnvifCameraManager((BaseOnvifPtzCamera)camera, cameraConfig);
			manager.RunAsync();	

			// todo: temporarily return null
			return manager;
		}
	}
}
