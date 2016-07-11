namespace Quickbird.Util
{
    public static class Log
    {
        /// <summary>Errors that should never happen but are silently ignored none-the-less.</summary>
        /// <param name="wat">Some descritpion that ay help debug the problem.</param>
        public static void ShouldNeverHappen(string wat) { Toast.Debug("SHOULD NEVER HAPPEN", wat); }
    }
}
