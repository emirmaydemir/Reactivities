using Application.Activities;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //buna root falan yazmadık çünkü Bir kökü vardır ve denetleyici tabanından türetilir çünkü temel API'mize bunu yerleştirdik.
    public class ActivitiesController : BaseApiController
    {
        //private readonly DataContext _context; //readonly özniteliği, bu alanın sadece kurucu metot içinde başlatılabileceğini ve başlatıldıktan sonra değerinin değiştirilemeyeceğini belirtir.

        //services.AddDbContext<DataContext>(options =>
        //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        //program.cs içerisinde tanımladığımız bu kodda persistence içerisinde yer alan DataContext türünde bir şey nesne oluşturursak 
        //ASP.net core dependency injection kullanmış olduk aslında yazdığımız bu servis sayesinde ASP.NET Core runtime'ı bu parametreyi otomatik olarak çözer ve enjekte eder. 
        //Aşağıdaki konfigürasyon, DataContext sınıfının ActivitiesController'ın kurucu metoduna otomatik olarak enjekte edilmesini sağlar.
        //public ActivitiesController(DataContext context)
        //{
        //    _context = context;         
        //}

        //Bu nedenle, Mediator.Send metoduna bir sorgu nesnesi geçiriyorsunuz. Bu sorgu nesnesi, işlemek istediğiniz talebi temsil eder ve bu talep belirli bir işleyici (handler) tarafından ele alınacaktır.
        //Eğer Mediator.Send metoduna bir işleyici sınıfı geçirseydiniz, bu durumda doğrudan işleyici sınıfı üzerinden bir talep oluşturmuş olur ve MediatR'ın tasarım felsefesine aykırı bir durum ortaya çıkardınız. Çünkü işleyici sınıfı, bir talebi ele almak yerine, sadece belirli bir talep türü ile ilişkilidir.
        //--Bu controller base api'den türediği için orada yer alan protected değişken olan Mediatoru kullanabiliyor--
        [HttpGet] //api/activities
        public async Task<ActionResult> GetActivities()
        {
            return HandleResult(await Mediator.Send(new List.Query()));
        }
        
        //içerisine parametre olarak yazdığımız şey url ekinde nasıl çağıracağımızı belirler yukarıdakini //api/activities diye çağırırken bunu //api/activities/id şeklinde çağırıyoruz.
        //burada yer alan id ismi ile fonksiyonda parametre olarak yer alan id eşleşmeli çünkü requestte gelen id onunla eşleşip anlayabilir.
        //Kimlik doğrulamamızın yolu, bahsettiğim gibi, JWT taşıyıcı jetonunu bir yetki belgesiyle göndermektir. JWT bearer diye bir şey kullandık identity service extensionda onun sayesinde algılayacak bunu.
        //[Authorize]  Bu kod sayesinde aktiviteye ulaşmanın tek yolu kimlik doğrulama olacaktır. Yani postman üzerinden saldırı yapamayacaklar bize headere token bilgisini eklemeleri gerekecek falan yani güvenliği sağladık. Ama bunu kullanmamıza gerek kalmadı çünkü program.cs içerisinde bunu tüm controllere entegre ettik addcontroller hizmeti içerisinde.
        [HttpGet("{id}")] //api/activities/'burada istekte gelen id bilgisi yer alacak'
        //yukarıda bahsettiğim  gibi mediator hizmetini bir kere list sınıfı ile sağladığımız için kaydoldu zaten diğer sınıflar için ekstradan kütüphane importuna gerek kalmıyor.
        public async Task<ActionResult> GetActivity(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query{Id = id}));
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity(Activity activity)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Activity = activity }));
        }

        [Authorize(Policy = "IsActivityHost")] //Infrastructure içinde yazdığımız IsHostRequirement kodunda sadece host olan kişiler aktiviteyi düzenleyebilsin demiştik. Bu belirteci ekliyoruz ki orada return ettiğimiz reqirementi algılasın ve sadece yetkili kişilere düzenmele yetkisi versin.
        [HttpPut("{id}")]
        public async Task<IActionResult> EditActivity(Guid id, Activity activity)
        {
            activity.Id = id;

            return HandleResult(await Mediator.Send(new Edit.Command { Activity = activity }));
        }

        [Authorize(Policy = "IsActivityHost")] //Sadece yetkili kişiler silebilsin diye yani aktivitenin hostları silebilir sadece.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id}));
        }

        [HttpPost("{id}/attend")]
        public async Task<IActionResult> Attend(Guid id)
        {
            return HandleResult(await Mediator.Send(new UpdateAttendance.Command { Id = id}));
        }
    }
}