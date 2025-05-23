using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class List
    {
        public class Query : IRequest<Result<List<CommentDto>>>
        {
            public Guid ActivityId { get; set; } //Yorum yapılan aktivite id bilgisi geliyor.
        }

        public class Handler : IRequestHandler<Query, Result<List<CommentDto>>>
        {
            private readonly DataContext _context; //Yorumları veritabanından getirmek için
            private readonly IMapper _mapper; //Yorumları CommentDto türünde döndürmek için
            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                //Burada Comments veritabanında hangi aktiviteye ait yorumları görmek istiyorsan onların listesini tarihine göre sıralayıp getiriyoruz.
                var comments = await _context.Comments
                    .Where(x => x.Activity.Id == request.ActivityId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectTo<CommentDto>(_mapper.ConfigurationProvider) //Döndüreceğimiz verilerin CommentDto türünde olmasını sağlıyoruz.
                    .ToListAsync();

                return Result<List<CommentDto>>.Success(comments);
            }
        }
    }
}