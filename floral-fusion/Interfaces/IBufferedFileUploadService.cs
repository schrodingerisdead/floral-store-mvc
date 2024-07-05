namespace floral_fusion.Interfaces
{
    public interface IBufferedFileUploadService
    {

        Task<string> UploadFile(IFormFile file);

    }
}