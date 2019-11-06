using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;
using TrackingCamera.Helpers;
using TrackingCamera.BaseCameraInterfaces;
using OpenCvSharp;

namespace TrackingCamera.BaseCameraClasses
{
	/// <summary>
	/// Abstract camera class implementing the Ptz and Onvif interfaces.
	/// </summary>
	public abstract class BaseOnvifPtzCamera : BasePtzCamera, IOnvifCamera, IPtzCamera
	{
		#region IPtzCamera members

		public int PanTiltSleepLongMilliseconds { get; set; }

		public int PanTileSleepShortMilliseconds { get; set; }

		public int PtzMovementSmallThreshold { get; set; }

		public override bool IsSupportsPTZ => true;

		#endregion

		#region IOnvifCamera members
		new public Mictlanix.DotNet.Onvif.Device.Device Camera { get; set; }
		public int HttpPort { get; set; }
		public Mictlanix.DotNet.Onvif.Media.MediaClient MediaClient { get; set; }
		public Profile MediaProfile { get; set; }
		public Mictlanix.DotNet.Onvif.Ptz.PTZClient PtzController { get; set; }
		public int OnvifPort { get; set; }
		public int RtspPort { get; set; }
		public string VideoStreamUri { get; set; }
		
		#endregion

		/// <summary>
		/// Sets up the connection to the camera, enquires to get metadata from the Onvif service 
		/// </summary>
		/// <param name="camera"></param>
		/// <returns></returns>
		static async Task AsyncHelper(BaseOnvifPtzCamera camera)
		{
			Globals.Log.Debug(string.Format("Connecting to camera at {0}", camera.HostAddress));

			camera.Camera = await OnvifClientFactory.CreateDeviceClientAsync(camera.HostAddress, camera.UserName, camera.Password);
			camera.MediaClient = await OnvifClientFactory.CreateMediaClientAsync(camera.HostAddress, camera.UserName, camera.Password); ;
			camera.PtzController = await OnvifClientFactory.CreatePTZClientAsync(camera.HostAddress, camera.UserName, camera.Password);

			Mictlanix.DotNet.Onvif.Media.GetProfilesResponse profiles = await camera.MediaClient.GetProfilesAsync();
			camera.MediaProfile = profiles.Profiles.FirstOrDefault();
			if (camera.MediaProfile != null)
			{
				StreamSetup streamSetup = new StreamSetup
											{ Stream = StreamType.RTPUnicast, Transport = new Transport() };

				streamSetup.Transport.Protocol = TransportProtocol.TCP;
				MediaUri videoStreamUriObject = await camera.MediaClient.GetStreamUriAsync(streamSetup, camera.MediaProfile.Name);
				camera.VideoStreamUri = videoStreamUriObject.Uri;

			}

			Mictlanix.DotNet.Onvif.Device.GetNetworkProtocolsRequest request = new Mictlanix.DotNet.Onvif.Device.GetNetworkProtocolsRequest();
			Mictlanix.DotNet.Onvif.Device.GetNetworkProtocolsResponse response = await camera.Camera.GetNetworkProtocolsAsync(request);
	
			// store http and rtsp ports
			foreach (NetworkProtocol protocol in response.NetworkProtocols)
			{
				string protocolName = protocol.Name.ToString();
				switch (protocolName)
				{
					case "HTTP":
						camera.HttpPort = protocol.Port[0];
						break;

					case "RTSP":
						camera.RtspPort = protocol.Port[0];
						break;
				}
			}

			Mictlanix.DotNet.Onvif.Media.GetVideoSourcesResponse video_sources = await camera.MediaClient.GetVideoSourcesAsync();

			Globals.Log.Debug("Camera connected");
		}

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="CameraIpAddress">the Ip address of the camera (without http://)</param>
		/// <param name="UserName">the user name to log in with</param>
		/// <param name="Password">the password to log in with</param>
		/// <param name="CameraName">the user friendly name of the camera</param>
		public BaseOnvifPtzCamera(string cameraIpAddress, string userName, string password, string cameraName)
			: base(cameraIpAddress, userName, password, cameraName)
		{
			this.IsPtzMoveRelative = false;
			this.PtzTrackingThreshold = 50;
			this.PtzMovementSmallThreshold = 80;
			this.PanTiltSleepLongMilliseconds = 1000;
			this.PanTileSleepShortMilliseconds = 300;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="CameraIpAddress">the Ip address of the camera (without http://)</param>
		/// <param name="UserName">the user name to log in with</param>
		/// <param name="Password">the password to log in with</param>
		/// <param name="CameraName">the user friendly name of the camera</param>
		/// <param name="onvifPort">the port number of the Onvif service</param>
		public BaseOnvifPtzCamera(string cameraIpAddress, string userName, string password,
			string cameraName, int onvifPort) 
			: this(cameraIpAddress, userName, password, cameraName)
		{
			this.OnvifPort = onvifPort;
			this.HostAddress = String.Format("{0}:{1}", this.CameraIpAddress, this.OnvifPort);

			AsyncHelper(this).Wait();

			Globals.Log.Debug("ONVIF Port: " + this.OnvifPort.ToString());
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Is it very far to move the camera in order to center it on the target?
		/// </summary>
		/// <param name="panAmt">The Pan amount that has been set.</param>
		/// <param name="tiltAmt">The tile amount that has been set.</param>
		/// <returns><c>true</c> if the pan or tilt amount is over the small movement threshold.</returns>
		public override bool IsFarToMove(int panAmt, int tiltAmt)
		{
			return (Math.Abs(panAmt) > this.PtzMovementSmallThreshold) | (Math.Abs(tiltAmt) > this.PtzMovementSmallThreshold);
		}

		/// <summary>
		/// Closes the Video stream
		/// </summary>
		public override void CloseVideo()
		{
			try
			{
				if (this.VideoStreamer != null)
				{
					this.VideoStreamer = null;
				}
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
				throw;
			}
		}

		/// <summary>
		/// Gets the next video frame from the camera.
		/// </summary>
		/// <returns></returns>
		public override Mat GetFrameImpl()
		{
			try
			{
				if (this.VideoStreamer != null)
				{
					Mat frame = new Mat();
					this.VideoStreamer.Read(frame);

					if (frame == null)
					{
						throw new ArgumentNullException("Empty frame was returned by videostream");
					}
					if (this.IsInvertedVideo) {
						// the camera is inverted, so flip the video feed.
						frame.Flip(FlipMode.Y);
					}

					return frame;
				}
				else
				{
					throw new ArgumentNullException("Cannot get frame because there is no videostream");
				}
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
			}

			return null;
		}

		#region Move Continuous Commands

		/// <summary>
		/// Sets the Zoom value before Execute Zoom command is called.
		/// </summary>
		/// <param name="zoomAmt"></param>
		public override void SetZoomContinuous(int zoomAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Executes the Zoom command.
		/// Call SetZoomContinuous first before calling this method.
		/// </summary>
		public override void ExecuteZoomContinuous()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Sets the Tilt value before Execute PanTilt command is called.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public override void SetTiltContinuous(int tiltAmt)
		{
			if (tiltAmt != 0 && Math.Abs(tiltAmt) > this.PtzTrackingThreshold)
			{
				if (this.IsInvertedVideo)
				{
					tiltAmt = tiltAmt * -1;
				}
			}
			else
			{
				tiltAmt = 0;
			}

			this.PtzTiltAmt = tiltAmt;
		}

		/// <summary>
		/// Sets the Pan value before Execute PanTilt command is called.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public override void SetPanContinuous(int panAmt)
		{
			if (panAmt != 0 && Math.Abs(panAmt) > this.PtzTrackingThreshold)
			{
				if (!this.IsInvertedVideo)
				{
					panAmt = panAmt * -1;
				}
			}
			else
			{
				panAmt = 0;
			}
			
			this.PtzPanAmt = panAmt;	
		}

		/// <summary>
		/// Executes the Pan and Tilt command.
		/// Call SetTiltContinuous first before calling this method.
		/// Call SetPanContinuous first before calling this method.
		/// </summary>
		public override void ExecutePanTiltContinuous()
		{
			this.PtzController.ContinuousMoveAsync(this.MediaProfile.Name, new PTZSpeed
			{
				PanTilt = new Vector2D
				{
					x = this.PtzPanAmt,
					y = this.PtzTiltAmt
				},
				Zoom = new Vector1D
				{
					x = 0
				}
			}, null);

			if (Math.Abs(this.PtzPanAmt) > 80 || Math.Abs(this.PtzTiltAmt) > 80)
			{
				// Wait for camera to move longer as there's further to move
				Task.Delay(this.PanTiltSleepLongMilliseconds);
			}
			else
			{
				// Wait for camera to move only a short while
				Task.Delay(this.PanTileSleepShortMilliseconds);
			}

			// Stop continuous move
			this.StopPtz();

		}

		#endregion

		#region Move Relative Commands

		/// <summary>
		/// Sets the relative Tilt Amount.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public override void SetTiltRelative(int tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Sets the relative Pan Amount.
		/// </summary>
		/// <param name="panAmt"></param>
		public override void SetPanRelative(int panAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}
	
		/// <summary>
		/// Executes the Relative PanTilt command.
		/// </summary>
		public override void ExecutePanTiltRelative()
		{
			this.StopPtz();
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Sets the relative Zoom Amount.
		/// </summary>
		/// <param name="zoomAmt"></param>
		public override void SetZoomRelative(int zoomAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Executes the relative Zoom Command.
		/// </summary>
		public override void ExecuteZoomRelative()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		#endregion

		/// <summary>
		/// Stops all Pan/tilt/zoom commands.
		/// </summary>
		public override void StopPtz()
		{
			// Stop continuous move
			this.PtzController.StopAsync(this.MediaProfile.Name, true, true);
		}

		#endregion

		#region  Protected Abstract Methods

		/// <summary>
		/// Implementation of the Open Video method.
		/// Must be overriden in the descending camera class.
		/// </summary>
		public abstract override void OpenVideoImpl();

		#endregion
	}
}
