using System;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Sensors;
using Windows.Devices.Gpio;

namespace RaspiTest3.Processes {
	internal class TemperatureSensor {
		internal string Output { get; set; }

		internal async Task InitializeAsync() {
			var gpioController = GpioController.GetDefault();
			var adcManager = new AdcProviderManager();

			adcManager.Providers.Add(
				new ADC0832() {
					ChipSelectPin = gpioController.OpenPin(18),
					ClockPin = gpioController.OpenPin(23),
					DataPin = gpioController.OpenPin(24),
				}
			);

			var adcControllers = await adcManager.GetControllersAsync();
			
			var sensor = new AnalogSensor() {
				AdcChannel = adcControllers[0].OpenChannel(0),
				ReportInterval = 250,
			};

			sensor.ReadingChanged += Sensor_ReadingChanged;
		}

		void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			var tempC = Math.Round(((255 - args.Reading.Value) - 121) * 0.21875, 1) + 21.8;

			Output = tempC.ToString();
		}
	}
}