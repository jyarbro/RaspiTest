using System;
using System.Threading.Tasks;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using UnitsNet;

namespace Sensors {
	internal class LowLevelThermistorSensor : IDisposable {
		SpiDevice Sensor { get; set; }

		public async Task InitializeAsync() {
			var spiSettings = new SpiConnectionSettings(0);
			spiSettings.ClockFrequency = 3600000;
			spiSettings.Mode = SpiMode.Mode0;

			var spiQuery = SpiDevice.GetDeviceSelector("SPI0");

			var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);

			if (deviceInfo != null && deviceInfo.Count > 0)
				Sensor = await SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings);
		}

		public Temperature GetTemperature() {
			//From data sheet -- 1 byte selector for channel 0 on the ADC
			//First Byte sends the Start bit for SPI
			//Second Byte is the Configuration Bit
			//1 - single ended 
			//0 - d2
			//0 - d1
			//0 - d0
			//             S321XXXX <-- single-ended channel selection configure bits
			// Channel 0 = 10000000 = 0x80 OR (8+channel) << 4
			var transmitBuffer = new byte[3] { 1, 0x80, 0x00 };
			var receiveBuffer = new byte[3];

			Sensor.TransferFullDuplex(transmitBuffer, receiveBuffer);
			//first byte returned is 0 (00000000), 
			//second byte returned we are only interested in the last 2 bits 00000011 (mask of &3) 
			//then shift result 8 bits to make room for the data from the 3rd byte (makes 10 bits total)
			//third byte, need all bits, simply add it to the above result 
			var result = ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
			
			//LM35 == 10mV/1degC ... 3.3V = 3300.0 mV, 10 bit chip # steps is 2 exp 10 == 1024
			var mv = result * (3300.0 / 1024.0);

			var celsius = mv / 10.0;

			return Temperature.FromDegreesCelsius(celsius);
		}

		public void Dispose() {
			Sensor.Dispose();
		}
	}
}