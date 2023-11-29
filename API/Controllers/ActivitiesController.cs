using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    //buna root falan yazmadık çünkü Bir kökü vardır ve denetleyici tabanından türetilir çünkü temel API'mize bunu yerleştirdik.
    public class ActivitiesController : BaseApiController
    {
        private readonly DataContext _context; //readonly özniteliği, bu alanın sadece kurucu metot içinde başlatılabileceğini ve başlatıldıktan sonra değerinin değiştirilemeyeceğini belirtir.

        //services.AddDbContext<DataContext>(options =>
        //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        //program.cs içerisinde tanımladığımız bu kodda persistence içerisinde yer alan DataContext türünde bir şey nesne oluşturursak 
        //ASP.net core dependency injection kullanmış olduk aslında yazdığımız bu servis sayesinde ASP.NET Core runtime'ı bu parametreyi otomatik olarak çözer ve enjekte eder. 
        //Aşağıdaki konfigürasyon, DataContext sınıfının ActivitiesController'ın kurucu metoduna otomatik olarak enjekte edilmesini sağlar.
        public ActivitiesController(DataContext context)
        {
            _context = context;         
        }

        [HttpGet] //api/activities
        public async Task<ActionResult<List<Activity>>> GetActivities()
        {
            return await _context.Activities.ToListAsync();
        }

        //içerisine parametre olarak yazdığımız şey url ekinde nasıl çağıracağımızı belirler yukarıdakini //api/activities diye çağırırken bunu //api/activities/id şeklinde çağırıyoruz.
        //burada yer alan id ismi ile fonksiyonda parametre olarak yer alan id eşleşmeli çünkü requestte gelen id onunla eşleşip anlayabilir.
        [HttpGet("{id}")] //api/activities/'burada istekte gelen id bilgisi yer alacak'
        public async Task<ActionResult<Activity>> GetActivity(Guid id)
        {
            return await _context.Activities.FindAsync(id);
        }

    }
}