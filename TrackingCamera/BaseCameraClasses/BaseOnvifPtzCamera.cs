﻿using System;
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
	public abstract class BaseOnvifPtzCamera : BaseCamera, IOnvifCamera, IPtzCamera
	{
		#region IPtzCamera members

		public bool IsPtzMoveRelative { get; set; }

		public int PanTiltSleepLongMilliseconds { get; set; }

		public int PanTileSleepShortMilliseconds { get; set; }

		public int PtzMovementSmallThreshold { get; set; }

		public int PtzTrackingThreshold { get; set; }

		public override bool IsSupportsPTZ => true;

		#endregion

		#region IOnvifCamera members
		new public Mictlanix.DotNet.Onvif.Device.Device Camera { get; set; }
		public string HttpPort { get; set; }
		public Mictlanix.DotNet.Onvif.Media.MediaClient MediaClient { get; set; }
		public Profile MediaProfile { get; set; }
		public Mictlanix.DotNet.Onvif.Ptz.PTZClient PtzController { get; set; }
		public int OnvifPort { get; set; }
		public string RtspPort { get; set; }
		public string VideoStreamUri { get; set; }
		public string HostAddress { get; set; }
		public int PtzPanAmt { get; set; }
		public int PtzTiltAmt { get; set; }
		public int PtzZoomAmt { get; set; }
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
											{
												Stream = StreamType.RTPUnicast,
												Transport = new Transport()
											};
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
						camera.HttpPort = protocol.Port[0].ToString();
						break;

					case "RTSP":
						camera.RtspPort = protocol.Port[0].ToString();
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
		/// Sets the amount of movement required in the Up/Down direction.
		/// </summary>
		/// <param name="tiltAmt">the amount to tilt.</param>
		public virtual void SetTilt(int tiltAmt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetTiltRelative(tiltAmt);
			}
			else
			{
				this.SetTiltContinuous(tiltAmt);
			}
		}

		/// <summary>
		/// Sets the amount of movement required in the left/right direction.
		/// </summary>
		/// <param name="panAmt">the amount to pan</param>
		public virtual void SetPan(int panAmt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetPanRelative(panAmt);
			}
			else
			{
				this.SetPanContinuous(panAmt);
			}
		}

		/// <summary>
		/// Sets the amount of zoom required in/out.
		/// </summary>
		/// <param name="zoomAmt">the amount to zoom</param>
		public virtual void SetZoom(int zoomAmt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetZoomRelative(zoomAmt);
			}
			else
			{
				this.SetZoomContinuous(zoomAmt);
			}
		}

		/// <summary>
		/// Executes the Pan & Tilt functions of the PTZ controller 
		/// according the Pan and Tilt values previously set.
		/// </summary>
		public virtual void ExecutePanTilt()
		{
			try
			{
				if (this.IsPtzMoveRelative)
				{
					this.ExecutePanTiltRelative();
				}
				else
				{
					this.ExecutePanTiltContinuous();
				}
			}
			catch (Exception ex)
			{
				Globals.Log.Error(ex);
			}
		}

		/// <summary>
		/// Executes the zoom of the PTZ controller 
		/// according the zoom value previously set.
		/// </summary>
		public virtual void ExecuteZoom()
		{
			try
			{
				if (this.IsPtzMoveRelative)
				{
					this.ExecuteZoomRelative();
				}
				else
				{
					this.ExecuteZoomContinuous();
				}
			}
			catch (Exception ex)
			{
				Globals.Log.Error(ex);
			}
		}

		/// <summary>
		/// Tells the Ptz controller to tilt upwards
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void MoveUp(object locker)
		{
			var TiltAmt = 100;
			if (this.IsInvertedVideo)
			{
				TiltAmt = TiltAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetPan(0);
					this.SetTilt(TiltAmt);
					this.ExecutePanTilt();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Tells the Ptz controller to tilt downwards
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void MoveDown(object locker)
		{
			var TiltAmt = -100;
			if (this.IsInvertedVideo)
			{
				TiltAmt = TiltAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetPan(0);
					this.SetTilt(TiltAmt);
					this.ExecutePanTilt();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Tells the Ptz controller to pan Left
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void MoveLeft(object locker)
		{
			var PanAmt = 100;
			if (this.IsInvertedVideo)
			{
				PanAmt = PanAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetPan(PanAmt);
					this.SetTilt(0);
					this.ExecutePanTilt();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Tells the Ptz controller to pan right
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void MoveRight(object locker)
		{
			var PanAmt = -100;
			if (this.IsInvertedVideo)
			{
				PanAmt = PanAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetPan(PanAmt);
					this.SetTilt(0);
					this.ExecutePanTilt();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Tells the Ptz Controller to zoom in
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void ZoomOut(object locker)
		{
			var ZoomAmt = -100;
			if (this.IsInvertedVideo)
			{
				ZoomAmt = ZoomAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetZoom(ZoomAmt);
					this.ExecuteZoom();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Tells the Ptz controller to zoom out
		/// </summary>
		/// <param name="locker">the lock object</param>
		public virtual void ZoomIn(object locker)
		{
			var ZoomAmt = 100;
			if (this.IsInvertedVideo)
			{
				ZoomAmt = ZoomAmt * -1;
			}
			lock (locker)
			{
				try
				{
					this.SetZoom(ZoomAmt);
					this.ExecuteZoom();
				}
				catch (Exception ex)
				{
					Globals.Log.Error(ex);
				}
			}
		}

		/// <summary>
		/// Is it very far to move the camera in order to center it on the target?
		/// </summary>
		/// <param name="panAmt">The Pan amount that has been set.</param>
		/// <param name="tiltAmt">The tile amount that has been set.</param>
		/// <returns><c>true</c> if the pan or tilt amount is over the small movement threshold.</returns>
		public virtual bool IsFarToMove(int panAmt, int tiltAmt)
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

		// private overrides
		public override object GetFrameImpl()
		{
			try
			{
				if (this.VideoStreamer != null)
				{
					/*
                    @"frame_counter = self._videostream.get(cv2.cv.CV_CAP_PROP_POS_FRAMES)
				if frame_counter == self._videostream.get(cv2.cv.CV_CAP_PROP_FRAME_COUNT):
					#frame_counter = 0  # Or whatever as long as it is the same as next line
					self._videostream.set(cv2.cv.CV_CAP_PROP_POS_FRAMES, 0)
				";*/
					// grab the frame from the video stream
					// todo:
					var frame = this.VideoStreamer;//.QueryFrame(); //.read();
					if (frame == null)
					{
						throw new ArgumentNullException("Empty frame was returned by videostream");
					}
					if (this.IsInvertedVideo) {
                        // the camera is inverted, so flip the video feed.
						// todo:
                        //Cv.Flip(frame);
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


		public virtual void SetZoomContinuous(int tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void ExecuteZoomContinuous()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		// private overrides
		public virtual void SetTiltContinuous(int tiltAmt)
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

		public virtual void SetPanContinuous(int panAmt)
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

		public virtual void ExecutePanTiltContinuous()
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

		public virtual void SetTiltRelative(int tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void SetPanRelative(int panAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}
	
		public virtual void ExecutePanTiltRelative()
		{
			this.StopPtz();
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void SetZoomRelative(int zoomAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void ExecuteZoomRelative()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void StopPtz()
		{
			// Stop continuous move
			this.PtzController.StopAsync(this.MediaProfile.Name, true, true);
		}

		public bool IsTargetCentered()
		{
			if ((this.PtzPanAmt == 0) & (this.PtzTiltAmt == 0))
			{
				return true;
			}
			else if ((Math.Abs(this.PtzPanAmt) < this.PtzTrackingThreshold) & (Math.Abs(this.PtzTiltAmt) < this.PtzTrackingThreshold))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region  Protected Abstract Methods

		public abstract override void OpenVideoImpl();

		#endregion
	}
}
