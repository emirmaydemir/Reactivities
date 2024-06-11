using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]// Model Doğrulama, HTTP 400 Hata Kodları, Dolaylı Bağlama vb özellikleri kazandırız bize
    [Route("api/[controller]")] //[controller] yer tutucusu, Controller sınıfının adını alır ve bu adı route şablonunda kullanır. Bu kullanım, Controller sınıflarının adının değiştiğinde veya başka bir Controller sınıfı eklediğinizde, route şablonunu her seferinde güncellemenize gerek olmadan dinamik bir şekilde uyum sağlamak için kullanışlıdır.
    //Üsttekileri bu temel apiden türeyen controllere yazmıyoruz çünkü onlarda bu özelliklere sahip oluyor otomatik olarak.
    public class BaseApiController : ControllerBase
    {
        //bunun private yapılmasının sebebi sadece bu sınıftan erişilsin istiyoruz
        private IMediator _mediator;
        
        //bunun protected yapılmasının sebebi ise BaseApiControlleri miras alan her controllerin Mediatora erişebilmesini sağlamak istiyoruz.
        //bu sayede her controller içerisinde constructor açıp mediator hizmetini tanımlamak zorunda kalmayacağız baseapi de tanımladık ve diğer controller bunu miras alıp beleşten kullanacak bu hizmeti.
        //Bu özellik, _mediator alanına, eğer değeri null ise servis sağlayıcıdan mediator örneği alınır ve kaydedilir. Daha sonra _mediator değeri artık null olmadığı için, bir daha servis çağrısı yapılmaz ve _mediator değeri doğrudan kullanılır.
        //yani bir controller mediatorü kullanmak istediğinde servislerde tanımladığımız "builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));" kodu çalışarak mediatorumuzu alacağız ve bu değeri _mediator değişkeni içerisine kaydettiğimiz için birdaha servisi boş yere çağırmyacağız.
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if(result == null) return NotFound();
            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);
            if (result.IsSuccess && result.Value == null)
                return NotFound();
            return BadRequest(result.Error);
        }
    }
}