using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IPhotoAccessor _photoAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor)
            {
                _photoAccessor = photoAccessor; //fotoğraf erişimi için silme yükleme falan bulut tabanlı sisteme
                _userAccessor = userAccessor; //sistemdeki mevcut kullanıcı erişimi için
                _context = context; //veritabanı erişimi için
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //Include(p => p.Photos) ifadesi, kullanıcıyı alırken ilişkili olduğu fotoğrafları da alır. p => p.Photos, Users tablosundaki Photos koleksiyonuna referans verir. Bu, kullanıcıya ait fotoğrafların da getirilmesini sağlar. Bir nesneyide getirmek istediğimizde include methodunu kullanmamız gerekiyor.
                //User tablosu derken biz ismini AppUser koyduk fakat identity servisin oluşturmuş olduğu user tablosuna ekleniyor app user içerisindeki değişkenler ek olarak ekleniyor yani hepsi user tablosunda kullanılıyor çünkü biz hazır user tablosunu kullanıyoruz IdentityUser desteği sayesinde.
                //Kısacası bu mevcut kullanıcının bilgilerini fotoğraflar ile birlikte döndürür include kullanmamızın sebebi photos'ta bir liste olduğu için nesnelerin veya listelerin algılanması için user sınıfında tanımlı olması yetmiyor include ile belirtmemiz gerekiyor.
                var user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
                
                //silinmek isteyen fotoğraf nesnesini id bilgisinden buluyoruz. Users içindeki photos listesinden.
                var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

                //Fotoğraf bulunamadıysa null döndürüyoruz.
                if (photo == null) return null;

                //Kullanıcının ana fotoğrafı ise silmesini engelleyeceğiz.
                if (photo.IsMain) return Result<Unit>.Failure("You cannot delete your main photo");

                //Fotoğrafı cloudinary yani bulut tabanlı sistemden siliyoruz id bilgisi ile. Fotoğrafın kendisini cloudda id bilgisini ise veritabanında tutuyoruz.
                var result = await _photoAccessor.DeletePhoto(photo.Id);

                //Sonuç null dönerse hata var demektir. Bu yüzden hata mesajı veriyoruz.
                if (result == null) return Result<Unit>.Failure("Problem deleting photo from Cloudinary");

                //Clouddan sildik ama Photos listemizden de siliyoruz fotoğrafı.
                user.Photos.Remove(photo);

                //Değişiklikleri kaydediyoruz.
                var success = await _context.SaveChangesAsync() > 0;

                if (success) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem deleting photo from API");
            }
        }
    }
}