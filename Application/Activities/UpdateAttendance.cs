using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class UpdateAttendance
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor; //Bu şu an istek atan kullanıcıyı çekmek için var Infrastructure katmanında kodladık bunu zaten.
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities
                    .Include(a => a.Attendees).ThenInclude(u => u.AppUser)
                    .SingleOrDefaultAsync(x => x.Id == request.Id);

                if(activity == null) return null; //Eğer aktivite yoksa null döner bu da yazdığımız validasyonlar sayesinde 404 not found hatası döndürecektir.

                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername()); //Users tablosu identityuser özelliği sayesinde gelen tablolardan biridir. AspNetUsers diye geçiyor tablo adında. _userAccessor ise şu anki kullanıcının adını almamızı sağlar yani şu an aktif olan kullanıcıyı. Infrastructure katmanında kodlamıştık bunu zaten.

                if(user == null) return null;

                var hostUsername = activity.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser?.UserName; // Burada aktiviteyi oluşturan host kullanıcının usernamesini aldık.

                var attendance = activity.Attendees.FirstOrDefault(x => x.AppUser.UserName == user.UserName); //Şu anki istek atan kullanıcıyı mevcut aktivitenin katılımcısı mı diye kontrol ediyoruz.

                if(attendance != null && hostUsername == user.UserName) //İsteği atan kişi host ise aktiviteyi iptal ediyoruz(pasif hale getiriyoruz.) tekrar aynı istek gelirse aktiviteyi yeniden aktif hale getiriyoruz.
                    activity.IsCancelled = !activity.IsCancelled;

                if(attendance != null && hostUsername != user.UserName) //İsteği atan kişi normal kullanıcı ise bu aktiviteden kendini kaldırmak istiyordur o yüzden kullanıcıyı aktivitenin katılımcı listesinden kaldırıyoruz.
                    activity.Attendees.Remove(attendance);

                if(attendance == null) //Katılımcı yok ise aktiviteye katılmak isteyen biridir o zaman kullanıcımızı aktiviteye ekliyoruz.
                {
                    attendance = new ActivityAttendee
                    {
                        AppUser = user,
                        Activity = activity,
                        IsHost = false
                    };

                    activity.Attendees.Add(attendance);
                }

                var result = await _context.SaveChangesAsync() > 0;

                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem updating attendance");
            }
        }
    }
}