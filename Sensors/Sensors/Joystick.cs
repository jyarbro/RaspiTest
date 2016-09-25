using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Input;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Input;
using Windows.Devices.Adc.Provider;
using Windows.Devices.Gpio;

namespace Sensors {
	internal class Joystick : IDisposable {
		const int PIN_LED_LEFT = 17;
		const int PIN_LED_MID = 18;
		const int PIN_LED_RIGHT = 19;
		const int PIN_BUTTON = 20;

		IAdcControllerProvider Adc { get; set; }
		GpioController GpioController { get; set; }

		SS944 Stick { get; } = new SS944();

		GpioPin LedLeft;
		GpioPin LedMid;
		GpioPin LedRight;

		double X;
		double Y;

		public async Task InitializeAsync() {
			var adcManager = new AdcProviderManager();

			var adc = new MCP3008();
			Adc = adc;
			adcManager.Providers.Add(adc);

			var adcControllers = await adcManager.GetControllersAsync();
			var adcController = adcControllers[0];

			Stick.XChannel = adcControllers[0].OpenChannel(0);
			Stick.YChannel = adcControllers[0].OpenChannel(1);
			Stick.ButtonPin = GpioController.OpenPin(PIN_BUTTON);
			Stick.ReadingChanged += Thumbstick_ReadingChanged;

			InitializeLeds();
		}

		public void Dispose() {
			Stick.Dispose();
		}

		void Thumbstick_ReadingChanged(IThumbstick sender, ThumbstickReadingChangedEventArgs args) {
			X = args.Reading.XAxis;
			Y = args.Reading.XAxis;

			UpdateLeds();
		}

		void InitializeLeds() {
			GpioController = GpioController.GetDefault();

			LedLeft = GpioController.OpenPin(PIN_LED_LEFT);
			LedMid = GpioController.OpenPin(PIN_LED_MID);
			LedRight = GpioController.OpenPin(PIN_LED_RIGHT);

			if (LedLeft.GetDriveMode() != GpioPinDriveMode.Output)
				LedLeft.SetDriveMode(GpioPinDriveMode.Output);

			if (LedMid.GetDriveMode() != GpioPinDriveMode.Output)
				LedMid.SetDriveMode(GpioPinDriveMode.Output);

			if (LedRight.GetDriveMode() != GpioPinDriveMode.Output)
				LedRight.SetDriveMode(GpioPinDriveMode.Output);
		}

		void UpdateLeds() {
			Debug.WriteLine($"X {X}, Y {Y}");

			if (X < 64) {
				LedLeft.Write(GpioPinValue.High);
				LedMid.Write(GpioPinValue.Low);
				LedRight.Write(GpioPinValue.Low);
			}
			else if (X > 64) {
				LedLeft.Write(GpioPinValue.Low);
				LedMid.Write(GpioPinValue.Low);
				LedRight.Write(GpioPinValue.High);
			}
			else {
				LedLeft.Write(GpioPinValue.Low);
				LedMid.Write(GpioPinValue.High);
				LedRight.Write(GpioPinValue.Low);
			}
		}
	}
}