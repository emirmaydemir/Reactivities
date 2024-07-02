using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            //IFormFile arayüzü, ASP.NET Core'da HTTP isteklerinde dosya yüklemelerini yönetmek için kullanılan bir arayüzdür. Bu arayüz, dosya yüklemelerini temsil eder ve dosyanın adını, içeriğini ve diğer meta verilerini erişmek için kullanılır.
            //API den istek atarkende form-data kullanıldığı için key value şeklinde eşleşme oluyor yani apide verdiğimiz key File olduğu için buradaki değişkenin ismi de File olmak zorundadır.
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor; //sistemdeki mevcut kullanıcı erişimi için
                _context = context; //veritabanı erişimi içim
                _photoAccessor = photoAccessor; //fotoğraf erişimi için silme yükleme falan bulut tabanlı sisteme
            }

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                //AddPhoto fonksiyonunu ben yazmıştım bu fonksiyon fotoğrafı bulut tabanlı sisteme yükler. ve sonucunda bulut tabanlı sistemdeki fotonun id ve url bilgilerini döner.
                var photoUploadResult = await _photoAccessor.AddPhoto(request.File);
                //Include(p => p.Photos) ifadesi, kullanıcıyı alırken ilişkili olduğu fotoğrafları da alır. p => p.Photos, Users tablosundaki Photos koleksiyonuna referans verir. Bu, kullanıcıya ait fotoğrafların da getirilmesini sağlar. Bir nesneyide getirmek istediğimizde include methodunu kullanmamız gerekiyor.
                var user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                //AddPhoto fonksiyonu url ve id döndürüyordu onları photo nesnemizin içerisine atıyoruz.
                var photo = new Photo
                {
                    Url = photoUploadResult.Url,
                    Id = photoUploadResult.PublicId
                };

                //Kullanıcının henüz ana fotoğrafı yoksa yeni eklenen foto main foto olarak işaretlenir bu sayede ilk eklenen foto main olarak işaretlenmiş olur amacımız ilk eklenen fotonun main olması.
                if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

                user.Photos.Add(photo);

                var result = await _context.SaveChangesAsync() > 0;

                //Yükleme başarılı ise başarılı mesajı yükleme başarısız ise hata mesajı dönüyor.
                if (result) return Result<Photo>.Success(photo);

                return Result<Photo>.Failure("Problem adding photo");
            }
        }
    }
}