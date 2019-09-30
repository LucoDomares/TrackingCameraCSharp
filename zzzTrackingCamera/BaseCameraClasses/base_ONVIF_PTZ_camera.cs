
using copy;

using time;

using base_ONVIF_camera = base_camera_classes.base_ONVIF_camera.base_ONVIF_camera;

using base_PTZ_camera = base_camera_classes.base_PTZ_camera.base_PTZ_camera;

using System.Collections.Generic;

using System;

public static class base_ONVIF_PTZ_camera {
    
    public class base_ONVIF_PTZ_camera
        : base_PTZ_camera, base_ONVIF_camera {
        
        public object _ptz_configuration_options;
        
        public object _ptz_move_request;
        
        public object _ptz_service;
        
        public object _ptz_status;
        
        public object _XMAX;
        
        public object _XMIN;
        
        public object _YMAX;
        
        public object _YMIN;
        
        public base_ONVIF_PTZ_camera(
            object camera_ip_address,
            object username,
            object password,
            object camera_name,
            object onvif_port,
            object onvif_wsdl_path) {
            // multiple inheritance
            base_PTZ_camera.@__init__(this, camera_ip_address, username, password, camera_name);
            base_ONVIF_camera.@__init__(this, camera_ip_address, username, password, camera_name, onvif_port, onvif_wsdl_path);
            this._ptz_status = null;
            this._ptz_service = null;
            this._ptz_move_request = null;
            this._ptz_configuration_options = null;
            // Create PTZ service object
            this._ptz_service = this._camera.create_ptz_service();
            // Get PTZ configuration options for getting continuous move range
            @"
		ptz_config_options_request = self._ptz_service.create_type('GetConfigurationOptions')
		ptz_config_options_request.ConfigurationToken = self._media_profile.PTZConfiguration.token
		self._ptz_configuration_options = self._ptz_service.GetConfigurationOptions(ptz_config_options_request)
		";
            this._ptz_configuration_options = this._ptz_service.GetConfigurationOptions(this._media_profile.PTZConfiguration.token);
            // Create ptz move request
            if (this._is_ptz_move_relative == true) {
                this._ptz_move_request = this._ptz_service.create_type("RelativeMove");
            } else {
                this._ptz_move_request = this._ptz_service.create_type("ContinuousMove");
            }
            this._ptz_move_request.ProfileToken = this._media_profile.token;
            this._ptz_service.Stop(new Dictionary<object, object> {
                {
                    "ProfileToken",
                    this._media_profile.token}});
            // Get range of pan and tilt
            // NOTE: X and Y are velocity vector
            this._XMAX = this._ptz_configuration_options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Max;
            this._XMIN = this._ptz_configuration_options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Min;
            this._YMAX = this._ptz_configuration_options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Max;
            this._YMIN = this._ptz_configuration_options.Spaces.ContinuousPanTiltVelocitySpace[0].YRange.Min;
            this._ptz_status = this._ptz_service.GetStatus(new Dictionary<object, object> {
                {
                    "ProfileToken",
                    this._media_profile.token}});
            if (this._ptz_status.Position == null) {
                var gp = this._ptz_service.create_type("GetPresets");
                gp.ProfileToken = this._media_profile.token;
                var presets = this._ptz_service.GetPresets(gp);
                this._ptz_status.Position = copy.deepcopy(presets[0].PTZPosition);
            }
        }
        
        // public overrides
        public virtual object stopPTZ() {
            // Stop continuous move
            this._ptz_service.Stop(new Dictionary<object, object> {
                {
                    "ProfileToken",
                    this._ptz_move_request.ProfileToken}});
        }
        
        // private overrides
        public virtual object _set_tilt_continuous(object tilt_amt) {
            if (tilt_amt != 0 && abs(tilt_amt) > this._ptz_tracking_threshold) {
                if (this._ptz_move_request.Velocity == null) {
                    this._ptz_move_request.Velocity = copy.deepcopy(this._ptz_status.Position);
                }
                if (this._isinverted) {
                    tilt_amt = tilt_amt * -1;
                }
            } else {
                tilt_amt = 0;
            }
            if (this._ptz_move_request.Velocity != null) {
                this._ptz_move_request.Velocity.PanTilt.y = tilt_amt;
            }
        }
        
        public virtual object _set_pan_continuous(object pan_amt) {
            if (pan_amt != 0 && abs(pan_amt) > this._ptz_tracking_threshold) {
                if (this._ptz_move_request.Velocity == null) {
                    this._ptz_move_request.Velocity = copy.deepcopy(this._ptz_status.Position);
                }
                if (!this._isinverted) {
                    pan_amt = pan_amt * -1;
                }
            } else {
                pan_amt = 0;
            }
            if (this._ptz_move_request.Velocity != null) {
                this._ptz_move_request.Velocity.PanTilt.x = pan_amt;
            }
        }
        
        public virtual object _execute_pan_tilt_continuous() {
            var pan_amt = this._ptz_move_request.Velocity.PanTilt.x;
            var tilt_amt = this._ptz_move_request.Velocity.PanTilt.y;
            this._ptz_service.ContinuousMove(this._ptz_move_request);
            if (abs(pan_amt) > 80 || abs(tilt_amt > 80)) {
                // Wait for camera to move longer as there's further to move
                time.sleep(this._pan_tilt_sleep_long);
            } else {
                // Wait for camera to move only a short while
                time.sleep(this._pan_tilt_sleep_short);
            }
            // Stop continuous move
            this.stopPTZ();
        }
        
        public virtual object _set_zoom_continuous(object tilt_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _execute_zoom_continuous() {
            throw new NotImplementedException("Not Implemented.");
        }
        
        // 
        // 		if pan_amt != 0 and abs(pan_amt) > self._ptz_tracking_threshold:
        // 			if self._ptz_move_request.Translation is None:
        // 				self._ptz_move_request.Translation = copy.deepcopy(self._ptz_status.Position)
        // 
        // 		else:
        // 			pan_amt = 0
        // 
        // 		self._ptz_move_request.Translation.PanTilt.y = pan_amt
        // 		
        public virtual object _set_tilt_relative(object tilt_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        // 
        // 		if pan_amt != 0.0 and abs(pan_amt) > self._ptz_tracking_threshold:
        // 			if self._ptz_move_request.Translation is None:
        // 				self._ptz_move_request.Translation = copy.deepcopy(self._ptz_status.Position)
        // 
        // 			self._ptz_move_request.Translation.PanTilt.x = pan_amt
        // 		
        public virtual object _set_pan_relative(object pan_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        // 
        // 		self._ptz_move_request.Speed = copy.deepcopy(self._ptz_move_request.Translation)
        // 		self._ptz_move_request.Speed.PanTilt.x = 20.0
        // 		self._ptz_move_request.Speed.PanTilt.y = 20.0
        // 		self._ptz_service.RelativeMove(self._ptz_move_request)
        // 		
        public virtual object _execute_pan_tilt_relative() {
            this.stopPTZ();
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _set_zoom_relative(object tilt_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _execute_zoom_relative() {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object is_far_to_move(object pan_amt, object tilt_amt) {
            return abs(pan_amt) > this._ptz_movement_small_threshold || abs(tilt_amt > this._ptz_movement_small_threshold);
        }
    }
}
