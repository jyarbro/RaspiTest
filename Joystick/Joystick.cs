using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Input;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Input;
using Windows.Devices.Gpio;

namespace Joystick {
	internal class Joystick {
		const uint REPORT_INTERVAL = 250;

		const int PIN_LED_LEFT = 17;
		const int PIN_LED_MID = 18;
		const int PIN_LED_RIGHT = 19;
		const int PIN_BUTTON = 20;

		GpioController GpioController;

		GpioPin LedLeft;
		GpioPin LedMid;
		GpioPin LedRight;

		double X;
		double Y;

		internal async Task InitializeAsync() {
			var adcManager = new AdcProviderManager();

			adcManager.Providers.Add(
				new MCP3008 {
					ChipSelectLine = 0,
					ControllerName = "SPI0"
				}
			);

			var adcControllers = await adcManager.GetControllersAsync();

			var thumbstick = new SS944 {
				XChannel = adcControllers[0].OpenChannel(0),
				YChannel = adcControllers[0].OpenChannel(1),
				ButtonPin = GpioController.OpenPin(PIN_BUTTON),
				ReportInterval = REPORT_INTERVAL,
			};

			thumbstick.ReadingChanged += Thumbstick_ReadingChanged;

			InitializeLeds();
		}

		private void Thumbstick_ReadingChanged(IThumbstick sender, ThumbstickReadingChangedEventArgs args) {
			X = args.Reading.XAxis;
		}

		internal void InitializeLeds() {
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

		internal void UpdateLeds() {
			// var currentValue = (Channel0 + Channel1) / 2;

			Debug.WriteLine($"X {X}");

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