using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using FluentValidation;

namespace Application.Activities
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        //Burada kurallar tanımladık aktivitilerin şu özellikleri boş olursa uyarı versin şeklinde Applicationserviceextensioda da entegre ettiğimiz için apiden istek geldiği gibi otomatik algılıyor burada yazdıklarımı ve boş olan alan varsa uyarıyı veriyor.
        //Örneğin başlık ya da açıklama boş bırakılırsa uyarı verecek.
        public ActivityValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Category).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.Venue).NotEmpty();
        }
    }
}