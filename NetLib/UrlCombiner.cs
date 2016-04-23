namespace NetLib
{
    using System;
    using System.Linq;

    public static class UrlCombiner
    {
        /// <summary>
        ///     Each item provided is treated as a separate element in the path.
        ///     Existing slashes are changed to forward slashes and double slashes not in the protocol are reduced to single.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static Uri CombineAsSeparateElements(params string[] parts)
        {
            var trimmed =
                parts.Select(
                    p =>
                        p.EndsWith("://") || p.EndsWith(@":\\")
                            ? p.TrimStart('/').TrimStart('\\')
                            : p.Trim('/').Trim('\\'));

            var joined = string.Join("/", trimmed);
            var onlyForwardSlashes = joined.Replace('\\', '/');

            var scan = onlyForwardSlashes;
            var start = 0;
            int index;
            while ((index = scan.IndexOf("//", start, StringComparison.Ordinal)) > -1)
            {
                // Reduces double slashes with single slashes, unless they are prefixed with a colon.
                // That is so it does not the protocol, for example  http://
                if (index > 1 && scan[index - 1] != ':')
                {
                    scan = scan.Remove(index, 1);
                    start = index;
                }
                else
                {
                    //Valid double slash, skip over it.
                    start = index + 1;
                }
            }
            return new Uri(scan);
        }
    }
}