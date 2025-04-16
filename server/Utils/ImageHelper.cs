namespace server.Utils
{
    public static class ImageHelper
    {
        public static string FixBase64String(string base64)
        {
            int mod4 = base64.Length % 4;
            return mod4 > 0 ? base64 + new string('=', 4 - mod4) : base64;
        }

        public static string DetectMimeType(string base64)
        {
            return base64.StartsWith("/9j/") ? "image/jpeg" :
                   base64.StartsWith("iVBORw0KGgo") ? "image/png" :
                   base64.StartsWith("R0lGODlh") ? "image/gif" :
                   base64.StartsWith("UklGR") ? "image/webp" : string.Empty;
        }
    }
}
