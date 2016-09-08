using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using RaspiTest4.Sensors;

namespace RaspiTest4.Processes {
	public sealed class HttpServer {
		const uint BufferSize = 8192;
		const int Port = 80;

		StreamSocketListener StreamSocketListener;
		AnalogSensorBase SpiActor;

		internal async Task<bool> InitializeAsync(AnalogSensorBase analogSensor) {
			SpiActor = analogSensor;

			StreamSocketListener = new StreamSocketListener();
			await StreamSocketListener.BindServiceNameAsync(Port.ToString()).AsTask();

			StreamSocketListener.ConnectionReceived += async (sender, args) => {
				await ReceiveAsync(sender, args);
				var responseBodyStream = BuildResponse();
				await RespondAsync(args.Socket, responseBodyStream);
			};

			await SpiActor.InitializeAsync();

			return true;
		}

		async Task ReceiveAsync(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args) {
			var request = new StringBuilder();

			using (var input = args.Socket.InputStream) {
				var data = new byte[BufferSize];
				var buffer = data.AsBuffer();
				var dataRead = BufferSize;

				while (dataRead == BufferSize) {
					await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
					request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
					dataRead = buffer.Length;
				}
			}
		}

		MemoryStream BuildResponse() {
			var responseBody = $"<html><body>Ouput: {SpiActor.Output}</body></html>";

			return new MemoryStream(Encoding.UTF8.GetBytes(responseBody));
		}

		async Task RespondAsync(StreamSocket socket, MemoryStream responseBodyStream) {
			using (IOutputStream output = socket.OutputStream) {
				using (Stream response = output.AsStreamForWrite()) {
					var header = "HTTP/1.1 200 OK\r\n" +
						$"Content-Length: {responseBodyStream.Length}\r\n" +
						"Connection: close\r\n\r\n";

					var headerArray = Encoding.UTF8.GetBytes(header);

					await response.WriteAsync(headerArray, 0, headerArray.Length);
					await responseBodyStream.CopyToAsync(response);
					await response.FlushAsync();
				}
			}
		}
	}
}