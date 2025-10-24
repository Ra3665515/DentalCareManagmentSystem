

using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly ClinicDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImageService(ClinicDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<PatientImage> UploadImageAsync(Guid patientId, Stream imageStream, string fileName)
    {
        var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "patients", patientId.ToString());
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
        var filePath = Path.Combine(uploadsDir, newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageStream.CopyToAsync(stream);
        }

        var thumbPath = await CreateThumbnailAsync(filePath, uploadsDir);

        var patientImage = new PatientImage
        {
            PatientId = patientId,
            FileName = newFileName,
            FilePath = Path.Combine("/uploads/patients/", patientId.ToString(), newFileName).Replace('\\', '/'),
            ThumbnailPath = Path.Combine("/uploads/patients/", patientId.ToString(), Path.GetFileName(thumbPath)).Replace('\\', '/'),
            UploadedAt = DateTime.UtcNow
        };

        _context.PatientImages.Add(patientImage);
        await _context.SaveChangesAsync();

        return patientImage;
    }

    public void DeleteImage(Guid imageId)
    {
        var image = _context.PatientImages.Find(imageId);
        if (image != null)
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, image.FilePath.TrimStart('/'));
            var thumbPath = Path.Combine(_webHostEnvironment.WebRootPath, image.ThumbnailPath.TrimStart('/'));

            if (File.Exists(fullPath)) File.Delete(fullPath);
            if (File.Exists(thumbPath)) File.Delete(thumbPath);

            _context.PatientImages.Remove(image);
            _context.SaveChanges();
        }
    }

    public List<PatientImage> GetImagesByPatientId(Guid patientId)
    {
        return _context.PatientImages.Where(i => i.PatientId == patientId).ToList();
    }

    private async Task<string> CreateThumbnailAsync(string imagePath, string outputDir)
    {
        using var image = await Image.LoadAsync(imagePath);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(300, 0),
            Mode = ResizeMode.Max
        }));

        var thumbFileName = "thumb_" + Path.GetFileName(imagePath);
        var thumbPath = Path.Combine(outputDir, thumbFileName);
        await image.SaveAsync(thumbPath);
        return thumbPath;
    }
}

