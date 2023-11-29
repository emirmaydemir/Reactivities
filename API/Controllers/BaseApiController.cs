using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]// Model Doğrulama, HTTP 400 Hata Kodları, Dolaylı Bağlama vb özellikleri kazandırız bize
    [Route("api/[controller]")] //[controller] yer tutucusu, Controller sınıfının adını alır ve bu adı route şablonunda kullanır. Bu kullanım, Controller sınıflarının adının değiştiğinde veya başka bir Controller sınıfı eklediğinizde, route şablonunu her seferinde güncellemenize gerek olmadan dinamik bir şekilde uyum sağlamak için kullanışlıdır.
    //Üsttekileri bu temel apiden türeyen controllere yazmıyoruz çünkü onlarda bu özelliklere sahip oluyor otomatik olarak.
    public class BaseApiController : ControllerBase
    {
        
    }
}