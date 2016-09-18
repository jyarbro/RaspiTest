using System;
using Windows.ApplicationModel.Background;

namespace Joystick {
	public sealed class StartupTask : IBackgroundTask {
		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			var joystick = new Joystick();
			await joystick.InitializeAsync();

			var timeout = DateTime.Now.AddMinutes(5);
			while (DateTime.Now < timeout) { }

			deferral.Complete();
		}
	}
}
