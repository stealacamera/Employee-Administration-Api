using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EmployeeAdministration.Infrastructure.Services.Utils;

internal class ImagesStorageService : IImagesStorageService
{
    private readonly Cloudinary _cloudinary;

    public ImagesStorageService(IOptions<CloudinaryOptions> options)
    {
        var cloudinaryOptions = options.Value;
        Account account = new(cloudinaryOptions.Name, cloudinaryOptions.ApiKey, cloudinaryOptions.ApiSecret);
        
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
            throw new ArgumentException("Empty file");

        var uploadParams = new ImageUploadParams { UseFilename = false };
        ImageUploadResult result;

        using (var fileStream = file.OpenReadStream())
        {
            uploadParams.File = new FileDescription(file.FileName, fileStream);
            result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        }

        return result.PublicId;
    }

    public async Task<string> GetFileUrlAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var file = await _cloudinary.GetResourceAsync(fileId, cancellationToken);
        return file.SecureUrl;
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
        => await _cloudinary.DeleteResourcesAsync(fileId);
}
