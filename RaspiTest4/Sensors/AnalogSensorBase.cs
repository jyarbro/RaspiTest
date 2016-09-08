using System;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Sensors;
using Windows.Devices.Gpio;

namespace RaspiTest4.Sensors {
	internal abstract class AnalogSensorBase {
		internal GpioController GpioController { get; set; }
		internal string Output { get; set; }

		internal virtual int PinChipSelect { get; }
		internal virtual int PinClock { get; }
		internal virtual int PinData { get; }

		internal virtual uint ReportInterval { get; set; } = 250;

		internal virtual async Task InitializeAsync() {
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

		internal abstract void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args);
	}
}