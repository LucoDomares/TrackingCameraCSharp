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

		public FixedSizedQueue<IplImage> CurrentStreamFrameQueue { get; set; }

		private bool IsFocusWindow { get; set; }

		public bool IsContinuousRecording { get; set; }

		protected bool IsInvertedVideo { get; set; }

		private object ProcessLock { get; set; }

		protected string Password { get; set; }

		private FixedSizedQueue<IplImage> ProcessedFrameQueue { get; set; }

		protected string UserName { get; set; }

		protected CvCapture VideoStreamer { get; set; }

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
			this.CurrentStreamFrameQueue = new FixedSizedQueue<IplImage>(1);
			this.ProcessedFrameQueue = new FixedSizedQueue<IplImage>(1);
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
		public virtual IplImage GetFrame()
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
		public IplImage GetCurrentFrame()
		{
			try
			{
				// return the latest frame from the Queue.
				IplImage frame;
				
				this.CurrentStreamFrameQueue.TryDequeue(out frame);
				return frame;
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
				throw;
			}
		}

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
		public abstract IplImage GetFrameImpl();

		#endregion
	}
}
