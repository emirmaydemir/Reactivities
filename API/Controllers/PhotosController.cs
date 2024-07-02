using Application.Photos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PhotosController : BaseApiController
    {
        /*
        [FromForm] özniteliği, ASP.NET Core'da, Add.Command nesnesinin HTTP POST isteğindeki form verilerinden doldurulacağını belirtir. Bu, form verilerinin otomatik olarak Add.Command nesnesine bağlanmasını sağlar.
        Örneğin, bir kullanıcı bir dosya yükleme formunu doldurup gönderdiğinde: Dosya (IFormFile File) formdan alınır ve command.File özelliğine atanır. 
        EN ÖNEMLİ AÇIKLAMA!!!
        Zaten postmanda da form-data kısmından seçtik bunu ve key kısmına File yazdık Add.cs içerisinde yer alan File ile eşleşmesi gerekiyor aksi taktirde çalışmaz
        */
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] Add.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command{Id = id}));
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMain(string id)
        {
            return HandleResult(await Mediator.Send(new SetMain.Command{Id = id}));
        }
    }
}