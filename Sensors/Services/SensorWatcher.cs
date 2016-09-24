using System;
using System.Threading.Tasks;
using Windows.Devices.Adc.Provider;
using Microsoft.IoT.DeviceCore.Adc;

namespace Sensors {
	internal class SensorWatcher {
		IMyAnalogSensor Sensor { get; set; }
		IAdcControllerProvider ADC { get; set; }

		public SensorWatcher(IMyAnalogSensor sensor, IAdcControllerProvider adc) {
			Sensor = sensor;
			ADC = adc;
		}

		public async Task StartAsync() {
			var adcManager = new AdcProviderManager();

			Sensor.AdcControllerProvider = ADC;

			adcManager.Providers.Add(
				(IAdcProvider) ADC
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
