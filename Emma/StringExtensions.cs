using Foundation;

namespace Toggl.Emma
{
    public static class StringExtensions
    {
        public static string Tr (this string key)
        {
            return NSBundle.MainBundle.LocalizedString (key, null, null);
        }
    }
}
