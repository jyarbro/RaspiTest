using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Sensors {
	public sealed class ExampleThermistorSensorStartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var deferral = taskInstance.GetDeferral();

			using (var sensor = new ThermistorSensor()) {
				await sensor.InitializeAsync();

				var timeout = DateTime.Now.AddMinutes(5);

				while (DateTime.Now < timeout) {
					var temperature = sensor.GetTemperature();
					Debug.WriteLine($"F {temperature.DegreesFahrenheit} | C {temperature.DegreesCelsius}");
					Task.Delay(1000).Wait();
				}
			}

			deferral.Complete();
		}
    }
}
