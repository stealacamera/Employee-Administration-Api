using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EmployeeAdministration.Infrastructure;

internal class ImagesService : IImagesService
{
    private readonly Cloudinary _cloudinary;

    public ImagesService(IOptions<CloudinaryOptions> options)
    {
        var cloudinaryOptions = options.Value;

        var account = new Account
        {
            ApiKey = cloudinaryOptions.ApiKey,
            ApiSecret = cloudinaryOptions.ApiSecret,
            Cloud = cloudinaryOptions.Name
        };

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
            throw new ArgumentException("Empty file");

        var uploadParams = new ImageUploadParams { UseFilename = false };

        using (var fileStream = file.OpenReadStream())
        {
            uploadParams.File = new FileDescription(file.FileName, fileStream);
        }

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
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
