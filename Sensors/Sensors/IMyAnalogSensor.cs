using System;
using Windows.Devices.Adc;
using Windows.Devices.Adc.Provider;

namespace Sensors {
	public interface IMyAnalogSensor : IDisposable {
		IAdcControllerProvider AdcControllerProvider { get; set; }
		AdcChannel AdcChannel { get; set; }

		void Initialize();
	}
}
