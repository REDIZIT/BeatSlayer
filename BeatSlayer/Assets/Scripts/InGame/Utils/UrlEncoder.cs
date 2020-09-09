namespace InGame.Utils
{
    public static class UrlEncoder 
    {
        public static string Encode(string value)
        {
            return value.Replace("&", "%26");
        }
    }
}
