using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService
    {
        //IConfiguration sayesnde appsetting.development içerisindeki değerlere erişebiliyoruz.
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config)
        {
            _config = config; 
        }
        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
            };

            //bu anahtarı kodladığımızda simetrik bir güvenlik anahtarı elde ederiz. veya bu anahtarı şifrelediğimizde, şifrelemek için kullandığımız anahtar aynı zamanda şifresini çözmek için de kullanılır.
            //simetrikte aynı anahtar ile şifreleyip aynı anahtar ile çözeriz şifreyi.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
            //tokenin simetrik güvenlik anahtarı kullanılarak imzalandığı yer burasıdır. Sha 512 ile imzalıyoruz.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            //Bir jsw token sunucuya verilmeden önce 3 adet bölümden oluşur
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // başlık bilgilerim ben olduğumu iddia ettiğim
                Expires = DateTime.UtcNow.AddDays(7), // token ka gün boyunca kullanılabilir
                SigningCredentials = creds    // şifrelenmiş anahtar alanı
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            //tokeni oluşturduk.
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}