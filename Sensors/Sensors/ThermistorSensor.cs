using System;
using System.Threading.Tasks;
using Windows.Devices.Adc.Provider;
using Microsoft.IoT.DeviceCore.Adc;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.Devices.Adc;
using Microsoft.IoT.Devices.Sensors;
using UnitsNet;

namespace Sensors {
	internal class ThermistorSensor : IDisposable {
		const int ADC_RANGE = 1023;
		const int THERMISTOR_BETA = 3950;
		const int PULLUP_RESISTANCE = 10000;
		const double VREF = 3.3;
		const double ABSOLUTE_ZERO = 273.15;

		IAdcControllerProvider Adc { get; set; }

		AnalogSensor Sensor { get; } = new AnalogSensor();

		int CurrentReading { get; set; }

		public async Task InitializeAsync() {
			var adcManager = new AdcProviderManager();

			var adc = new MCP3008();
			Adc = adc;
			adcManager.Providers.Add(adc);

			var adcControllers = await adcManager.GetControllersAsync();
			var adcController = adcControllers[0];

			Sensor.AdcChannel = adcController.OpenChannel(0);
			Sensor.ReadingChanged += Sensor_ReadingChanged;
		}

		public Temperature GetTemperature() {
			// Inspired by https://www.sunfounder.com/learn/Sensor-Kit-v1-0-for-Raspberry-Pi/lesson-10-analog-temperature-sensor-sensor-kit-v1-0-for-pi.html

			var volts = VREF * CurrentReading / ADC_RANGE;
			var ohms = PULLUP_RESISTANCE * volts / (VREF - volts);
			var lnOhms = Math.Log(ohms / PULLUP_RESISTANCE);

			var celsius = 1 / ((lnOhms / THERMISTOR_BETA) + (1 / (ABSOLUTE_ZERO + (ADC_RANGE / 10))));
			celsius = celsius - ABSOLUTE_ZERO;

			return Temperature.FromDegreesCelsius(celsius);
		}

		public void Dispose() {
			Sensor.Dispose();
		}

		void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			CurrentReading = Adc.ReadValue(0);
		}
	}
}