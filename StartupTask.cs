using RaspiTest3.Processes;
using Windows.ApplicationModel.Background;

namespace RaspiTest3 {
	public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var initialized = await new HttpServer().InitializeAsync();
			while (initialized) { }
		}
	}
}