using System;
using Microsoft.IoT.DeviceCore.Sensors;

namespace RaspiTest4.Sensors {
	internal class TemperatureSensor : AnalogSensorBase {
		internal override int PinChipSelect { get; } = 18;
		internal override int PinClock { get; } = 23;
		internal override int PinData { get; } = 24;

		internal override void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			var tempC = Math.Round(((255 - args.Reading.Value) - 121) * 0.21875, 1) + 21.8;

			Output = tempC.ToString();
		}
	}
}