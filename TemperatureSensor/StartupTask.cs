using Windows.ApplicationModel.Background;
using Services;

namespace TemperatureSensor {
	public sealed class StartupTask : IBackgroundTask
    {
		internal static BackgroundTaskDeferral Deferral = null;

		internal TemperatureSensorWatcher SensorWatcher { get; } = new TemperatureSensorWatcher();
		internal RaspiWebServer WebServer { get; } = new RaspiWebServer();

		public async void Run(IBackgroundTaskInstance taskInstance)
        {
			Deferral = taskInstance.GetDeferral();

			await SensorWatcher.InitializeAsync();

			await WebServer.InitializeAsync();
			WebServer.RequestHandler += WebServer_RequestHandler;

			Deferral.Complete();
		}

		void WebServer_RequestHandler(object sender, string request) {
			WebServer.ResponseBuffer.Add(SensorWatcher.Output);
		}
	}
}