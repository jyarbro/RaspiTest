using Windows.ApplicationModel.Background;

namespace Button {
	public sealed class StartupTask : IBackgroundTask
    {
		internal static BackgroundTaskDeferral Deferral = null;

		public void Run(IBackgroundTaskInstance taskInstance)
        {
			Deferral = taskInstance.GetDeferral();

			var buttonWatcher = new ButtonWatcher();
			buttonWatcher.Initialize();

			while (buttonWatcher.Counter < 10) { }

			Deferral.Complete();
		}
    }
}
