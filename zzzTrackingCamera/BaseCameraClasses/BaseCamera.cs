#region Using Clauses
using System.Collections;
using TrackingCamera.Helpers;
using Emgu.CV;
using System;
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
		protected string CameraName { get; set; }
		private Queue CurrentFrameQueue { get; set; }
		private bool IsFocusWindow { get; set; }
		private bool IsContinuousRecording { get; set; }
		protected bool IsInvertedVideo { get; set; }
		private object ProcessLock { get; set; }
		protected string Password { get; set; }
		private Queue ProcessedFrameQueue { get; set; }
		protected string UserName { get; set; }
		protected ICapture VideoStreamer { get; set; }
		public virtual bool IsSupportsPTZ => false;
		
		public BaseCamera(string CameraIpAddress, string UserName, string Password, string CameraName)
		{
			this.ProcessLock = new object();
			this.UserName = UserName;
			this.Password = Password;
			this.CameraIpAddress = CameraIpAddress;
			this.IsContinuousRecording = false;
			this.CurrentFrameQueue = new Queue(1);
			this.ProcessedFrameQueue = new Queue(1);
			this.IsFocusWindow = true;
			this.IsCalculateFrameCentre = true;
			this.IsAutoTrackEnabled = false;
			if (CameraName != "")
			{
				this.CameraName = CameraName;
			}
			else
			{
				this.CameraName = this.GetType().Name; // typeof(this).Name;
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
		public virtual void OpenVideo()
		{
			try
			{
				// call implementor
				this.OpenVideoImpl();
			}
			catch (Exception detail)
			{
				Helpers.Globals._log.Error(detail);
			}
		}

		public virtual void CloseVideo()
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

		public virtual object GetFrame()
		{
			try
			{
				// call implementor
				return this.GetFrameImpl();
			}
			catch (Exception detail)
			{
				Helpers.Globals._log.Error(detail);
				throw;
			}
		}

		#endregion

		#region  Protected Abstract Methods
		public abstract void OpenVideoImpl();

		public abstract object GetFrameImpl();

		#endregion
	}
}
