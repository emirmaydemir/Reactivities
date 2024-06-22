using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Security
{
    //Bu sınıf bir aktiviteyi host dışında biri düzenleyemesin diye var yani sadece aktiviteyi oluşturan kişi düzenlesin diye var. Hostu buluyoruz burada.
    public class IsHostRequirement : IAuthorizationRequirement
    {
        
    }
    public class IsHostRequirementHandler : AuthorizationHandler<IsHostRequirement>
    {
        private readonly DataContext _dbContext; //Veritabanı bağlamımız.
        private readonly IHttpContextAccessor _httpContextAccessor; ////IHttpContextAccessor kullanarak HTTP bağlamına (HTTP context) erişir. Bu sınıf, HTTP isteği sırasında kullanıcının kimliğine dair bilgileri almak için kullanılır.
        public IsHostRequirementHandler(DataContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        //AuthorizationHandlerContext identity ile AspNet tabloları oluşturmuştuk ya işte o tablolara erişmek için AuthorizationHandlerContext kendi oluşturduğum tablolara erişmek için ise DataContext kullanacağız.
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsHostRequirement requirement)
        {
            //ClaimTypes.NameIdentifier şu anda sistemden istek atan kullanıcının id bilgisini getirir. identity ile otomatik oluşturulmuş olan user tablosunda bu id bilgisine ait kullanıcıyı getiriyoruz. User tablosunu ben oluşturmadım identity sayesinde gelen aspnet tablolarında yer alıyor.
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Eğer kullanıcılar tablomuzda bu id bilgisine ait birisi bulunamazsa yetkilendirme işlemini sağlayamadığı içinyetkisiz işlem return ediyoruz.
            if (userId == null) return Task.CompletedTask;

            //Bu kod bize http isteği sırasında oluşan url içerisinden id parametresine ait id değerini döndürmelidir.
            var activityId = Guid.Parse(_httpContextAccessor.HttpContext?.Request.RouteValues
                .SingleOrDefault(x => x.Key == "id").Value?.ToString());

            //Etkinlik ve kullanıcı id bilgilerine bakarak bize bu bilgilere ait katılımcı objesini döndürür.
            var attendee = _dbContext.ActivityAttendees
                .AsNoTracking() //Bellekte tutmaması için kullanıyoruz bu kodu aksi taktirde etkinliği düzenleyince hostu siliyordu durduk yere.
                .FirstOrDefaultAsync(x => x.AppUserId == userId && x.ActivityId == activityId)
                .Result;

            //Eğer bir katılımcımız yoksa, o zaman görevi bir kez daha yetkisiz işlem noktasına geri döndüreceğiz
            if (attendee == null) return Task.CompletedTask;

            //Eğer katılımcı host kullanıcı ise yetkiyi veriyoruz requirement yetkili işlem demektir.
            if (attendee.IsHost) context.Succeed(requirement);
            
            //Host değilse yetkisiz işlem return ediyoruz çünkü aktiviteyi sadece host düzenleyebilir.
            return Task.CompletedTask;
        }
    }
}