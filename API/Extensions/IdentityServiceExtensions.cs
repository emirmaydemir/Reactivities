using System.Text;
using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace API.Extensions
{
    public static class IdentitynServiceExtensions
    {
        //ASP.NET Core Identity sistemi sayesinde veritabanında bulunan ASP ile başlayan tablolar oluşturuldu .AddEntityFrameworkStores<DataContext>() ile bağladık ve migration kodunu terminala yazarak oluşturmuş olduk.
        //pass hashleri identity kendi algortiması ile yaptı baya süreçten geçen bir algoritma
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            //bu sadece belirtilen kullanıcı tipi için kimlik sistemini ekler veya ekler ve yapılandırır.
            services.AddIdentityCore<AppUser>(opt =>{
                opt.Password.RequireNonAlphanumeric = false;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<DataContext>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            //!!ÖNEMLİ-- Token doğru mu anlamak için key yetiyor bize key ile şifreleyip aynı key ile şifreyi çözüyoruz çünkü
            //Kimlik doğrulamak için servisimiz burası içerisinde keyimizi falan verdik tokeni kontrol edecek doğru tokenle giriyor muyuz diye. Token doğru mu anlamak için key yetiyor bize key ile şifreleyip aynı key ile şifreyi çözüyoruz çünkü
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt => 
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        //Burası yayıncının imzalama anahtarını doğrular. Bura olmadığı taktirde imzalanmış veya imzalanmamış her türlü eski token kabul edilecektir.
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false, // ValidateIssuer ve ValidateAudience tokene eklemediğimiz için burada false yaptık.
                        ValidateAudience = false
                    };
                });

            //Infrastructure katmanı içerisinde yazdığımız aktivite yetkilendirme kodunu burada servis olarak sunuyoruz. IsHostRequirement isimli sınıfta yer alıyor.
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("IsActivityHost", policy => 
                {
                    policy.Requirements.Add(new IsHostRequirement());
                });
            });
            services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
            //services.AddScoped<TokenService>() ifadesi, TokenService sınıfının bir HTTP isteği boyunca aynı örnekle kullanılmasını sağlar ve bu sınıfın diğer bileşenlere enjekte edilmesini kolaylaştırır.
            services.AddScoped<TokenService>(); // Kendi oluşturduğum servisi eklemek için kullanılan kod parçası.

            return services;
        }
    }
}