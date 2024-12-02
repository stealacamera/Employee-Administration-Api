using Microsoft.AspNetCore.Http;

namespace EmployeeAdministration.Application.Abstractions.Services.Utils;

public interface IImagesStorageService
{
    Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<string> GetFileUrlAsync(string fileId, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);
}
