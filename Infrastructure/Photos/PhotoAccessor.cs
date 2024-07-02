using Application.Interfaces;
using Application.Photos;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Photos
{
    public class PhotoAccessor : IPhotoAccessor
    {
        private readonly Cloudinary _cloudinary;
        public PhotoAccessor(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<PhotoUploadResult> AddPhoto(IFormFile file)
        {
            if(file.Length > 0)
            {
                //using kullanıyoruz çünkü bu yöntemle işi biter bitmez hafızadan silsin istiyorum.
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                   File = new FileDescription(file.FileName, stream), //Yüklenecek dosyayı okuyoruz.
                   Transformation = new Transformation().Height(500).Width(500).Crop("fill") //Burada görüntünün cloudinary içerisine yüklenirken kare olmasını ve kullanıcıların kırpmasına izin verme özelliğini ekliyoruz. Bu sayede istemci tarafında css ile uğraşmaya gerek kalmaz.
                };
                
                var uploadResult = await _cloudinary.UploadAsync(uploadParams); //resmi cloudinary içerisine yüklüyoruz.

                if(uploadResult.Error != null)
                {
                    throw new Exception(uploadResult.Error.Message);
                }

                //Fotoğrafı cloudinary bulut sistemine yükledik fakat bunun url bilgisini sanırım veritabanına falan kaydedebilmek için clien tarafında resimleri url ile çekeceğiz sonuçta - o yüzden resim buluta kaydediltikten sonra url bilgileri return ediliyor.
                return new PhotoUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.SecureUrl.ToString()
                };
            }

            return null;
        }

        public async Task<string> DeletePhoto(string publicId)
        {
            //Gelen fotonun id bilgisini alıp buluttan siliyoruz.
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            //Cevap ok ise sonuç değilse null döner.
            return result.Result == "ok" ? result.Result : null;
        }
    }
}