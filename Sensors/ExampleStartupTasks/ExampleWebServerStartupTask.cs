using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Sensors {
	public sealed class ExampleWebServerStartupTask : IBackgroundTask {
		public string Output { get; set; }

		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			var webServer = new RaspiWebServer();
			await webServer.InitializeAsync();

			webServer.RequestHandler += WebServer_RequestHandler;
			
			// Add an event handler here to update the Output property asynchronously.

			var timeout = DateTime.Now.AddMinutes(5);

			while (DateTime.Now < timeout)
				Task.Delay(1000).Wait();
			
			deferral.Complete();
		}

		void WebServer_RequestHandler(object sender, string request) {
			var webServer = (RaspiWebServer)sender;

			try {
				webServer.ResponseBuffer.Add($"{Output}<br />");
			}
			catch (Exception e) {
				while (e.InnerException != null)
					e = e.InnerException;

				webServer.ResponseBuffer.Add($"{e.Message}<br />");
			}
		}
	}
}