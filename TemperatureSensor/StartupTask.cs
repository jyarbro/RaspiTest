using Windows.ApplicationModel.Background;

namespace TemperatureSensor {
	public sealed class StartupTask : IBackgroundTask {
		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			var sensorWatcher = new ThermistorSensorWatcher();
			await sensorWatcher.StartAsync();
						
			deferral.Complete();
		}
	}
}