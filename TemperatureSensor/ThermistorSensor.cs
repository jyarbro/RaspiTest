using System;
using Windows.Devices.Adc;
using Windows.Devices.Adc.Provider;
using Microsoft.IoT.DeviceCore;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.DeviceHelpers;
using Microsoft.IoT.Devices.Sensors;
using UnitsNet;
using System.Diagnostics;

namespace TemperatureSensor {
	public sealed class ThermistorSensor : IScheduledDevice {
		const int THERMISTOR_BETA = 3950;
		const int PULLUP_RESISTANCE = 10000;
		const double VREF = 3.3;
		const double ABSOLUTE_ZERO = 273.15;

		public uint ReportInterval {
			get {
				return Sensor.ReportInterval;
			}

			set {
				Sensor.ReportInterval = value;
			}
		}

		public AdcChannel AdcChannel {
			get {
				return Sensor.AdcChannel;
			}
			set {
				Sensor.AdcChannel = value;
			}
		}

		public IAdcControllerProvider AdcControllerProvider { get; set; }

		AnalogSensor Sensor { get; } = new AnalogSensor();

		int AdcRange { get; set; }
		bool Initialized { get; set; }

		public void Initialize() {
			if (Sensor.AdcChannel == null)
				throw new MissingIoException(nameof(AdcChannel));

			AdcRange = AdcChannel.Controller.MaxValue - AdcChannel.Controller.MinValue;

			Sensor.ReadingChanged += Sensor_ReadingChanged;

			Initialized = true;
		}

		public void Dispose() {
			Sensor.Dispose();
		}

		void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			if (Sensor.AdcChannel == null)
				throw new MissingIoException(nameof(AdcChannel));

			if (!Initialized)
				throw new Exception("Sensor not initialized.");

			var currentReading = AdcControllerProvider.ReadValue(0);

			// Inspired by https://www.sunfounder.com/learn/Sensor-Kit-v1-0-for-Raspberry-Pi/lesson-10-analog-temperature-sensor-sensor-kit-v1-0-for-pi.html

			var volts = VREF * currentReading / AdcRange;
			var ohms = PULLUP_RESISTANCE * volts / (VREF - volts);
			var lnOhms = Math.Log(ohms / PULLUP_RESISTANCE);

			var celsius = 1 / ((lnOhms / THERMISTOR_BETA) + (1 / (ABSOLUTE_ZERO + (AdcRange / 10))));
			celsius = celsius - ABSOLUTE_ZERO;

			var t = Temperature.FromDegreesCelsius(celsius);

			Debug.WriteLine($"F: {t.DegreesFahrenheit}  C: {t.DegreesCelsius}");
		}
	}
}