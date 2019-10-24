using System;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using Mictlanix.DotNet.Onvif.Ptz;
using TrackingCamera.CameraClasses;
using TrackingCamera.Helpers;
using TrackingCamera.CameraManagers;
using System.IO;

namespace TrackingCamera {
	class Program {
		static void Main (string[] args)
		{
			MainAsync().Wait();
		}

		/// <summary>
		/// Main asycronous method 
		/// </summary>
		/// <returns>An Asyncronous <c>Task</c></returns>
		private static async Task MainAsync()
		{
			string cameraConfigFile = ConfigurationManager.AppSettings["CameraSettingsPath"];
			CameraConfigsList cameraConfigs = Globals.ReadFromXmlFile<CameraConfigsList>(cameraConfigFile);

			try
			{
				StaticCameraManagerFactory.CameraManagerFactory factory = new StaticCameraManagerFactory.CameraManagerFactory();
				StaticCameraManagerFactory.RegisterAllCameraManagers(factory);

				BaseCameraManagersList runningManagers = new BaseCameraManagersList();

				try
				{
					int camerasToOpen = (from cameraConfig in cameraConfigs where cameraConfig.IsActive select cameraConfig).Count();
					Globals.Log.Info(string.Format("Starting {0} cameras.", camerasToOpen));

					// start up a camera manager for each camera in the camera config file.
					foreach (CameraConfig cameraConfig in cameraConfigs)
					{
						if (cameraConfig.IsActive)
						{
							// the camera is enabled, start up a Manager for it.
							BaseCameraManager cameraManager = factory.CreateCameraManager(cameraConfig);
							runningManagers.Add(cameraManager);
						}
						else
						{
							// the camera is disabled.
							Globals.Log.Info(string.Format("Skipping camera '{0}' since it's marked inactive.", cameraConfig.CameraName));
						}
					}

					Console.WriteLine();
					Console.WriteLine(String.Format("Tracking Camera Controller Service is controlling {0} cameras.", runningManagers.Count()));
					Console.WriteLine("Enter 'q' to quit.");
					Console.WriteLine();

					// loop until told to stop
					bool isQuitting = false;
					do
					{
						string input = Console.ReadLine().ToLower().Trim();
						switch (input)
						{
							case "q":
							case "quit":
							case "end":
								isQuitting = true;
								break;
						}

					} while (!isQuitting);
				}
				finally
				{
					foreach (BaseCameraManager manager in runningManagers)
					{
						manager.Stop();
					}

					// wait for the camera managers to stop.
					do
					{
						await Task.Delay(200);
					} while ((from manager in runningManagers where manager.IsStopped = false select manager).Count() > 0);

					runningManagers.Clear();
				}
			}
			catch (Exception detail)
			{
				Globals.Log.Error(detail);
			}
			finally
			{
				if (cameraConfigs != null)
				{
					Globals.WriteToXmlFile<CameraConfigsList>(cameraConfigFile, cameraConfigs);
				}
			}
		}
	}
}
