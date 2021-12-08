namespace ExternalPortal.Helpers
{
    public static class FileSizeHelper
    {
        public static string GetFileSizeAsString(double fileLength)
        {
            string[] abbreviations = { "B", "KB", "MB", "GB", "TB" };
            int position = 0;

            while (fileLength >= 1024 && position < abbreviations.Length - 1)
            {
                position++;
                fileLength /= 1024;
            }

            var result = string.Format("{0:0.##}{1}", fileLength, abbreviations[position]);

            return result;
        }
    }
}
