using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Activities
{
    public class List
    {
        //Bu sınıf, bir talebi (query) temsil eder. IRequest<T> arayüzünü uygular ve bu durumda List<Activity> tipinde bir sonuç döndürecek bir sorguyu belirtir.
        public class Query : IRequest<Result<List<ActivityDto>>>{}

        //Bu sınıf, bir talebi (query) işleyen bir handler'ı temsil eder. 
        //ilk paramatre alacağı değer ikincisi ise döndüreceği değerin türü
        public class Handler : IRequestHandler<Query, Result<List<ActivityDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            //constructor içerisinde db contexti alıyoruz çünkü verileri çekmek için db ye bağlanacağız
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _mapper = mapper;
                _context = context;
            }
            public async Task<Result<List<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                /* Lazy Loading vs. Eager Loading
                    Lazy Loading (Tembel Yükleme): Varsayılan olarak, Entity Framework Core'da ilişkili veriler, yalnızca ihtiyaç duyulduğunda yüklenir. Yani, Activity nesnesi çekildiğinde, Attendees koleksiyonu yüklenmez. Bu koleksiyona erişmeye çalıştığınızda, veri tabanına ek bir sorgu gönderilerek veriler getirilir. Bu, performans açısından sorun yaratabilir ve "N+1" problemleri olarak bilinen durumlara yol açabilir.
                    Eager Loading (Erken Yükleme): Include ve ThenInclude metodları kullanılarak, ilişkili tüm veriler tek bir sorguda yüklenir. Bu, veri çekme işlemini optimize eder ve veri tabanına gönderilen sorgu sayısını azaltır.*/

                //Burada döndüreceğimiz şeyin bir ActivityDto nesnesi olmasını sağladık ProjectTo sayesinde yoksa veritabanındaki Activity modelimiz dönerdi ama biz mapleme ayarını yaptığımız ActivityDto nesnesinin dönmesini istiyoruz.
                //Automapper sayesinde select sorgusu ile uzun uzun tüm sütunları çekme yükünden kurtulduk.
                var activities = await _context.Activities
                    .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider, 
                        new {currentUsername = _userAccessor.GetUsername()}) // bu satırı ekledik çünkü MappingProfiles içerisinde şu an sistemde olan kullanıcıya erişmemiz gerekiyordu bizde buradan gönderiyoruz.
                    .ToListAsync();

                return Result<List<ActivityDto>>.Success(activities);
            }
        }

    }
}