using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Adc;
using Windows.Devices.Gpio;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;

namespace TemperatureSensor {
	public class ThermistorSensorWatcher {
		const int PIN_CHIP_SELECT = 18;
		const int PIN_CLOCK = 23;
		const int PIN_DATA = 24;

		public async Task StartAsync() {
			var sensor = new ThermistorSensor();
			sensor.AdcChannel = await GetAdcChannelAsync();
			sensor.Initialize();

			sensor.ReadingChanged += Sensor_ReadingChanged;

			var timeout = DateTime.Now.AddMinutes(5);

			while (DateTime.Now < timeout)
				Task.Delay(1000).Wait();

			sensor.Dispose();
		}

		void Sensor_ReadingChanged(ITemperatureSensor sender, ITemperatureReading args) {
			var t = args.Temperature;

			if (t == null) {
				Debug.WriteLine("Temperature is null.");
				return;
			}

			Debug.WriteLine($"F: {t.DegreesFahrenheit}  C: {t.DegreesCelsius}");
		}

		async Task<AdcChannel> GetAdcChannelAsync() {
			var adcManager = new AdcProviderManager();
			var gpioController = GpioController.GetDefault();

			//adcManager.Providers.Add(
			//	new MCP3008()
			//);

			adcManager.Providers.Add(
				new ADC0832() {
					ChipSelectPin = gpioController.OpenPin(PIN_CHIP_SELECT),
					ClockPin = gpioController.OpenPin(PIN_CLOCK),
					DataPin = gpioController.OpenPin(PIN_DATA),
				}
			);

			var adcControllers = await adcManager.GetControllersAsync();
			var adcController = adcControllers[0];

			return adcController.OpenChannel(0);
		}
	}
}
