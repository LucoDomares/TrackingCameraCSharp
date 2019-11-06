#region Using Clauses
using System.Collections;
using System;
using OpenCvSharp;
using TrackingCamera.Helpers;
using System.Collections.Concurrent;
#endregion

namespace TrackingCamera.BaseCameraClasses
{
	/// <summary>
	/// The base camera class from which all cameras descend.
	/// </summary>
	public abstract class BaseCamera
	{
		protected bool IsAutoTrackEnabled { get; set; }

		protected bool IsCalculateFrameCentre { get; set; }

		protected object Camera { get; set; }

		protected string CameraIpAddress { get; set; }

		public string CameraName { get; set; }

		public FixedSizedQueue<Mat> CurrentStreamFrameQueue { get; set; }

		private bool IsFocusWindow { get; set; }

		public bool IsContinuousRecording { get; set; }

		protected bool IsInvertedVideo { get; set; }

		private object ProcessLock { get; set; }

		protected string Password { get; set; }

		private FixedSizedQueue<Mat> ProcessedFrameQueue { get; set; }

		protected string UserName { get; set; }

		protected VideoCapture VideoStreamer { get; set; }

		public virtual bool IsSupportsPTZ => false;

		public bool IsPtzMoveRelative { get; set; }

		public string HostAddress { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="CameraIpAddress">the Ip address of the camera (without http://)</param>
		/// <param name="UserName">the user name to log in with</param>
		/// <param name="Password">the password to log in with</param>
		/// <param name="CameraName">the user friendly name of the camera</param>
		public BaseCamera(string CameraIpAddress, string UserName, string Password, string CameraName)
		{
			this.ProcessLock = new object();
			this.UserName = UserName;
			this.Password = Password;
			this.CameraIpAddress = CameraIpAddress;
			this.IsContinuousRecording = false;
			this.CurrentStreamFrameQueue = new FixedSizedQueue<Mat>(1);
			this.ProcessedFrameQueue = new FixedSizedQueue<Mat>(1);
			this.IsFocusWindow = true;
			this.IsCalculateFrameCentre = true;
			this.IsAutoTrackEnabled = false;
			if (CameraName != "")
			{
				this.CameraName = CameraName;
			}
			else
			{
				this.CameraName = this.GetType().Name;
			}
			this.Camera = null;
			this.VideoStreamer = null;
			this.IsInvertedVideo = false;
		}

		/// <summary>
		/// Destructor
		/// </summary>
		public virtual void Del()
		{
			this.CloseVideo();
		}

		#region Public methods
		/// <summary>
		/// Wrapper for the descendent open video implementation
		/// </summary>
		public virtual void OpenVideo()
		{
			try
			{
				// call implementor
				this.OpenVideoImpl();
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
			}
		}

		/// <summary>
		/// Wrapper for the descendent close video implementation
		/// </summary>
		public virtual void CloseVideo()
		{
			try
			{
				if (this.VideoStreamer != null)
				{
					// todo:
					//this._videostream.stop();
				}
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
				throw;
			}
		}

		/// <summary>
		/// Frames per second received from the camera. 
		/// </summary>
		/// <returns></returns>
		public double Fps()
		{
			return this.VideoStreamer.Fps;
		}

		/// <summary>
		/// Wrapper for the descendent get video frame implementation
		/// </summary>
		/// <returns></returns>
		public virtual Mat GetFrame()
		{
			try
			{
				// call implementor
				return this.GetFrameImpl();
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
				throw;
			}
		}

		/// <summary>
		/// Returns the most recent video frame from the Videostream.
		/// </summary>
		/// <returns>an <c>IplImage</c>.</returns>
		public Mat GetCurrentFrame()
		{
			try
			{
				// return the latest frame from the Queue.			
				this.CurrentStreamFrameQueue.TryDequeue(out Mat frame);

				return frame;
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
				throw;
			}
		}

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
		public abstract bool IsFarToMove(int panAmt, int tiltAmt);

		#region Move Continuous Commands


		/// <summary>
		/// Sets the Zoom value before Execute Zoom command is called.
		/// </summary>
		/// <param name="zoomAmt"></param>
		public virtual void SetZoomContinuous(int zoomAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Executes the Zoom command.
		/// Call SetZoomContinuous first before calling this method.
		/// </summary>
		public virtual void ExecuteZoomContinuous()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		#endregion

		#region Move Relative Commands

		/// <summary>
		/// Sets the relative Tilt Amount.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public virtual void SetTiltRelative(int tiltAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Sets the relative Pan Amount.
		/// </summary>
		/// <param name="panAmt"></param>
		public virtual void SetPanRelative(int panAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Executes the Relative PanTilt command.
		/// </summary>
		public virtual void ExecutePanTiltRelative()
		{
			this.StopPtz();
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Sets the relative Zoom Amount.
		/// </summary>
		/// <param name="zoomAmt"></param>
		public virtual void SetZoomRelative(int zoomAmt)
		{
			throw new NotImplementedException("Not Implemented.");
		}

		/// <summary>
		/// Executes the relative Zoom Command.
		/// </summary>
		public virtual void ExecuteZoomRelative()
		{
			throw new NotImplementedException("Not Implemented.");
		}

		#endregion

		#endregion

		#region  Protected Abstract Methods

		/// <summary>
		/// Implementation of the Open Video method.
		/// Must be overriden in the descending camera class.
		/// </summary>
		public abstract void OpenVideoImpl();

		/// <summary>
		/// Implementation of the Get Frame method.
		/// Must be overriden in the descending camera class.
		/// </summary>
		/// <returns></returns>
		public abstract Mat GetFrameImpl();

		/// <summary>
		/// Stops all Pan/tilt/zoom commands.
		/// </summary>
		public abstract void StopPtz();

		/// <summary>
		/// Sets the Tilt value before Execute PanTilt command is called.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public abstract void SetTiltContinuous(int tiltAmt);

		/// <summary>
		/// Sets the Pan value before Execute PanTilt command is called.
		/// </summary>
		/// <param name="tiltAmt"></param>
		public abstract void SetPanContinuous(int panAmt);

		/// <summary>
		/// Executes the Pan and Tilt command.
		/// Call SetTiltContinuous first before calling this method.
		/// Call SetPanContinuous first before calling this method.
		/// </summary>
		public abstract void ExecutePanTiltContinuous();


		#endregion
	}
}
