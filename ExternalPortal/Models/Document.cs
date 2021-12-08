using ExternalPortal.Helpers;

namespace ExternalPortal.Models
{
    public class Document
    {
        public byte[] Bytes { get; set; }
        public string Filename { get; set; }
        public string FileSizeAsString { get; set; }
        public string DocumentId { get; set; }
        public string Reference { get; set; }

        public Document() { }

        public Document(byte[] bytes, string filename)
        {
            Bytes = bytes;
            Filename = filename;
            FileSizeAsString = bytes != null ? FileSizeHelper.GetFileSizeAsString(Bytes.Length) : string.Empty;
        }
    }
}
