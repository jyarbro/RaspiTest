using System;
using Windows.Devices.Adc;
using Windows.Foundation;
using Microsoft.IoT.DeviceCore;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.DeviceHelpers;
using Microsoft.IoT.Devices.Sensors;
using UnitsNet;

namespace TemperatureSensor {
	public sealed class ThermistorSensor : ITemperatureSensor, IScheduledDevice {
		const int THERMISTOR_BETA = 3950;
		const int PULLUP_RESISTANCE = 10000;
		const double VREF = 3.3;
		const double ABSOLUTE_ZERO = 273.15;

		public event TypedEventHandler<ITemperatureSensor, ITemperatureReading> ReadingChanged {
			add {
				return _ReadingChanged.Add(value);
			}
			remove {
				_ReadingChanged.Remove(value);
			}
		}
		ObservableEvent<ITemperatureSensor, ITemperatureReading> _ReadingChanged;

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

		AnalogSensor Sensor { get; } = new AnalogSensor();

		TemperatureReading CurrentReading { get; set; } = new TemperatureReading(Temperature.Zero);

		bool Initialized { get; set; }

		public ThermistorSensor() {
			_ReadingChanged = new ObservableEvent<ITemperatureSensor, ITemperatureReading>(firstAdded: OnFirstAdded, lastRemoved: OnLastRemoved);
		}

		public ITemperatureReading GetCurrentReading() {
			if (Sensor.AdcChannel == null)
				throw new MissingIoException(nameof(AdcChannel));

			Update(null);

			return CurrentReading;
		}

		public void Dispose() {
			Sensor.Dispose();
		}

		public void Initialize() {
			if (Sensor.AdcChannel == null)
				throw new MissingIoException(nameof(AdcChannel));

			Initialized = true;
		}

		void Update(AnalogSensorReading sensorReading) {
			if (!Initialized)
				throw new Exception("Sensor not initialized.");

			// Inspired by https://www.sunfounder.com/learn/sensor-kit-v2-0-for-raspberry-pi-b-plus/lesson-18-temperature-sensor-sensor-kit-v2-0-for-b-plus.html

			var volts = VREF * sensorReading.Value / 255;
			var ohms = PULLUP_RESISTANCE * volts / (VREF - volts);
			var lnOhms = Math.Log(ohms / PULLUP_RESISTANCE);

			var celsius = 1 / ((lnOhms / THERMISTOR_BETA) + (1 / (ABSOLUTE_ZERO + 25)));
			celsius = celsius - ABSOLUTE_ZERO;

			var temperature = Temperature.FromDegreesCelsius(celsius);
			var temperatureReading = new TemperatureReading(temperature);

			// Update current value
			lock (CurrentReading) {
				CurrentReading = temperatureReading;
			}

			// Notify
			_ReadingChanged.Raise(this, CurrentReading);
		}

		void OnFirstAdded() {
			Sensor.ReadingChanged += Sensor_ReadingChanged;
		}

		void OnLastRemoved() {
			Sensor.ReadingChanged -= Sensor_ReadingChanged;
		}

		void Sensor_ReadingChanged(IAnalogSensor sender, AnalogSensorReadingChangedEventArgs args) {
			Update(args.Reading);
		}
	}
}