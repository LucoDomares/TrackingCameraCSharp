#region Using Clauses
using System.Collections;
using TrackingCamera.Helpers;
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
		protected object VideoStreamer { get; set; }
		public virtual bool IsSupportsPTZ => false;
		
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
		/// Wrapper for the descendent get video frame implementation
		/// </summary>
		/// <returns></returns>
		public virtual object GetFrame()
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

		#endregion

		#region  Protected Abstract Methods
		public abstract void OpenVideoImpl();

		public abstract object GetFrameImpl();

		#endregion
	}
}
