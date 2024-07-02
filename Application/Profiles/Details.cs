using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles
{
    public class Details
    {
        //update işlemi olmadığı için query yapıyoruz update create falan olsaydı command olacaktı.
        public class Query : IRequest<Result<Profile>>
        {
            //Profile bilgisine erişmek için username yeterli bize hepsinin usernamesi farklı olmak zorunda zaten.
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Profile>>
        {
            private readonly DataContext _context; //veritabanı erişimi için
            private readonly IMapper _mapper; //ÖNEMLİ AÇIKLAMA: Tablo ile bir DTO sınıfını falan eşleyebilmek için kullanıyoruz. Çünkü Users tablosunda baya bir sütun bulunuyor ve bu sistemi yavaşlatır bunun yerine sadece bize lazım olan Profile içerisindeki (UserName, DisplayName, Bio, Image ve Photos değişkenleri yeterli bizim için).
            private readonly IUserAccessor _userAccessor; //sistemdeki mevcut kullanıcı erişimi için
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<Profile>> Handle(Query request, CancellationToken cancellationToken)
            {
                //ÖNEMLİ AÇIKLAMA: Users Tablosu ile bir DTO sınıfını falan eşleyebilmek için kullanıyoruz. Çünkü Users tablosunda baya bir sütun bulunuyor ve bu sistemi yavaşlatır bunun yerine sadece bize lazım olan Profile içerisindeki (UserName, DisplayName, Bio, Image ve Photos değişkenleri yeterli bizim için).
                var user = await _context.Users
                    .ProjectTo<Profile>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(x => x.UserName == request.Username);

                //Buna gerek yok sanırım çünkü zaten başarı durumunu ele alıyoruz + null HandleResult içerisinde.
                if (user == null) return null;

                return Result<Profile>.Success(user);
            }
        }
    }
}