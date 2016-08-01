using RaspiTest.Processes;
using Windows.ApplicationModel.Background;

namespace RaspiTest {
	public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var initialized = await new HttpServer().InitializeAsync();
			while (initialized) { }
		}
	}
}