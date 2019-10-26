using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using TrackingCamera.BaseCameraClasses;
using TrackingCamera.CameraClasses;
using TrackingCamera.Helpers;

namespace TrackingCamera.CameraManagers
{
	class OnvifCameraManager : BaseCameraManager
	{
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
		/// The camera being managed.
		/// </summary>
		BaseOnvifPtzCamera Camera { get; set; }

		/// <summary>
		/// The configuration of the camera.
		/// </summary>
		CameraConfig CameraConfig { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="camera">The camera being managed.</param>
		/// <param name="cameraConfig">The config of the camera being managed.</param>
		public OnvifCameraManager(BaseOnvifPtzCamera camera, CameraConfig cameraConfig)
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
			await ProcessLatestFrame();

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
			// loop indefinitely, controlling camera movement as described by PanAmt, TiltAmt.
			// todo: Implement ZoomAmt control.
			while (!this.IsStopping)
			{
				if ((this.Camera.PtzPanAmt == 0) || (this.Camera.PtzTiltAmt == 0))
				{
					// nothing to do, yield to other processes for a while
					await Task.Delay(500);
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
						await Task.Delay(150);
					}
					else
					{
						if (!IsOnHighAlert)
						{
							// sleep for a while
							await Task.Delay(150);
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
			// loop indefinitely, pulling the latest frame from the video feed continuously.
			while (!this.IsStopping)
			{
				IplImage frame = this.Camera.GetFrame();
				if (frame != null)
				{
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
		private async Task ProcessLatestFrame()
		{
			using (CvWindow cameraWindow = new CvWindow(this.Camera.CameraName, WindowMode.AutoSize))
			{
				// loop indefinitely
				while (!this.IsStopping)
				{
					try
					{
						int fps = Convert.ToInt32(this.Camera.Fps());
						IplImage frame = this.Camera.GetCurrentFrame();

						if (frame != null)
						{
							frame = Helpers.VideoTools.ResizeIplTo(frame, 1280, 720);

							if (this.IsAutoTracking)
							{
								// Todo: Implement Auto Tracking.
								int frameCenterX = 0;
								int frameCenterY = 0;

								if (frameCenterX == 0)
								{
									// calculate center of frame.
									frameCenterX = frame.Width / 2;
									frameCenterY = frame.Height / 2;
								}

								// Todo: detect and process faces in the frame.
								
							}

							// On Screen Display (OSD) variables.
							string autoTrackText = "";
							string recordingText = "";
							string watermarkText = "";
							CvScalar rgbColor;

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
								rgbColor = new CvScalar(0, 0, 255);
							}
							else if (this.IsAutoTracking)
							{
								rgbColor = new CvScalar(0, 255, 0);
							}
							else
							{
								rgbColor = new CvScalar(0, 255, 255);
							}

							watermarkText = string.Format("Auto Tracking: {0}    Continuous Recording: {1}    FPS: {2}", autoTrackText, recordingText, fps);

							CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7);
							CvPoint watermarkLocation = new CvPoint(10, 22);
							Cv.PutText(frame, watermarkText, watermarkLocation, font, rgbColor);

							cameraWindow.ShowImage(frame);
						}

						int key = CvWindow.WaitKey((int)(1000 / fps)); // & 0xFF;
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
		}
	}
}
