using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GPMS.Application.Common.Settings;
using GPMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Services
{
    public class CloudinaryService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderPath)
        {
            var uploadResult = new RawUploadParams();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderPath,
                    PublicId = file.FileName, // Cloudinary appends the extension for raw files if not provided, but we pass the full name here.
                    Overwrite = true
                };
                
                var result = await _cloudinary.UploadAsync(uploadParams);
                
                if (result.Error != null)
                {
                    throw new System.Exception(result.Error.Message);
                }

                return result.SecureUrl.ToString();
            }

            return string.Empty;
        }

        public async Task DeleteFileAsync(string publicId, string resourceType = "raw")
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType == "image" ? ResourceType.Image : 
                               resourceType == "video" ? ResourceType.Video : 
                               ResourceType.Raw
            };
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
