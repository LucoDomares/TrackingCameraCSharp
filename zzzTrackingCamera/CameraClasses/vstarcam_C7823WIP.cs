using TrackingCamera.BaseCameraClasses;
using TrackingCamera.BaseCameraInterfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using TrackingCamera.OnvifMedia10;

namespace TrackingCamera.CameraClasses
{
	public class Vstarcam_C7823WIP : BaseOnvifPtzCamera, IOnvifCamera
	{
	
		new public object VideoStreamer;

		public Vstarcam_C7823WIP(
			string camera_ip_address,
			string username,
			string password,
			string camera_name,
			string onvif_port,
			string onvif_wsdl_path)
			: base(camera_ip_address, username, password, camera_name, onvif_port, onvif_wsdl_path)
		{
			
		}

		// overrides

		public override void OpenVideoImpl()
		{
			if (this.VideoStreamer != null)
			{
				this.CloseVideo();
			}
			var address_full = "";
			if (this.VideoStreamUri != null)
			{
				string addressPrefix = "http://";
				string addressSuffix = "/livestream.cgi?";
				string addressUserSuffix = "user=";
				string addressPasswordSuffix = "&pwd=";
				string AddressFinalSuffix = "&streamid=0";
				address_full = string.Format("{0}{1}:{2}{3}{4}{5}{6}{7}{8}", addressPrefix, this.CameraIpAddress, this.HttpPort.ToString(), addressSuffix, addressUserSuffix, this.UserName, addressPasswordSuffix, this.Password, AddressFinalSuffix);
			}
			//IIPCamera camera = (IIPCamera)this.Camera;
			//this.VideoStreamer = camera.AvailableStreams[0]; //. VideoStream(address_full).start();
			//////self._videostream = VideoStream("Lamby.mov").start()

			//time.sleep(0.5);
		}
	}

	public class Vstarcam_C7823WIPCameraBuilder : CameraBuilder
	{

		public Vstarcam_C7823WIP _instance;

		public Vstarcam_C7823WIPCameraBuilder()
		{
			this._instance = null;
		}
	
		public override BaseCamera Build(string onvif_wsdl_path, Dictionary<string, string> settings)
		{
			var camera_ip_address = settings["ip_addr"];
			var username = settings["username"];
			var password = settings["password"];
			var camera_name = settings["camera_name"];
			var onvif_port = settings["onvif_port"];
			if (this._instance == null)
			{
				this._instance = new Vstarcam_C7823WIP(camera_ip_address, username, password, camera_name, onvif_port, onvif_wsdl_path);
			}
			return this._instance;
		}
	}


	public class Vstarcam_C7823WIPTaskBuilder : CameraBuilder
	{
		public Thread _instance;

		public Vstarcam_C7823WIPTaskBuilder()
		{
			this._instance = null;
		}

		public Thread Start(string onvif_wsdl_path, Dictionary<string, string> camera_settings, Dictionary<string, string> appsettings, Hashtable _ignored)
		{
			string camera_ip_address = camera_settings["ip_addr"];
			string username = camera_settings["username"];
			string password = camera_settings["password"];
			string camera_name = camera_settings["camera_name"];
			string onvif_port = camera_settings["onvif_port"];
			string proto_file = appsettings["proto_file"];
			string detector_path = appsettings["detector_path"];
			string detector_model_file = appsettings["detector_model_file"];
			string embedding_model_file = appsettings["embedding_model_file"];
			string recogniser_model_file = appsettings["recogniser_model_file"];
			string label_encoder_file = appsettings["label_encoder_file"];
			string min_confidence = appsettings["min_confidence"];
			//
			//commands = 'python tasks/task_vStarCam_camera.py '
			//args = '-i ' + camera_ip_address + ', -o ' + str(onvif_port) + ', -u ' + username + ', -p ' + password + \
			//	', -w ' + onvif_wsdl_path + ', --prototxt ' + proto_file + ', --model ' + detector_model_file + \
			//	', --confidence ' + str(min_confidence) + ', --detector ' + detector_path + ', --embedder ' + \
			//	embedding_model_file + ', --recognizer ' + recogniser_model_file + ', --labelencoder ' + \
			//	label_encoder_file + ', --cameraname ""' + camera_name + '""'
			//

			var arg1 = "-i" + camera_ip_address;
			var arg2 = "-o" + onvif_port.ToString();
			var arg3 = "-u" + username;
			var arg4 = "-p" + password;
			var arg5 = "-w" + onvif_wsdl_path;
			var arg6 = "-t" + proto_file;
			var arg7 = "-m" + detector_model_file;
			var arg8 = "-c" + min_confidence.ToString();
			var arg9 = "-d" + detector_path;
			var arg10 = "-e" + embedding_model_file;
			var arg11 = "-r" + recogniser_model_file;
			var arg12 = "-l" + label_encoder_file;
			var arg13 = "-n\"" + camera_name + "\"";

			// todo: spawn a process thread?
			/*
			var proc = subprocess.Popen(new List<string> {
				"python",
				"tasks/task_vStarCam_camera.py",
				arg1,
				arg2,
				arg3,
				arg4,
				arg5,
				arg6,
				arg7,
				arg8,
				arg9,
				arg10,
				arg11,
				arg12,
				arg13
			}, shell: false, stdout: subprocess.PIPE, stderr: subprocess.PIPE, cwd: os.path.dirname(os.path.dirname(os.path.realpath(@__file__))));
			*/

			// todo: temporarily return null
			//return proc;
			return null;
		}
	}
}
