using Microsoft.AspNetCore.WebUtilities;

namespace floral_fusion.Interfaces
{
    public interface IStreamFileUploadService
    {

        Task<bool> UploadFile(MultipartReader reader, MultipartSection section);

    }
}
