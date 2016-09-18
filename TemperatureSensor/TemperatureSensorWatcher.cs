using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Sensors;

namespace TemperatureSensor {
	internal class TemperatureSensorWatcher {
		const uint REPORT_INTERVAL = 250;

		const int PIN_CHIP_SELECT = 18;
		const int PIN_CLOCK = 23;
		const int PIN_DATA = 24;

		public string Output { get; set; }

		GpioController GpioController { get; set; }

		public async Task InitializeAsync() {
			GpioController = GpioController.GetDefault();

			var adcManager = new AdcProviderManager();

			adcManager.Providers.Add(
				new MCP3008()
			);

			//adcManager.Providers.Add(
			//	new ADC0832() {
			//		ChipSelectPin = GpioController.OpenPin(PIN_CHIP_SELECT),
			//		ClockPin = GpioController.OpenPin(PIN_CLOCK),
			//		DataPin = GpioController.OpenPin(PIN_DATA),
			//	}
			//);

			var adcControllers = await adcManager.GetControllersAsync();

			var sensor = new AnalogSensor() {
				AdcChannel = adcControllers[0].OpenChannel(0),
				ReportInterval = REPORT_INTERVAL,
			};

			sensor.ReadingChanged += Sensor_ReadingChanged;
		}

		public void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			var tempC = Math.Round(((255 - args.Reading.Value) - 121) * 0.21875, 1) + 21.8;

			Output = $@"{DateTime.Now.ToString("hh:mm")}: {tempC} C [{args.Reading.Value}]";
		}
	}
}