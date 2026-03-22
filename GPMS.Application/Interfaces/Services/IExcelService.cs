using GPMS.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface IExcelService
{
    Task<byte[]> GenerateProjectImportTemplateAsync();
    Task<IEnumerable<ProjectImportPreviewDto>> PreviewProjectImportAsync(IFormFile file);
}
