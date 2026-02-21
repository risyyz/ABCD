namespace ABCD.Server.Models {
    public class FileUploadSettings {
        public const string SectionName = "FileUpload";
        public string RootPath { get; set; } = "uploads";
        public string RequestPath { get; set; } = "/uploads";
    }

    public class ImageUploadResponse {
        public string ImageUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}