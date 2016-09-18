using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Sensors;

namespace TemperatureSensor {
	public class TemperatureSensorWatcher {
		public GpioController GpioController { get; set; }
		public string Output { get; set; }

		public int PinChipSelect { get; } = 18;
		public int PinClock { get; } = 23;
		public int PinData { get; } = 24;

		public uint ReportInterval { get; set; } = 250;

		public async Task InitializeAsync() {
			GpioController = GpioController.GetDefault();

			var adcManager = new AdcProviderManager();

			adcManager.Providers.Add(
				new ADC0832() {
					ChipSelectPin = GpioController.OpenPin(PinChipSelect),
					ClockPin = GpioController.OpenPin(PinClock),
					DataPin = GpioController.OpenPin(PinData),
				}
			);

			var adcControllers = await adcManager.GetControllersAsync();

			var sensor = new AnalogSensor() {
				AdcChannel = adcControllers[0].OpenChannel(0),
				ReportInterval = ReportInterval,
			};

			sensor.ReadingChanged += Sensor_ReadingChanged;
		}

		public void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			var tempC = Math.Round(((255 - args.Reading.Value) - 121) * 0.21875, 1) + 21.8;

			Output = $"{tempC} C";
		}
	}
}