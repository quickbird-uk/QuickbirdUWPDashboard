namespace Quickbird.Util
{
    using Windows.UI.Notifications;

    public static class Toast
    {
        /// <summary>Toast used for debug messages.</summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void Debug(string title, string text)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{title} - {text}");
            FireToast(title, text);
#endif
        }

        public static void NotifyUserOfError(string text) { FireToast("Error", text); }

        public static void NotifyUserOfInformation(string text) { FireToast("Info", text); }


        private static void FireToast(string title, string text)
        {
            // Get a toast XML template
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            // vFill in the text elements
            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(text));

            // Create the toast and attach event listeners
            var toast = new ToastNotification(toastXml);

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
