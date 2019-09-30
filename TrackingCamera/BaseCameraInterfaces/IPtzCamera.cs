using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingCamera.BaseCameraClasses;

namespace TrackingCamera.BaseCameraInterfaces
{
	/// <summary>
	/// Interface for a camera supporting Point To Zoom (Ptz) camera functions.
	/// </summary>
	public interface IPtzCamera 
	{
		bool IsPtzMoveRelative { get; set; }

		int PanTiltSleepLongMilliseconds { get; set; }

		int PanTileSleepShortMilliseconds { get; set; }

		int PtzMovementSmallThreshold { get; set; }

		int PtzTrackingThreshold { get; set; }

		int PtzPanAmt { get; set; }

		int PtzTiltAmt { get; set; }

		int PtzZoomAmt { get; set; }


		#region Public Methods
		void SetTilt(int tilt_amt);

		void SetPan(int pan_amt);

		void SetZoom(int zoom_amt);

		void ExecutePanTilt();

		void ExecuteZoom();

		void MoveUp(object locker);

		void MoveDown(object locker);

		void MoveLeft(object locker);

		void MoveRight(object locker);

		void ZoomOut(object locker);

		void ZoomIn(object locker);

		bool IsFarToMove(int PanAmt, int TiltAmt);

		#endregion

		#region  Protected Abstract Methods

		void StopPtz();

		void OpenVideoImpl();

		object GetFrameImpl();

		void SetTiltContinuous(int tiltAmt);

		void SetPanContinuous(int panAmt);

		void ExecutePanTiltContinuous();

		void SetZoomContinuous(int zoomAmt);

		void ExecuteZoomContinuous();

		void SetTiltRelative(int tiltAmt);

		void SetPanRelative(int panAmt);

		void ExecutePanTiltRelative();

		void SetZoomRelative(int zoomAmt);

		void ExecuteZoomRelative();

		#endregion
	}
}
