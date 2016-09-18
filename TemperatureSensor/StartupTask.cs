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

			var timer = DateTime.Now;
			while (timer < DateTime.Now.AddMinutes(5)) { }

			deferral.Complete();
		}
	}
}