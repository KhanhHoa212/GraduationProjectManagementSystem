using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads a file to Cloudinary with organization (semesters/groups).
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="folderPath">The folder path (e.g., semesters/SP25/groups/Group1).</param>
        /// <returns>The URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(IFormFile file, string folderPath);
        
        /// <summary>
        /// Deletes a file from Cloudinary.
        /// </summary>
        /// <param name="publicId">The public ID of the file.</param>
        /// <param name="resourceType">The type of resource (default: raw).</param>
        Task DeleteFileAsync(string publicId, string resourceType = "raw");
    }
}
