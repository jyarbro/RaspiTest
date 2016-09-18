using System;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;

namespace Joystick {
	public sealed class StartupTask : IBackgroundTask
    {
		internal static BackgroundTaskDeferral Deferral = null;

		public async void Run(IBackgroundTaskInstance taskInstance)
        {
			Deferral = taskInstance.GetDeferral();

			var joystick = new Joystick();
			await joystick.InitializeAsync();

			while(true) {
				joystick.UpdateLeds();
				await Task.Delay(TimeSpan.FromMilliseconds(100));
			}

			Deferral.Complete();
		}
	}
}
