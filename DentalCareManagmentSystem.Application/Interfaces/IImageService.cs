
using Microsoft.AspNetCore.Http;
using DentalCareManagmentSystem.Domain.Entities;
using System.IO;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IImageService
{
    Task<PatientImage> UploadImageAsync(Guid patientId, Stream imageStream, string fileName);
    void DeleteImage(Guid imageId);
    List<PatientImage> GetImagesByPatientId(Guid patientId);
}
