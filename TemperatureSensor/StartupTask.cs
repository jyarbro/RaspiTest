using System;
using Windows.ApplicationModel.Background;
using Services;

namespace TemperatureSensor {
	public sealed class StartupTask : IBackgroundTask {
		internal TemperatureSensorWatcher SensorWatcher { get; } = new TemperatureSensorWatcher();
		internal RaspiWebServer WebServer { get; } = new RaspiWebServer();

		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			await SensorWatcher.InitializeAsync();

			await WebServer.InitializeAsync();

			WebServer.RequestHandler += (object sender, string request) => {
				WebServer.ResponseBuffer.Add(SensorWatcher.Output);
			};

			var timeout = DateTime.Now.AddMinutes(5);
			while (DateTime.Now < timeout) { }

			deferral.Complete();
		}
	}
}