using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security
{
    public class UserAccessor : IUserAccessor
    {
        //SECURITY altındaki sınıflar genelde sistemdeki kullanıcının kim olduğunu bulmak ile alakalı oluyor. Burada şu an hangi kullanıcı istek atıyorsa onu buluyoruz.
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        //Peki buna neden ihtiyacımız var? API projemizin bağlamı içerisinde değiliz ve HTTP bağlamına erişmemiz gerekiyor. Bunu bu arayüz üzerinden yapabiliriz çünkü HTTP bağlamımız kullanıcı nesnelerimizi içerir ve Kullanıcı nesnelerimiz aracılığıyla token içerisindeki iddialara erişebiliriz.
        //IHttpContextAccessor kullanarak HTTP bağlamına (HTTP context) erişir. Bu sınıf, HTTP isteği sırasında kullanıcının kimliğine dair bilgileri almak için kullanılır.
        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
        }
        
        //Kullanıcı adını çekiyoruz http isteği sırasındaki.
        public string GetUsername()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }
    }
}