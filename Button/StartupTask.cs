using Windows.ApplicationModel.Background;

namespace Button {
	public sealed class StartupTask : IBackgroundTask
    {
		public void Run(IBackgroundTaskInstance taskInstance)
        {
			var deferral = taskInstance.GetDeferral();

			var buttonWatcher = new ButtonWatcher();
			buttonWatcher.Initialize();

			while (buttonWatcher.Counter < 10) { }

			deferral.Complete();
		}
    }
}
