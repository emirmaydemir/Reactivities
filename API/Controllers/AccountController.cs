using System.Security.Claims;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]// Model Doğrulama, HTTP 400 Hata Kodları, Dolaylı Bağlama vb özellikleri kazandırız bize
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            
        }

        //[AllowAnonymous] özniteliği (attribute), ASP.NET Core'da bir denetleyici (controller) veya eylem (action) üzerinde kullanıldığında, o denetleyici veya eylemin kimlik doğrulaması gerektirmediğini belirtir.
        [AllowAnonymous] //Bunu yaptık çünkü program.cs içerinde AddControllers servisinde tüm controllere kimlik doğrulamayı zorunlu kıldık. Bu sebepten ötürü tüm controller token isteyecek fakat account controller için token kontrolüne gerek yoktur. Bu controlleri pas geç demek istedik burada.
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email); //Dbden kullanıcı maili varsa kullanıcı bilgilerini döndürüyor bize.

            if(user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password); //ASP net identity bizim için hazırlamış tüm bu fonksiyonları aktardığımız şifreyi db deki ile karşılaştırıp bize true or false dönüyor.

            if(result)
            {
                return CreateUserObject(user);
            }

            return Unauthorized();
        }
         
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //IdntityServiceExtensions içerisinde mail ve şifre ile ilgili kontroller var fakat kullanıcı adı için hazır kontrol kodu yazmamış geliştiriciler o yüzden kendimiz yazıyoruz.
            if(await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username", "Username taken"); //ModelState isimli bir tanımlayıcı var .net corede hatamızı buna key value şeklinde verip öyle döndürüyoruz.
                return ValidationProblem(); //Sistemin oluşturduğu hata mesajları formatında hata mesajı oluşturmamı sağlıyor yukarıda tanımladığım hata mesajını sisteme entegre etmiş oldum bir nevi.
            }

            if(await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email)) //Bunun kontrolü identity özellikleri arasında yer alıyor fakat hata mesajı içi böyle bir if açtık diğer hata mesajları bizi tatmin etmedi.
            {
                ModelState.AddModelError("email", "Email taken"); //ModelState isimli bir tanımlayıcı var .net corede hatamızı buna key value şeklinde verip öyle döndürüyoruz.
                return ValidationProblem(); //Sistemin oluşturduğu hata mesajları formatında hata mesajı oluşturmamı sağlıyor yukarıda tanımladığım hata mesajını sisteme entegre etmiş oldum bir nevi.
            }

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if(result.Succeeded) //Kayıt başarılı ise sunucumuzdan yani apiden UserDto nesnemizi döndürüyoruz postmanda istek atınca bu değerlerin döndüğünü görebilirsin.
            {
                return CreateUserObject(user);
            }

            return BadRequest(result.Errors);
        }

        //Burayı tokene bağlı kıldık authorize yazarak diğer endpointler tokensizde çalışıyor.
        //[Authorize] özniteliği, bu eylemin kimliği doğrulanmış kullanıcılara açık olduğunu belirtir.
        [Authorize] //Yukarıda AllowAnonymous demiştik tüm controllere uygulanıyor normalde ama bu uç nokta için uygulanmasın demek istedik. Burası için kimlik doğrulama yapılacak yani. 
        [HttpGet] //hiçbir parametremiz olmayacak. Yani birisi API hesabı uç noktasına bir istekte bulunursa, o zaman onların bilgilerini alacağız.
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            //AÇIKLAMALARI OKU ÇOK ÖNEMLİ AŞAĞIDAKİ 3 AÇIKLAMAYIDA OKU.
            //Bu mekanizma, kullanıcının kimliği doğrulanmış bir istekte bulunmasını gerektirir, token'ındaki e-posta claim'ini kullanarak kullanıcıyı bulur ve kullanıcının bilgilerini bir DTO (Data Transfer Object) olarak döner. Bu şekilde, istemci tarafında kullanıcı bilgileri ve token'ı yeniden kullanılabilir.
            //Yani postmandan token ile istek atıyoruz ya bu endpointe işte o tokenin başlık bilgisinde email, username gibi bilgiler yer alıyor ya heh işte istek atan tokendeki Email bilgisini çekip mevcut kullanıcıyı buluyoruz.
            //Sonuçta her kullanıcının tokeni unique yani eşsiz. User ve claim denen şeyde tokenin içerisinde yer alan Email oluyor. Bu uç noktaya sadece token ile istek atabiliyoruz.
            var user = await _userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));

            return CreateUserObject(user);

        }

        private UserDto CreateUserObject(AppUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = null,
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }
    }
}