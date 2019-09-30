using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;
using TrackingCamera.BaseCameraInterfaces;

namespace TrackingCamera.BaseCameraClasses
{
	public abstract class BaseOnvifPtzCamera : BaseCamera, IOnvifCamera, IPtzCamera
	{
		#region IPtzCamera members

		public bool IsPtzMoveRelative { get; set; }

		public double PanTiltSleepLongSeconds { get; set; }

		public double PanTileSleepShortSeconds { get; set; }

		public int PtzMovementSmallThreshold { get; set; }

		public int PtzTrackingThreshold { get; set; }

		public override bool IsSupportsPTZ => true;

		#endregion

		#region IOnvifCamera members
		new public Mictlanix.DotNet.Onvif.Device.Device Camera { get; set; }
		public string HttpPort { get; set; }
		public Mictlanix.DotNet.Onvif.Media.Media Media { get; set; }
		public Mictlanix.DotNet.Onvif.Ptz.PTZ PtzController { get; set; }
		//public Profile MediaProfile { get; set; }
		public string OnvifPort { get; set; }
		//public string OnvifWsdlPath { get; set; }
		public string RtspPort { get; set; }
		public string VideoStreamUri { get; set; }
		public string HostAddress { get; set; }
		#endregion

		static async Task AsyncHelper(BaseOnvifPtzCamera camera)
		{
			camera.Camera = await OnvifClientFactory.CreateDeviceClientAsync(camera.HostAddress, camera.UserName, camera.Password);
			camera.Media = await OnvifClientFactory.CreateMediaClientAsync(camera.HostAddress, camera.UserName, camera.Password);
			camera.PtzController = await OnvifClientFactory.CreatePTZClientAsync(camera.HostAddress, camera.UserName, camera.Password);
		}

		public BaseOnvifPtzCamera(string cameraIpAddress, string userName, string password, string cameraName)
			: base(cameraIpAddress, userName, password, cameraName)
		{
			this.IsPtzMoveRelative = false;
			this.PtzTrackingThreshold = 50;
			this.PtzMovementSmallThreshold = 80;
			this.PanTiltSleepLongSeconds = 1.0;
			this.PanTileSleepShortSeconds = 0.3;
		}

		public BaseOnvifPtzCamera(string cameraIpAddress, string userName, string password,
			string cameraName, string onvifPort, string onvifWsdlPath) 
			: this(cameraIpAddress, userName, password, cameraName)
		{
			this.OnvifPort = onvifPort;
			this.HostAddress = String.Format("{0}:{1}", this.CameraIpAddress, this.OnvifPort);
			//this.OnvifWsdlPath = onvifWsdlPath;

			Helpers.Globals._log.Debug(string.Format("Connecting to camera at {0}", this.CameraIpAddress));

			AsyncHelper(this).Wait();

			/*
			//this.Camera = Onvif.PTZService //IPCameraFactory.GetCamera(String.Format("{0}:{1}", this.CameraIpAddress, this.OnvifPort), this.UserName, this.Password); //ONVIFCamera(camera_ip_address, this._onvif_port, this._username, this._password, onvif_wsdl_path);
			this.Camera = new zzzOnvifCameraController(false);
			this.Camera.Initialise(String.Format("{0}:{1}", this.CameraIpAddress, this.OnvifPort), this.UserName, this.Password);
			//IIPCamera camera = (IIPCamera)this.Camera;
			//camera.Start();
			*/


			Helpers.Globals._log.Debug("Camera connected");

			// todo: do we still need this?
			/*
			// Create media service object
			this._camera.devicemgmt.GetServices(false); 
			this._media = this._camera.create_media_service();
            this._media_profile = this._media.GetProfiles()[0];

            // get video stream uri
            var getstreamobj = this._media.create_type("GetStreamUri");
            getstreamobj.ProfileToken = this._media_profile.token;
            getstreamobj.StreamSetup = new Dictionary<object, object> {
                {
                    "Stream",
                    "RTP-Unicast"},
                {
                    "Transport",
                    new Dictionary<object, object> {
                        {
                            "Protocol",
                            "TCP"}}}};

            this._videostream_uri = this._media.GetStreamUri(getstreamobj);

            // get list of supported network protocols
            none network_protocols = this._camera.devicemgmt.GetNetworkProtocols();

            // store http and rtsp ports
            foreach (var protocol in network_protocols) {
                if (protocol.Name == "HTTP") {
                    this._http_port = protocol.Port[0];
                } else if (protocol.Name == "RTSP") {
                    this._rtsp_port = protocol.Port[0];
                }
            }
			*/
			Helpers.Globals._log.Debug("ONVIF Port: " + this.OnvifPort.ToString());
		}


		#region Public Methods
		public virtual void SetTilt(int tilt_amt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetTiltRelative(tilt_amt);
			}
			else
			{
				this.SetTiltContinuous(tilt_amt);
			}
		}

		public virtual void SetPan(int pan_amt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetPanRelative(pan_amt);
			}
			else
			{
				this.SetPanContinuous(pan_amt);
			}
		}

		public virtual void SetZoom(int zoom_amt)
		{
			if (this.IsPtzMoveRelative)
			{
				this.SetZoomRelative(zoom_amt);
			}
			else
			{
				this.SetZoomContinuous(zoom_amt);
			}
		}

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
				Helpers.Globals._log.Error(ex);
			}
		}

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
				Helpers.Globals._log.Error(ex);
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

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
					Helpers.Globals._log.Error(ex);
				}
			}
		}

		public virtual bool IsFarToMove(int panAmt, int tiltAmt)
		{
			return (Math.Abs(panAmt) > this.PtzMovementSmallThreshold) | (Math.Abs(tiltAmt) > this.PtzMovementSmallThreshold);
		}

		public override void CloseVideo()
		{
			try
			{
				if (this.VideoStreamer != null)
				{
					//this._videostream.stop(); // todo:
				}
			}
			catch (Exception detail)
			{
				Helpers.Globals._log.Error(detail);
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
					var frame = this.VideoStreamer.QueryFrame(); //.read();
					if (frame == null)
					{
						throw new ArgumentNullException("Empty frame was returned by videostream");
					}
					/*if (this._isinverted) {
                        // the camera is inverted, so flip the video feed.
                        frame = cv2.flip(frame, 0);
                    }*/
					return frame;
				}
				else
				{
					throw new ArgumentNullException("Cannot get frame because there is no videostream");
				}
			}
			catch (Exception detail)
			{
				Helpers.Globals._log.Error(detail);
			}
			return null;

		}

		public virtual void SetZoomContinuous(object tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void ExecuteZoomContinuous()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		// private overrides
		public virtual void SetTiltContinuous(object tiltAmt)
		{
			/*
			if (tilt_amt != 0 && abs(tilt_amt) > this._ptz_tracking_threshold)
			{
				if (this._ptz_move_request.Velocity == null)
				{
					this._ptz_move_request.Velocity = copy.deepcopy(this._ptz_status.Position);
				}
				if (this._isinverted)
				{
					tilt_amt = tilt_amt * -1;
				}
			}
			else
			{
				tilt_amt = 0;
			}
			if (this._ptz_move_request.Velocity != null)
			{
				this._ptz_move_request.Velocity.PanTilt.y = tilt_amt;
			}
			*/
		}

		public virtual void SetPanContinuous(object panAmt)
		{
			/*
			if (pan_amt != 0 && abs(pan_amt) > this._ptz_tracking_threshold)
			{
				if (this._ptz_move_request.Velocity == null)
				{
					this._ptz_move_request.Velocity = copy.deepcopy(this._ptz_status.Position);
				}
				if (!this._isinverted)
				{
					pan_amt = pan_amt * -1;
				}
			}
			else
			{
				pan_amt = 0;
			}
			if (this._ptz_move_request.Velocity != null)
			{
				this._ptz_move_request.Velocity.PanTilt.x = pan_amt;
			}
			*/
		}

		public virtual void ExecutePanTiltContinuous()
		{
			/*
			var pan_amt = this._ptz_move_request.Velocity.PanTilt.x;
			var tilt_amt = this._ptz_move_request.Velocity.PanTilt.y;
			this._ptz_service.ContinuousMove(this._ptz_move_request);
			if (abs(pan_amt) > 80 || abs(tilt_amt > 80))
			{
				// Wait for camera to move longer as there's further to move
				time.sleep(this._pan_tilt_sleep_long);
			}
			else
			{
				// Wait for camera to move only a short while
				time.sleep(this._pan_tilt_sleep_short);
			}
			// Stop continuous move
			this.stopPTZ();
			*/
		}

		public virtual void SetTiltRelative(object tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void SetPanRelative(object panAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}
	
		public virtual void ExecutePanTiltRelative()
		{
			this.StopPtz();
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void SetZoomRelative(object tilt_amt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual void ExecuteZoomRelative()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		public virtual bool IsFarToMove(object pan_amt, object tilt_amt)
		{
			// todo:
			//return abs(pan_amt) > this._ptz_movement_small_threshold || abs(tilt_amt > this._ptz_movement_small_threshold);
			return false;
		}

		public virtual void StopPtz()
		{
			/*
			// Stop continuous move
			this._ptz_service.Stop(new Dictionary<object, object> {
				{
					"ProfileToken",
					this._ptz_move_request.ProfileToken}});
					*/
		}


		#endregion

		#region  Protected Abstract Methods

		public abstract override void OpenVideoImpl();

		#endregion
	}
}
