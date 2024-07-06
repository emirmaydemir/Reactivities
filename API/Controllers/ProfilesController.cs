using Application.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProfilesController : BaseApiController
    {
        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
        }

        //Burada command sınıfına istek atarken bio ve displayname bilgilerini yazmadığımı fark etmişsindir buradaki olayı aşağıda açıklıyorum
        //Bu mekanizma, HTTP isteğinin gövdesinden (body) gelen verileri alır ve bunları ilgili parametreye dönüştürür. Bu sayede, command nesnesinin DisplayName ve Bio özellikleri, HTTP isteğinin gövdesinde gönderilen JSON verileriyle doldurulacaktır.
        //Yani client tarafından atılan istekte commandın içerisine profile nesnesi yer alacak ve bunun içerisinden bio ve displayname seçilecek merak etme sonrasında sınıfta işleyecek gerekli işlemleri.
        //HandleResult BaseApiController içerisinde tanımlanmış bir fonksiyondur hata mesajlarını döndürek kısım orasıdır.
        [HttpPut]
        public async Task<IActionResult> Edit(Edit.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }
    }
}