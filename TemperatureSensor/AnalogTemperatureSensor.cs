using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Adc;
using Windows.Foundation;
using Microsoft.IoT.DeviceCore;
using Microsoft.IoT.DeviceCore.Sensors;
using Microsoft.IoT.DeviceHelpers;
using Microsoft.IoT.Devices.Sensors;
using UnitsNet;

namespace TemperatureSensor {
	public sealed class AnalogTemperatureSensor : ITemperatureSensor, IScheduledDevice {
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

		[DefaultValue(400d)]
		public double ZeroDegreeOffset { get; set; } = 400;

		[DefaultValue(3300)]
		public int ReferenceMilliVolts { get; set; } = 3300;

		[DefaultValue(20d)]
		public double MillivoltsPerDegree { get; set; } = 20;

		[DefaultValue(0d)]
		public double CalibrationOffset { get; set; } = 0;

		AnalogSensor Sensor { get; } = new AnalogSensor();

		TemperatureReading CurrentReading { get; set; } = new TemperatureReading(Temperature.Zero);

		public AnalogTemperatureSensor() {
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

		void Update(AnalogSensorReading reading) {
			var averageRatio = 0D;
			var totalReads = 5;
			
			if (reading != null) {
				averageRatio = reading.Ratio;
				totalReads = 6;
			}

			// Calculate average
			for (int i = 0; i < totalReads; i++) {
				averageRatio += Sensor.GetCurrentReading().Ratio;
				Task.Delay(1).Wait();
			}

			var ratio = averageRatio / totalReads;

			// Multiply by reference
			var milliVolts = ratio * ReferenceMilliVolts;

			// Convert to Celsius
			double celsius = ((milliVolts - ZeroDegreeOffset) / MillivoltsPerDegree) + CalibrationOffset;

			// ADC0832
//			var tempC = Math.Round(((255 - reading.Value) - 121) * 0.21875, 1) + 21.8;

			// Update current value
			lock (CurrentReading) {
				CurrentReading = new TemperatureReading(Temperature.FromDegreesCelsius(celsius));
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