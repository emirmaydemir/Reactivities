using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>   
        {
            public Activity Activity {get; set;}
        }

        //Activity özelliği için özel bir validator (ActivityValidator) kullanır. Bu, Command sınıfının Activity özelliği doğrulanırken ActivityValidator kurallarının uygulanacağı anlamına gelir.
        //Burada kısaca boş alan bırakılırsa uyarı vermesini sağlıyoruz. ActivityValidator sınıfına gidip daha iyi anlayabilirsin. AbstractValidator bu yazı sayesinde tanıyor doğrulama olduğunu servis tarafı.
        //Örneğin, ActivityValidator : AbstractValidator<Activity> şeklinde tanımlanan bir sınıf, Activity sınıfı için doğrulama kurallarını içerir. FluentValidation bu tür tanımları otomatik olarak bulabilir ve kullanabilir.
        //Örneğin, bir Command modeli bağlandığında, ASP.NET Core, CommandValidator sınıfını kullanarak doğrulama kurallarını uygular. Bu, Command modelinin doğrulanmasını sağlar.
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
            }
        }
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //IUserAccessor içerisinde Infrastructure içerisinde http isteğini gönderen kişinin kullanıcı adını döndüren bir servis var orası sayesinde kullanıcı adını kontrol edebiliyoruz.
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                var attendee = new ActivityAttendee
                {
                    AppUser = user,
                    Activity = request.Activity,
                    IsHost = true
                };

                //Aktiviteyi oluşturan kişiyi katılımcılar listesine ekledik şu anda.
                request.Activity.Attendees.Add(attendee); //Bu satır ile katılımcılar listemize host olan kişiyi eklemiş olduk. Sonradan katılacak insanları ayarlamadık daha

                _context.Activities.Add(request.Activity);

                var result = await _context.SaveChangesAsync() > 0; // burası 1 veya 0 döndürür aslında veitabanuna birşey yazılmadıysa 0 değeri döner.

                if(!result) return Result<Unit>.Failure("Failed to create activity");

                return Result<Unit>.Success(Unit.Value); // değişiklikleri veritabanımıza kaydetmenin sonucunu kontrol etmektir. değer dönmüyor burası mecbur bizde sonuca bakarız.
            }
        }
    }
}