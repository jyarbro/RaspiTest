using Windows.ApplicationModel.Background;
using RaspiTest4.Sensors;
using RaspiTest4.Processes;

namespace RaspiTest4 {
	public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var analogSensor = new JoystickSensor();

			var initialized = await new HttpServer().InitializeAsync(analogSensor);
			while (initialized) { }
		}
    }
}