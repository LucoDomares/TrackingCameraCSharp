using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using TrackingCamera.BaseCameraClasses;
using TrackingCamera.CameraClasses;
using TrackingCamera.DetectorClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraManagers
{
	/// <summary>
	/// The Base Camera Manager class.
	/// </summary>
	public class BaseCameraManager
	{
		/// <summary>
		/// The camera being managed.
		/// </summary>
		protected virtual BasePtzCamera Camera { get; set; }

		/// <summary>
		/// The configuration of the camera.
		/// </summary>
		protected CameraConfig CameraConfig { get; set; }

		/// <summary>
		/// The locking object for Ptz control.
		/// </summary>
		readonly object ptzLock = new object();

		/// <summary>
		/// The locking object for the video stream.
		/// </summary>
		readonly object videoLock = new object();

		/// <summary>
		/// Indicates if the system is on high alert.
		/// <para>This is usually <c>True</c> if faces have been detected in the video feed recently.</para>
		/// <para>Otherwise, if no feces have been found for a while, <c>False</c></para>
		/// </summary>
		bool IsOnHighAlert { get; set; } = false;

		/// <summary>
		/// Indicates if Ptz auto tracking of faces is enabled.
		/// </summary>
		bool IsAutoTracking { get; set; } = false;

		/// <summary>
		/// The amount to Pan the camera left/right. +/- values indicate left and right movement.
		/// </summary>
		int PanAmt { get; set; } = 0;

		/// <summary>
		/// The amount to Tilt the camera up/down. +/- values indicate up and down movement.
		/// </summary>
		int TiltAmt { get; set; } = 0;

		/// <summary>
		/// Indicates if the Task is stopping.
		/// </summary>
		/// <remarks>Returns <c>True</c> if stopping, <c>False</c> otherwise.</remarks>
		protected bool IsStopping { get; set; } = false;

		/// <summary>
		/// Indicates if the Task has been stopped
		/// </summary>
		/// <remarks>Returns <c>True</c> if stopped, <c>False</c> otherwise.</remarks>
		public bool IsStopped { get; set; } = false;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="camera">The camera being managed.</param>
		/// <param name="cameraConfig">The config of the camera being managed.</param>
		public BaseCameraManager(BaseOnvifPtzCamera camera, CameraConfig cameraConfig) 
		{
			this.Camera = camera;
			this.CameraConfig = cameraConfig;
		}


		/// <summary>
		/// Runs the manager Ansyncronously.
		/// </summary>
		public async void RunAsync()
		{
			// start asynchronous ptz Controller. Don't wait for results.
			Task ptzController = ManagePtzControl();

			// start asynchronous video stream monitor. Don't wait for results.
			Task videostreamManager = ManageVideoStream();

			// manage video stream - this is a blocking call.
			await ProcessFrames();

			// if we get here, then managing of the video stream has stopped/completed.
			// mark the process as stopped.
			this.IsStopped = true;
		}

		/// <summary>
		/// Runs the Ptz controller Asyncronously.
		/// </summary>
		/// <returns>An Asyncronous <c>Task</c></returns>
		private async Task ManagePtzControl()
		{
			if (this.Camera == null)
			{
				Globals.Log.Error("Camera device not set. Shutting down.");
				return;
			}

			// loop indefinitely, controlling camera movement as described by PanAmt, TiltAmt.
			// todo: Implement ZoomAmt control.
			while (!this.IsStopping)
			{
				if ((this.Camera.PtzPanAmt == 0) || (this.Camera.PtzTiltAmt == 0))
				{
					// nothing to do, yield to other processes for a while
					await Task.Delay(100);
				}
				else
				{
					if (!this.Camera.IsTargetCentered())
					{
						lock (this.ptzLock)
						{
							this.Camera.SetPan(this.PanAmt);
							this.Camera.SetTilt(this.TiltAmt);
						}

						// execute the pan/tilt
						this.Camera.ExecutePanTilt();

						// sleep for a while
						await Task.Delay(50);
					}
					else
					{
						if (!IsOnHighAlert)
						{
							// sleep for a while
							await Task.Delay(50);
						}
					}
				}
			}
		}

		/// <summary>
		/// Continually retrieves the latest video frame from the stream.
		/// </summary>
		/// <returns>An Asyncronous <c>Task</c></returns>
		private async Task ManageVideoStream()
		{
			if (this.Camera == null)
			{
				Globals.Log.Error("Camera device not set. Shutting down.");
				return;
			}

			// loop indefinitely, pulling the latest frame from the video feed continuously.
			while (!this.IsStopping)
			{
				Mat frame = this.Camera.GetFrame();
				if (frame != null)
				{
					frame = Helpers.VideoTools.ResizeIplTo(frame, 640, 360);
					Camera.CurrentStreamFrameQueue.Enqueue(frame);
				}

				// Yield to other processes.
				await Task.Delay(1);
			}
		}

		/// <summary>
		/// Performs processing of the most recent frame from the video feed. 
		/// This is an Ansyncronous method.
		/// </summary>
		/// <returns>An Asyncronous <c>Task</c></returns>
		private async Task ProcessFrames()
		{
			if (this.Camera == null)
			{
				Globals.Log.Error("Camera device not set. Shutting down.");
				return;
			}

			string detectorPath = ConfigurationManager.AppSettings["DetectorPath"];
			string basePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["CameraSettingsPath"]);
			string detectorPathFull = System.IO.Path.Join(basePath, detectorPath);
			string protoFile = ConfigurationManager.AppSettings["ProtoFile"];
			string detectorModelFile = ConfigurationManager.AppSettings["DetectorModelFile"];
			string embeddingModelFile = ConfigurationManager.AppSettings["EmbeddingModelFile"];
			string recogniserModelFile = ConfigurationManager.AppSettings["RecogniserModelFile"];
			string labelEncoderFile = ConfigurationManager.AppSettings["LabelEncoderFile"];

			// build the detector.
			DnnCaffeFaceDetector detector = new DnnCaffeFaceDetector(CameraConfig.MinConfidence,
				detectorPathFull, protoFile, detectorModelFile, embeddingModelFile, recogniserModelFile,
				labelEncoderFile);

			// show window.
			Cv2.NamedWindow(this.Camera.CameraName, WindowMode.AutoSize);
			bool resizeWindow = true;
			int frameCenterX = 0;
			int frameCenterY = 0;
			FacesList facesList;

			// loop indefinitely.
			while (!this.IsStopping)
			{
				try
				{
					int fps = Convert.ToInt32(this.Camera.Fps());
					Mat frame = this.Camera.GetCurrentFrame();

					if (frame != null)
					{
						if (this.IsAutoTracking)
						{
							// Todo: Implement Auto Tracking.
							if (frameCenterX == 0)
							{
								// calculate center of frame.
								frameCenterX = frame.Width / 2;
								frameCenterY = frame.Height / 2;
							}

							// process faces in the frame.
							frame = detector.Detect(frame, out facesList);

							// todo: track highest confidence face
						}

						// On Screen Display (OSD) variables.
						string autoTrackText = "";
						string recordingText = "";
						string watermarkText = "";
						Scalar rgbColor;

						// set up text for OSD.
						if (this.IsAutoTracking)
						{
							autoTrackText = "ON";
						}
						else
						{
							autoTrackText = "OFF";
						}

						if (this.Camera.IsContinuousRecording)
						{
							recordingText = "ON";
						}
						else
						{
							recordingText = "OFF";
						}

						// set up colours for OSD.
						if (this.Camera.IsContinuousRecording)
						{
							rgbColor = new Scalar(0, 0, 255);
						}
						else if (this.IsAutoTracking)
						{
							rgbColor = new Scalar(0, 255, 0);
						}
						else
						{
							rgbColor = new Scalar(0, 255, 255);
						}

						watermarkText = string.Format("Auto Tracking: {0}    Continuous Recording: {1}    FPS: {2}", autoTrackText, recordingText, fps);

						Point watermarkLocation = new Point(10, 22);
						Cv2.PutText(frame, watermarkText, watermarkLocation, HersheyFonts.HersheyComplex, 0.9, rgbColor);

						if (resizeWindow)
						{
							Cv2.ResizeWindow(this.Camera.CameraName, frame.Width, frame.Height);
							resizeWindow = false;
						}
						Cv2.ImShow(this.Camera.CameraName, frame);
					}

					int key = Cv2.WaitKeyEx((int)(1000 / fps));
					if (key == 'q')
					{
						break;
					}

					switch (key)
					{
						case 2490368: // Up Arrow Key
							Camera.MoveUp(this.ptzLock);
							break;
						case 2621440:  // Down Arrow Key  
							Camera.MoveDown(this.ptzLock);
							break;
						case 2424832:  // Left Arrow Key
							Camera.MoveLeft(this.ptzLock);
							break;
						case 2555904:  // Right Arrow Key
							Camera.MoveRight(this.ptzLock);
							break;
						case 'z':
							Camera.ZoomIn(this.ptzLock);
							break;
						case 'x':
							Camera.ZoomOut(this.ptzLock);
							break;
						case 'a':
							this.IsAutoTracking = VideoTools.FlipBool(this.IsAutoTracking);

							if (!this.IsAutoTracking)
							{
								Camera.StopPtz();
							}
							break;
						case 'c':
							Camera.IsContinuousRecording = VideoTools.FlipBool(Camera.IsContinuousRecording);
							break;

					}
				}
				catch (Exception detail)
				{
					Globals.Log.Error(detail);
				}
			}
		}

		public void Stop()
		{
			this.IsStopping = true;
		}

	}
}
