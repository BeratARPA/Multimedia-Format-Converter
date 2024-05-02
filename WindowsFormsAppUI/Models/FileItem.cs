namespace WindowsFormsAppUI.Models
{
    public class FileItem
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; }
    }
}
