namespace Quickbird.Util
{
    using Windows.UI.Notifications;

    public static class ToastService
    {
        /// <summary>Toast used for sharing debug messages. Only use from the Logging Service!</summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void Debug(string title, string text)
        {
            if (SettingsService.Instance.DebugToastsEnabled == false)
                return;
            //System.Diagnostics.Debug.WriteLine($"{title} - {text}");
            FireToast(title, text);
        }

        public static void NotifyUserOfError(string text) { FireToast("Error", text); }

        public static void NotifyUserOfInformation(string text) { FireToast("Info", text); }


        private static void FireToast(string title, string text)
        {
            //No toasts if the setting is toggled off
            if (SettingsService.Instance.ToastsEnabled == false)
                return; 

            // Get a toast XML template
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            // vFill in the text elements
            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(text));

            // Create the toast and attach event listeners
            var toast = new ToastNotification(toastXml);

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            try
            {//Turns out this crap throws an exception if the text is too long
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (System.Exception ex)
            {
                LoggingService.LogInfo("There was an error in creating a toast notification, probably because text is too large", Windows.Foundation.Diagnostics.LoggingLevel.Verbose);
            }
        }
    }
}
