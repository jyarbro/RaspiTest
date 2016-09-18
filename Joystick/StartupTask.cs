using System;
using Windows.ApplicationModel.Background;

namespace Joystick {
	public sealed class StartupTask : IBackgroundTask {
		public async void Run(IBackgroundTaskInstance taskInstance) {
			var deferral = taskInstance.GetDeferral();

			var joystick = new Joystick();
			await joystick.InitializeAsync();

			var timer = DateTime.Now;
			while (timer < DateTime.Now.AddMinutes(5)) { }

			deferral.Complete();
		}
	}
}
