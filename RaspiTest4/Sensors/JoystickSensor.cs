using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Sensors;
using Windows.Devices.Gpio;

namespace RaspiTest4.Sensors {
	internal class JoystickSensor : AnalogSensorBase {
		internal override int PinChipSelect { get; } = 6;
		internal override int PinClock { get; } = 5;
		internal override int PinData { get; } = 4;

		int OutputLeftPin = 17;
		int OutputMidPin = 18;
		int OutputRightPin = 19;

		int Frame = 5;

		GpioPin OutputLeft { get; set; }
		GpioPin OutputMid { get; set; }
		GpioPin OutputRight { get; set; }

		double Total { get; set; }

		double Lowest { get; set; }
		double Highest { get; set; }

		Queue<double> Readings { get; set; } = new Queue<double>();

		internal override async Task InitializeAsync() {
			await base.InitializeAsync();

			OutputLeft = GpioController.OpenPin(OutputLeftPin);
			OutputMid = GpioController.OpenPin(OutputMidPin);
			OutputRight = GpioController.OpenPin(OutputRightPin);

			if (OutputLeft.GetDriveMode() != GpioPinDriveMode.Output)
				OutputLeft.SetDriveMode(GpioPinDriveMode.Output);

			if (OutputMid.GetDriveMode() != GpioPinDriveMode.Output)
				OutputMid.SetDriveMode(GpioPinDriveMode.Output);

			if (OutputRight.GetDriveMode() != GpioPinDriveMode.Output)
				OutputRight.SetDriveMode(GpioPinDriveMode.Output);
		}

		internal override void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			Total += args.Reading.Value;
			Readings.Enqueue(args.Reading.Value);

			if (Readings.Count > Frame)
				Total -= Readings.Dequeue();

			var average = Total / Frame;

			if (average < 127.9) {
				Output = "Left";
				OutputLeft.Write(GpioPinValue.High);
				OutputMid.Write(GpioPinValue.Low);
				OutputRight.Write(GpioPinValue.Low);
			}
			else if (average > 128.1) {
				Output = "Right";
				OutputLeft.Write(GpioPinValue.Low);
				OutputMid.Write(GpioPinValue.Low);
				OutputRight.Write(GpioPinValue.High);
			}
			else {
				Output = "Mid";
				OutputLeft.Write(GpioPinValue.Low);
				OutputMid.Write(GpioPinValue.High);
				OutputRight.Write(GpioPinValue.Low);
			}
		}
	}
}