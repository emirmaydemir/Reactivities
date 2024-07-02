using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class SetMain
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor; //sistemdeki mevcut kullanıcı erişimi için
                _context = context; //veritabanı erişimi için
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //Include(p => p.Photos) ifadesi, kullanıcıyı alırken ilişkili olduğu fotoğrafları da alır. p => p.Photos, Users tablosundaki Photos koleksiyonuna referans verir. Bu, kullanıcıya ait fotoğrafların da getirilmesini sağlar. Bir nesneyide getirmek istediğimizde include methodunu kullanmamız gerekiyor.
                //User tablosu derken biz ismini AppUser koyduk fakat identity servisin oluşturmuş olduğu user tablosuna ekleniyor app user içerisindeki değişkenler ek olarak ekleniyor yani hepsi user tablosunda kullanılıyor çünkü biz hazır user tablosunu kullanıyoruz IdentityUser desteği sayesinde.
                //Kısacası bu mevcut kullanıcının bilgilerini fotoğraflar ile birlikte döndürür include kullanmamızın sebebi photos'ta bir liste olduğu için nesnelerin veya listelerin algılanması için user sınıfında tanımlı olması yetmiyor include ile belirtmemiz gerekiyor.
                var user = await _context.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                //Ana fotoğraf yapılmak istenen fotoğraf nesnesini id bilgisinden buluyoruz. Users içindeki photos listesinden.
                var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

                //Fotoğraf bulunamadıysa null döndürüyoruz.
                if (photo == null) return null;

                //Eskiden main olan foto varsa onu bir değişkene atıyoruz mainliğini devre dışı bırakmak için.
                var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

                //Eskiden main olan foto varsa onun mainliğini kaldırıyoruz.
                if (currentMain != null) currentMain.IsMain = false;

                //Yeni main fotomuzu kullanıcının sectiği foto olarak belirledik.
                photo.IsMain = true;
                //Değişiklikleri kaydettik.
                var success = await _context.SaveChangesAsync() > 0;

                if (success) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem setting main photo");
            }
        }
    }
}