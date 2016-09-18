using System;
using Windows.ApplicationModel.Background;
using Services;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.Devices.Adc;
using Windows.Devices.Adc;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Sensors;
using System.Diagnostics;

namespace TemperatureSensor {
	public sealed class StartupTask : IBackgroundTask {
		public string Output { get; set; }

		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			var webServer = new RaspiWebServer();
			await webServer.InitializeAsync();

			var sensor = new AnalogTemperatureSensor();
			sensor.AdcChannel = await GetAdcChannelAsync();

			sensor.ReadingChanged += Sensor_ReadingChanged;
			webServer.RequestHandler += WebServer_RequestHandler;

			var timeout = DateTime.Now.AddMinutes(5);
			while (DateTime.Now < timeout) { }

			sensor.Dispose();
			
			deferral.Complete();
		}

		void WebServer_RequestHandler(object sender, string request) {
			var webServer = (RaspiWebServer)sender;

			try {
				webServer.ResponseBuffer.Add($"{Output}<br />");
			}
			catch (Exception e) {
				while (e.InnerException != null)
					e = e.InnerException;

				webServer.ResponseBuffer.Add($"{e.Message}<br />");
			}
		}

		void Sensor_ReadingChanged(ITemperatureSensor sender, ITemperatureReading args) {
			var t = args.Temperature;

			if (t == null) {
				Debug.WriteLine("Temperature is null.");
				return;
			}

			Debug.WriteLine($"F: {t.DegreesFahrenheit}  C: {t.DegreesCelsius}");

			Output = $@"{DateTime.Now.ToString("hh:mm")}: {t.DegreesCelsius} C";
		}

		async Task<AdcChannel> GetAdcChannelAsync() {
			var adcManager = new AdcProviderManager();

			adcManager.Providers.Add(
				new MCP3008()
			);

			var adcControllers = await adcManager.GetControllersAsync();
			var adcController = adcControllers[0];

			return adcController.OpenChannel(0);
		}
	}
}