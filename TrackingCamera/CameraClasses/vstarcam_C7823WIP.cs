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
		//new public CvCapture VideoStreamer;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cameraIpAddress">The IP address of the camera.</param>
		/// <param name="username">The username to login with.</param>
		/// <param name="password">The password to login with.</param>
		/// <param name="cameraName">The User-friendly name of the camera.</param>
		/// <param name="onvifPort">The ONVIF port number for controlling Ptz functions.</param>
		public Vstarcam_C7823WIP(
			string cameraIpAddress,
			string username,
			string password,
			string cameraName,
			int onvifPort)
			: base(cameraIpAddress, username, password, cameraName, onvifPort)
		{
			
		}

		#region Overrides

		/// <summary>
		/// The implmentation of the Open Video method for this camera.
		/// </summary>
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
			//this.VideoStreamer.GStreamerQueueLength = 1;
			//this.VideoStreamer = OpenCvSharp.VideoCapture()

			Task.Delay(500);
		}

		#endregion
	}

	#region Camera Builder

	/// <summary>
	/// The camera Manager factory class.
	/// There are 2 kinds of factory: 
	///    1. Camera Manager Factory - creates a Manager on it's own thread to manage a running camera.
	///    2. Camera Factory - creates a running camera which is then managed by it's Manager.
	/// </summary>
	public class Vstarcam_C7823WIPCameraBuilder : CameraBuilder
	{
		// the instantiated camera.
		public Vstarcam_C7823WIP Instance;

		/// <summary>
		/// constructor
		/// </summary>
		public Vstarcam_C7823WIPCameraBuilder()
		{
			this.Instance = null;
		}

		/// <summary>
		/// Instantiates a camera ready to be managed by its camera manager.
		/// </summary>
		/// <param name="cameraConfig">The configuration of the camera to be instantiated.</param>
		/// <returns>A running <c>Vstarcam_C7823WIP</c> camera</returns>
		public override BaseCamera Build(CameraConfig cameraConfig)
		{
			if (this.Instance == null)
			{
				this.Instance = new Vstarcam_C7823WIP(cameraConfig.IpAddress, cameraConfig.UserName, cameraConfig.Password, cameraConfig.CameraName, cameraConfig.OnvifPort);
			}
			return this.Instance;
		}
	}

	#endregion

	#region Manager Builder

	/// <summary>
	/// The camera manager factory class.
	/// </summary>
	public class Vstarcam_C7823WIPManagerBuilder : CameraManagerBuilder
	{
		// the managing thread.
		public Thread Instance;

		/// <summary>
		/// constructor
		/// </summary>
		public Vstarcam_C7823WIPManagerBuilder()
		{
			this.Instance = null;
		}

		/// <summary>
		/// Instantiates a Manager ready to manage a camera.
		/// </summary>
		/// <param name="cameraConfig">The configuration of the camera to be managed.</param>
		/// <returns>A <c>BaseCameraManager</c></returns>
		public override BaseCameraManager Build(Helpers.CameraConfig cameraConfig)
		{
			// todo: this whole method can be generic and move to base class
			BaseCamera camera = StaticCameraFactory.Factory.CreateCamera(cameraConfig);
			camera.OpenVideo();

			OnvifCameraManager manager = new OnvifCameraManager((BaseOnvifPtzCamera)camera, cameraConfig);
			manager.RunAsync();	

			return manager;
		}
	}

	#endregion
}
