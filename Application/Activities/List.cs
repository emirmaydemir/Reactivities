using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        //Bu sınıf, bir talebi (query) temsil eder. IRequest<T> arayüzünü uygular ve bu durumda List<Activity> tipinde bir sonuç döndürecek bir sorguyu belirtir.
        public class Query : IRequest<List<Activity>>{}

        //Bu sınıf, bir talebi (query) işleyen bir handler'ı temsil eder. 
        //ilk paramatre alacağı değer ikincisi ise döndüreceği değerin türü
        public class Handler : IRequestHandler<Query, List<Activity>>
        {
            private readonly DataContext _context;
            //constructor içerisinde db contexti alıyoruz çünkü verileri çekmek için db ye bağlanacağız
            public Handler(DataContext context)
            {
                _context = context;  
            }
            public async Task<List<Activity>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Activities.ToListAsync();
            }
        }

    }
}