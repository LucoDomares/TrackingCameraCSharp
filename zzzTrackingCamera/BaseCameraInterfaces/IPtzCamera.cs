using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingCamera.BaseCameraClasses;

namespace TrackingCamera.BaseCameraInterfaces
{
	/// <summary>
	/// Interface supporting Point To Zoom Camera functions.
	/// </summary>
	public interface IPtzCamera 
	{
		bool IsPtzMoveRelative { get; set; }

		double PanTiltSleepLongSeconds { get; set; }

		double PanTileSleepShortSeconds { get; set; }

		int PtzMovementSmallThreshold { get; set; }

		int PtzTrackingThreshold { get; set; }


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

		void SetTiltContinuous(object tilt_amt);

		void SetPanContinuous(object pan_amt);

		void ExecutePanTiltContinuous();

		void SetZoomContinuous(object tilt_amt);

		void ExecuteZoomContinuous();

		void SetTiltRelative(object tilt_amt);

		void SetPanRelative(object pan_amt);

		void ExecutePanTiltRelative();

		void SetZoomRelative(object tilt_amt);

		void ExecuteZoomRelative();

		#endregion
	}
}
