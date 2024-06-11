using Application.Core;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>> 
        {
            public Activity Activity {get; set;}
        }

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
            private readonly IMapper _mapper;
            public Handler(DataContext context, IMapper mapper)
            { 
                _mapper = mapper;     
                _context = context; 
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(request.Activity.Id);

                if(activity == null) return null;
                
                //postmanda atılan activity değişkenleri ile title, description gibi activity modelimizde yer alan title ve desc gibi değişkenleri eşleştiriyoruz çünkü postmanda gelen title ise Modelde Title oluyor t harfleri uyuşmadığından sıkıntı olabilir bu bunun önüne geçmek için postman kullanıyoruz.
                //_mapper.Map(request.Activity, activity); satırı, AutoMapper kütüphanesini kullanarak request.Activity nesnesindeki verileri activity nesnesine kopyalar. Bu sayede, activity nesnesi, request.Activity nesnesindeki güncel verilere sahip olur.
                _mapper.Map(request.Activity, activity);

                var result = await _context.SaveChangesAsync() > 0;

                if(!result) return Result<Unit>.Failure("Failed to update activity");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}