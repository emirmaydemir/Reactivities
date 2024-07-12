using Application.Comments;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class ChatHub : Hub
    {
        //API denetleyicilerini kullanmak yerine, uç noktalar olarak hub'larımızı sinyal olarak kullanacağız.

        private readonly IMediator _mediator;
        public ChatHub(IMediator mediator)
        {
            _mediator = mediator; //Yine medıator aracılığı ile aplication sınıflarımızlar cqrs iletişimi yapacağız burasıda bir controller gibi düşünebilirsin ama SignalR, sunucu ve istemci arasında sürekli açık bir bağlantı kurar ve bu bağlantı üzerinden verilerin anında iletilmesini sağlar.
        }

        public async Task SendComment(Create.Command command) //Bu metod, yeni bir yorum gönderildiğinde çalışır. İşlevselliği adım adım inceleyelim:
        {
            var comment = await _mediator.Send(command); //Yorumu application katmanında yer alan create işleyicisinde işleyip veri tabanına kaydeder. Aynı zamanda create tarafında işlem başarı ile yorum döner çünkü sürekli olarak yorum akışı sağlanmalı o yüzden create işleyicimiz yorumu döndürüyor.

            await Clients.Group(command.ActivityId.ToString()) //command.ActivityId.ToString() ifadesi, yorumun yapıldığı etkinliğin kimliğini alır. Clients.Group metodu, bu etkinlik kimliğine sahip gruptaki tüm bağlantılı istemcilere mesaj göndermek için kullanılır.
                .SendAsync("ReceiveComment", comment.Value); //SendAsync("ReceiveComment", comment.Value) çağrısı, tüm grup üyelerine ReceiveComment adında bir SignalR olayı gönderir ve yorumun içeriğini (comment.Value) iletir.
        }

        public override async Task OnConnectedAsync() //Bu metod, yeni bir istemci (kullanıcı) bağlantı kurduğunda çalışır. İşlevselliği adım adım inceleyelim:
        {
            var httpContext = Context.GetHttpContext(); // çağrısı, istemcinin bağlantı isteğiyle ilişkili HTTP bağlamını alır.
            var activityId = httpContext.Request.Query["activityId"]; // ifadesi, bağlantı isteğindeki activityId adlı sorgu parametresini alır. Bu, kullanıcının hangi etkinliğe bağlandığını belirtir.
            //Context.ConnectionId SignalR içindeki her bir istemci bağlantısının benzersiz tanımlayıcısıdır yani id bilgisidir.
            await Groups.AddToGroupAsync(Context.ConnectionId, activityId); //Groups.AddToGroupAsync(Context.ConnectionId, activityId) çağrısı, istemciyi belirli bir etkinlik grubuna ekler. Context.ConnectionId istemcinin bağlantı kimliğini, activityId ise etkinlik kimliğini temsil eder.
            var result = await _mediator.Send(new List.Query{ActivityId = Guid.Parse(activityId)}); //Veritabanından etkinlikle ilgili yorumları getirir.
            await Clients.Caller.SendAsync("LoadComments", result.Value); //ifadesi, yeni bağlanan istemciye (Caller), LoadComments adında bir SignalR olayı gönderir ve etkinliğe ait yorumları (result.Value) iletir.
        }
    }
}