using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Create
    {
        public class Command : IRequest<Result<CommentDto>>
        {
            public string Body { get; set; }
            public Guid ActivityId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Body).NotEmpty(); //Kullanıcının girdiği yorum boş olmasın diye validate ekledik.
            }
        }

        public class Handler : IRequestHandler<Command, Result<CommentDto>>
        {
            private readonly DataContext _context; //Yorumları veritabanına kaydetmek için.
            private readonly IMapper _mapper; //Normalde Create işleyicilerinde birşey döndürmediğimiz IMapper kullanmıyorduk fakat burada yorumu atan kullanıcının ismini ve resmini CommentDto cinsinden döndürmemiz gerektiği için mapper lazım bize.
            private readonly IUserAccessor _userAccessor; //Yorum atan kullanıcıya erişmek için.

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                //ÖNEMLİ AÇIKLAMA
                //Veribanından yorum atılan aktiviteyi getireceğiz fakat bunu getirirken aktivi içerisinde bulunan Comments, Comments içerisinde bulunan Author ve Author içerisinde bulunan Photos nesnesini de algılaması için include methodu ile belirtiyoruz.
                var activity = await _context.Activities
                    .Include(x => x.Comments)
                    .ThenInclude(x => x.Author)
                    .ThenInclude(x => x.Photos)
                    .FirstOrDefaultAsync(x => x.Id == request.ActivityId);


                if (activity == null) return null; //Aktivite bulunamazsa null döndürüp hata sayfamıza yönlendiririz.

                var user = await _context.Users
                    .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername()); //Yorumu atan kullanıcıya ait bilgileri getiriyoruz veritabanından comment tablosuna kaydedebilmek için.

                var comment = new Comment //comment nesnesi oluşturup yukarıda getirdiğimiz yorumu atan kullanıcı, yorumun atıldığı aktivite ve mesajın içeriği bilgileri ile dolduruyoruz.
                {
                    Author = user,
                    Activity = activity,
                    Body = request.Body
                };

                activity.Comments.Add(comment); //Aktiviteler tablosundaki yorumlar listesine atılan yorumu ekliyoruz bu sayede o aktiviteye ait yorumları görüntüleyebiliriz.

                var success = await _context.SaveChangesAsync() > 0; //Değişiklikleri kaydediyoruz ve başarılı ise 1 döner.

                if (success) return Result<CommentDto>.Success(_mapper.Map<CommentDto>(comment)); //İşlem başarılı ise başarılı mesajı ile birlikte Yorumları CommentDto türünde dönüyoruz. CommentDto türünde dönebilmek için mapperi kullanıyoruz eşleştirsin Comment veribanaı ile CommentDto nesnesini. Yorumu dönme sebebimiz yorumu atan kişiye ait resim ve isim bilgileri lazım bize yorum atıldıktan sonra.

                return Result<CommentDto>.Failure("Failed to add comment"); //Yorum atılırken hata olursa hata mesajını döndürüyoruz.
            }
        }
    }
}