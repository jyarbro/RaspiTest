using Windows.ApplicationModel.Background;
using Microsoft.IoT.Devices.Adc;

namespace Sensors {
	public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var deferral = taskInstance.GetDeferral();

			var sensor = new ThermistorSensor();
			var adc = new MCP3008();

			var sensorWatcher = new SensorWatcher(sensor, adc);

			await sensorWatcher.StartAsync();

			deferral.Complete();
		}
    }
}
