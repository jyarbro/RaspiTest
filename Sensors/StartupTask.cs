﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Sensors {
	public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			var deferral = taskInstance.GetDeferral();

			using (var sensor = new Joystick()) {
				await sensor.InitializeAsync();

				var timeout = DateTime.Now.AddMinutes(5);

				while (DateTime.Now < timeout) {
					Task.Delay(1000).Wait();
				}
			}

			deferral.Complete();
		}
    }
}
