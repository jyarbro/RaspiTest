using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Windows.Devices.Adc.Provider;

namespace TemperatureSensor {
	internal class ThermistorSensorWatcher {
		ThermistorSensor Sensor { get; } = new ThermistorSensor();

		public async Task StartAsync() {
			var adcManager = new AdcProviderManager();

			Sensor.AdcControllerProvider = new MCP3008();

			adcManager.Providers.Add(
				(IAdcProvider)Sensor.AdcControllerProvider
			);

			var adcControllers = await adcManager.GetControllersAsync();

			Sensor.AdcChannel = adcControllers[0].OpenChannel(0);
			Sensor.Initialize();

			var timeout = DateTime.Now.AddMinutes(5);

			while (DateTime.Now < timeout)
				Task.Delay(1000).Wait();

			Sensor.Dispose();
		}
	}
}
