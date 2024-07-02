using Application.Photos;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IPhotoAccessor
    {
        //Eğer Iform dosyasının üzerine gelirsek, bu Http isteğiyle gönderilen dosyayı temsil eder. Ve bu, bir dosyayla ilişkilendireceğiniz boyut, dosya boyutu, dosya boyutu gibi tüm özelliklerle birlikte gelir.
        Task<PhotoUploadResult> AddPhoto (IFormFile file);
        Task<string> DeletePhoto(string publicId);
    }
}