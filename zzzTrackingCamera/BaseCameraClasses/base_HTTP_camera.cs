
using cv2;

using base_camera = base_camera_classes.base_camera.base_camera;

using globals = helpers.globals;

public static class base_HTTP_camera {
    
    public class base_HTTP_camera
        : base_camera {
        
        public object _http_port;
        
        public object _rtsp_port;
        
        public base_HTTP_camera(
            object camera_ip_address,
            object username,
            object password,
            object camera_name,
            object http_port,
            object rtsp_port)
            : base(camera_ip_address, username, password, camera_name) {
            this._http_port = http_port;
            this._rtsp_port = rtsp_port;
            globals.logger.debug("HTTP Port: " + this._http_port.ToString());
            globals.logger.debug("RTSP Port: " + this._rtsp_port.ToString());
        }
        
        // private overrides
        public virtual object _get_frame_impl() {
            try {
                if (this._videostream != null) {
                    // grab the frame from the video stream
                    var frame = this._videostream.read();
                    if (frame == null) {
                        throw new ValueError("Empty frame was returned by videostream");
                    }
                    if (this._isinverted) {
                        // the camera is inverted, so flip the video feed.
                        frame = cv2.flip(frame, 0);
                    }
                    return frame;
                } else {
                    throw new ValueError("Cannot get frame because there is no videostream");
                }
            } catch (Exception) {
                globals.logger.error(detail);
            }
            return null;
        }
    }
}
