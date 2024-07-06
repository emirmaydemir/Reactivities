using Application.Core;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string DisplayName {get; set;}
            public string Bio {get; set;}
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.DisplayName).NotEmpty(); //Sadece displayname için boş bırakılamaz şartını koyduk bioyu kayıt olurken istemiyoruz o şart değil.
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context; //veritabanı erişimi için
            private readonly IUserAccessor _userAccessor; //sistemdeki mevcut kullanıcı erişimi için.
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername()); //_userAccessor.GetUsername() sistemdeki mevcut kullanıcıyı döndürür bize users tablosundan istek atan kullanıcının nesnesini getiriyoruz.

                user.Bio = request.Bio ?? user.Bio; //Eğer request.Bio null ise, user.Bio kendi mevcut değerini korur.
                user.DisplayName = request.DisplayName ?? user.DisplayName; //Eğer request.DisplayName null ise, user.DisplayName kendi mevcut değerini korur.

                _context.Entry(user).State = EntityState.Modified; //Kullanıcı bir şeyi değiştirmeden tekrar istek atarsa bad request olarak algılanıyor çünkü veritabanında değişiklik olmuyor ama hata da olmuyor biz de birşey değiştirmeden güncelle butonuna basarsa güncellemiş gibi tepki versin diye bu kodu ekledik. Birşey değiştirmese bile 200 ok dönecektir.

                var success = await _context.SaveChangesAsync() > 0; //Değişiklikleri kaydediyoruz ve işlem başarılı ise 1 döner yoksa 0 döner.

                if (success) return Result<Unit>.Success(Unit.Value); //İşlem başarılı ise başarılı mesajı döndüreceğiz

                return Result<Unit>.Failure("Problem updating profile"); //Kaydetme işlemi başarısız ise hata mesajı döndüreceğiz.
            }
        }
    }
}