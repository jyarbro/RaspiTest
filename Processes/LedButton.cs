using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RaspiTest3.Processes {
	internal class LedButton {
		const int ID_INPUT_PIN = 25;
		const int ID_OUTPUT_PIN = 21;

		GpioController GpioController;
		GpioPin OutputPin;
		GpioPin InputPin;

		bool Initialized;

		internal void Initialize() {
			GpioController = GpioController.GetDefault();

			if (GpioController == null)
				throw new Exception();

			OutputPin = GpioController.OpenPin(ID_OUTPUT_PIN);

			if (OutputPin == null)
				throw new Exception();

			InputPin = GpioController.OpenPin(ID_INPUT_PIN);

			if (InputPin == null)
				throw new Exception();

			if (OutputPin.GetDriveMode() != GpioPinDriveMode.Output)
				OutputPin.SetDriveMode(GpioPinDriveMode.Output);

			if (InputPin.GetDriveMode() != GpioPinDriveMode.Input)
				InputPin.SetDriveMode(GpioPinDriveMode.Input);

			Initialized = true;
		}

		internal async Task MonitorAsync() {
			if (!Initialized)
				throw new Exception("Load first");

			var counter = 0;
			var newInput = true;

			while (true) {
				if (InputPin.Read() == GpioPinValue.High && newInput) {
					counter++;
					OutputPin.Write(GpioPinValue.High);
					newInput = false;
				}

				if (InputPin.Read() == GpioPinValue.Low) {
					OutputPin.Write(GpioPinValue.Low);
					newInput = true;
				}

				if (counter > 5)
					break;

				await Task.Delay(TimeSpan.FromMilliseconds(10));
			}

			OutputPin.Write(GpioPinValue.Low);
		}
	}
}
