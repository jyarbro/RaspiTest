using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Services {
	public class RaspiWebServer {
		public event EventHandler<string> RequestHandler;

		public List<string> ResponseBuffer { get; } = new List<string>();

		public int ListenPort { get; set; } = 80;
		public uint BufferSize { get; set; } = 8192;

		StreamSocketListener StreamSocketListener { get; } = new StreamSocketListener();

		public async Task InitializeAsync() {
			await StreamSocketListener.BindServiceNameAsync(ListenPort.ToString()).AsTask();

			StreamSocketListener.ConnectionReceived += async (sender, args) => {
				using (var input = args.Socket.InputStream) {
					await ReceiveAsync(input);
				}

				using (var output = args.Socket.OutputStream) {
					await RespondAsync(output);
				}
			};
		}

		async Task ReceiveAsync(IInputStream input) {
			if (RequestHandler == null)
				return;

			var request = new StringBuilder();

			var data = new byte[BufferSize];
			var buffer = data.AsBuffer();
			var dataRead = BufferSize;

			while (dataRead == BufferSize) {
				await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
				request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
				dataRead = buffer.Length;
			}

			RequestHandler.Invoke(this, request.ToString());
		}

		async Task RespondAsync(IOutputStream output) {
			var responseContent = string.Join("\n", ResponseBuffer);
			var responsePage = $"<html><body>{responseContent}</body></html>";

			var responseBodyStream = new MemoryStream(Encoding.UTF8.GetBytes(responsePage));

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