using System;
using Windows.Devices.Gpio;

namespace Button {
	public class ButtonWatcher {
		public int Counter { get; set; }

		const int PIN_INPUT = 25;
		const int PIN_OUTPUT = 21;

		GpioController GpioController;
		GpioPin InputPin;
		GpioPin OutputPin;

		public void Initialize() {
			GpioController = GpioController.GetDefault();

			if (GpioController == null)
				throw new Exception();

			OutputPin = GpioController.OpenPin(PIN_OUTPUT);

			if (OutputPin == null)
				throw new Exception();

			InputPin = GpioController.OpenPin(PIN_INPUT);

			if (InputPin == null)
				throw new Exception();

			if (OutputPin.GetDriveMode() != GpioPinDriveMode.Output)
				OutputPin.SetDriveMode(GpioPinDriveMode.Output);

			if (InputPin.GetDriveMode() != GpioPinDriveMode.Input)
				InputPin.SetDriveMode(GpioPinDriveMode.Input);

			InputPin.ValueChanged += InputPin_ValueChanged;
		}

		void InputPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args) {
			OutputPin.Write(InputPin.Read());
			Counter++;
		}
	}
}
