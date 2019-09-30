
using random;

using time;

using requests;

using base_HTTP_camera = base_camera_classes.base_HTTP_camera.base_HTTP_camera;

using base_PTZ_camera = base_camera_classes.base_PTZ_camera.base_PTZ_camera;

using globals = helpers.globals;

using System;

public static class base_HTTP_PTZ_camera {
    
    public class base_HTTP_PTZ_camera
        : base_PTZ_camera, base_HTTP_camera {
        
        public bool _generate_rnd_numbers_on_command;
        
        public object _http_session;
        
        public bool _is_ptz_move_relative;
        
        public double _pan_tilt_sleep_long;
        
        public double _pan_tilt_sleep_short;
        
        public string _ptz_command_pan;
        
        public string _ptz_command_stop;
        
        public string _ptz_command_tilt;
        
        public string _ptz_command_uri;
        
        public string _ptz_command_zoom;
        
        public int _ptz_movement_small_threshold;
        
        public object _ptz_pan_amt;
        
        public object _ptz_tilt_amt;
        
        public int _ptz_tracking_threshold;
        
        public object _ptz_zoom_amt;
        
        public bool _stop_flag;
        
        public object supportsPTZ {
            get {
                return true;
            }
        }
        
        public base_HTTP_PTZ_camera(
            object camera_ip_address,
            object username,
            object password,
            object camera_name,
            object http_port,
            object rtsp_port)
            : base(camera_ip_address, username, password, camera_name) {
            base_HTTP_camera.@__init__(this, camera_ip_address, username, password, camera_name, http_port, rtsp_port);
            this._is_ptz_move_relative = false;
            this._ptz_tracking_threshold = 50;
            this._ptz_movement_small_threshold = 80;
            this._pan_tilt_sleep_long = 1.0;
            this._pan_tilt_sleep_short = 0.8;
            this._generate_rnd_numbers_on_command = false;
            this._stop_flag = false;
            this._ptz_pan_amt = 0;
            this._ptz_tilt_amt = 0;
            this._ptz_zoom_amt = 0;
            this._ptz_command_uri = "http://{}:{}@{}:{}".format(this._username, this._password, this._camera_ip_address, this._http_port);
            this._ptz_command_pan = "";
            this._ptz_command_tilt = "";
            this._ptz_command_zoom = "";
            this._ptz_command_stop = "";
            // Create the session for all http requests to go through
            this._http_session = requests.Session();
            // workaround: make a test HTTP request to force the session.proxy to be set.
            // if we dont do this, the PTZTask thread hangs whien getting default proxy info from the Operating System.xxxa
            var r = this._http_session.get("http://www.google.com/");
            // stop any existing PTZ activity
            this.stopPTZ();
        }
        
        // public overrides
        public virtual object stopPTZ() {
            this._execute_command_http(this._ptz_command_stop);
            this._stop_flag = false;
        }
        
        // privates
        public virtual object _execute_command_http(object command_url) {
            if (command_url != "") {
                try {
                    if (this._generate_rnd_numbers_on_command) {
                        command_url = command_url + random.randint(1000000000000000, 9999999999999999).ToString();
                    }
                    globals.logger.info("PTZ: " + command_url);
                    var r = this._http_session.get(command_url, timeout: 4.5);
                    if (r.status_code != 200) {
                        // OK
                        globals.logger.error("bad command result - " + r.reason);
                    }
                } catch (Exception) {
                    globals.logger.error(detail);
                }
            }
        }
        
        // private overrides
        // this method should be overridden in the descendant class and called down to before implementing descendant logic
        public virtual object _set_tilt_continuous(object tilt_amt) {
            this._ptz_tilt_amt = tilt_amt;
        }
        
        // this method should be overridden in the descendant class and called down to before implementing descendant logic
        public virtual object _set_pan_continuous(object pan_amt) {
            this._ptz_pan_amt = pan_amt;
        }
        
        // this method should be overridden in the descendant class and called down to before implementing descendant logic
        public virtual object _set_zoom_continuous(object zoom_amt) {
            this._ptz_zoom_amt = zoom_amt;
        }
        
        public virtual object _execute_pan_tilt_continuous() {
            object command_url;
            var has_panned_tilt_zoomed = false;
            if (this._ptz_pan_amt != 0) {
                command_url = this._ptz_command_uri.format(this._ptz_command_pan);
                this._execute_command_http(command_url);
                has_panned_tilt_zoomed = true;
            }
            if (this._ptz_tilt_amt != 0) {
                command_url = this._ptz_command_uri.format(this._ptz_command_tilt);
                this._execute_command_http(command_url);
                has_panned_tilt_zoomed = true;
            }
            if (this._ptz_zoom_amt != 0) {
                command_url = this._ptz_command_uri.format(this._ptz_command_zoom);
                this._execute_command_http(command_url);
                has_panned_tilt_zoomed = true;
            }
            if (has_panned_tilt_zoomed) {
                if (abs(this._ptz_pan_amt) > 80 || abs(this._ptz_tilt_amt > 80)) {
                    // Wait for camera to move longer as there's further to move
                    globals.logger.info("Sleeping for {} seconds".format(this._pan_tilt_sleep_long));
                    time.sleep(this._pan_tilt_sleep_long);
                } else {
                    // Wait for camera to move only a short while
                    globals.logger.info("Sleeping for {} seconds".format(this._pan_tilt_sleep_short));
                    time.sleep(this._pan_tilt_sleep_short);
                }
                // Stop continuous move
                this.stopPTZ();
            } else if (this._stop_flag) {
                this.stopPTZ();
            }
        }
        
        public virtual object _execute_zoom_continuous() {
            if (this._ptz_command_zoom != "") {
                var command_url = this._ptz_command_uri.format(this._ptz_command_zoom);
                this._execute_command_http(command_url);
                // Wait for camera to zoom only a short while
                time.sleep(this._pan_tilt_sleep_short);
            }
            // Stop continuous move
            this.stopPTZ();
        }
        
        // relative movement functions not used by this class
        public virtual object _set_tilt_relative(object tilt_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _set_pan_relative(object pan_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _execute_pan_tilt_relative() {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _set_zoom_relative(object tilt_amt) {
            throw new NotImplementedException("Not Implemented.");
        }
        
        public virtual object _execute_zoom_relative() {
            throw new NotImplementedException("Not Implemented.");
        }
    }
}
