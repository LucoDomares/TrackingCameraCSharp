using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;
using TrackingCamera.CameraClasses;

namespace TrackingCamera
{
	public partial class MainForm : Form
	{
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		public MainForm()
		{
			InitializeComponent();
			Vstarcam_C7823WIP camera = new Vstarcam_C7823WIP("192.168.8.18","admin","Gateway2","Driveway Cam","10080","");
			/*
			//camera_factory.CameraFactory factory = new camera_factory.CameraFactory();
			//Onvif.Core.Client.Media.MediaClient media = new Onvif.Core.Client.Media.MediaClient("")
			System.Threading.Tasks.Task<Onvif.Core.Client.Media.MediaClient> mediaTask = Onvif.Core.Client.OnvifClientFactory.CreateMediaClientAsync("192.168.8.18:10080", "admin", "Gateway2");
			mediaTask.Start();
			mediaTask.Wait();
			Onvif.Core.Client.Media.MediaClient media = mediaTask.Result;

			//Onvif.Core.Client.Media.MediaClient x = await CreateMediaClientAsync("192.168.8.18:10080", "admin", "Gateway2");
			//System.Threading.Tasks.Task<Onvif.Core.Client.Media.MediaClient> mediaTask
			//media.GetProfilesAsync

			BaseOnvifPtzCamera camera = new Vstarcam_C7823WIP("192.168.8.18", "admin", "Gateway2", "Driveway Cam", "10080", "C:\\Users\\Lamby\\Documents\\Visual Studio 2017\\Projects\\TrackingCamera\\TrackingCamera\\wsdl");

			camera.OpenVideo();

			camera.CloseVideo();
			*/
		}
	}
}
