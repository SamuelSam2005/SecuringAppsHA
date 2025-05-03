namespace SecureDocumentExchange.Web.Helpers
{
    public static class FileValidator
    {
        public static bool IsValidDocx(IFormFile file)
        {
            if (file == null || file.Length < 4)
                return false;

            try
            {
                using var stream = file.OpenReadStream();
                byte[] header = new byte[4];
                stream.Read(header, 0, 4);

                // ZIP signature: 50 4B 03 04 (for .docx files)
                return header[0] == 0x50 &&
                       header[1] == 0x4B &&
                       header[2] == 0x03 &&
                       header[3] == 0x04;
            }
            catch
            {
                return false;
            }
        }
    }
}
