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
                return BadRequest("Username is already taken");
            }

            if(await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email)) //Bunun kontrolü identity özellikleri arasında yer alıyor fakat hata mesajı içi böyle bir if açtık diğer hata mesajları bizi tatmin etmedi.
            {
                return BadRequest("Email is already taken");
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
        [Authorize] //Yukarıda AllowAnonymous demiştik tüm controllere uygulanıyor normalde ama bu uç nokta için uygulanmasın demek istedik. Burası için kimlik doğrulama yapılacak yani. 
        [HttpGet] //hiçbir parametremiz olmayacak. Yani birisi API hesabı uç noktasına bir istekte bulunursa, o zaman onların bilgilerini alacağız.
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
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