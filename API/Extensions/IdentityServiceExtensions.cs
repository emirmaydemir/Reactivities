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
                    /*Peki neden mesajlaşmada jwt token doğrulama lazım
                    1. Token Yenileme
                    JWT token'larının belirli bir geçerlilik süresi vardır. Kullanıcı sürekli bağlantı üzerinden iletişim kurarken, bu token'ların geçerliliği sona erebilir. Bu nedenle, yeni bir token sağlanması gerekebilir ve bu token'ın her istekle birlikte doğrulanması önemlidir.
                    2. Farklı İstemciler ve Oturumlar
                    Bir kullanıcının birden fazla istemciden veya oturumdan bağlanması mümkündür. Her istemcinin ve oturumun ayrı ayrı kimlik doğrulama yapması gerekir. Bu nedenle, her istek için token doğrulaması yapılması gerekebilir.
                    */
                    /*Bu kod parçası, /chat yolu üzerinden gelen HTTP isteklerinde sorgu parametrelerinden alınan access_token'ı JWT doğrulama sürecine dahil eder. Bu, WebSocket veya SignalR gibi durumlarda, token'ların sorgu parametreleri üzerinden iletilmesini ve doğrulanmasını sağlar.
                    access_token sorgu parametresini alır. İstek yolunun /chat ile başladığını kontrol eder. Eğer her iki koşul da sağlanıyorsa, bu token'ı JWT doğrulama için kullanır.
                    Bu şekilde, belirli bir yol üzerinden gelen istekler için JWT doğrulama sürecine esneklik kazandırılmış olur.*/
                    //Burası SignalR hub kullanırken token kullanmamız için var
                    //OnMessageReceived olayı üzerinde işlem yaparak belirli bir URL yolu için '("/chat")' sorgu parametrelerinden alınan JWT'yi doğrulama sürecine dahil eder. 
                    opt.Events = new JwtBearerEvents //Bu kısım, JwtBearerEvents nesnesi oluşturur ve OnMessageReceived olayını tanımlar. OnMessageReceived olayı, bir mesaj alındığında tetiklenir ve JWT token'ı alınıp doğrulanmadan önce müdahale edebilmenizi sağlar.
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"]; //Bu satır, gelen HTTP isteğinde bulunan access_token sorgu parametresini alır. Eğer sorgu parametrelerinde bir access_token varsa, bu değeri accessToken değişkenine atar. Yani kullanıcının tokenini burada alıyoruz.
                            var path = context.HttpContext.Request.Path; //Bu satır, gelen HTTP isteğinin yolunu (path) alır.
                            if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat")) //Bu koşul, access_token sorgu parametresi doluysa ve istek yolu /chat ile başlıyorsa kontrol eder. Yani chat yapmak isteyen kullanıcı var demektir.
                            {
                                context.Token = accessToken; //Eğer bu iki koşul da sağlanıyorsa, context.Token değeri accessToken olarak ayarlanır. Bu, JWT doğrulama işlemi için kullanılacak token'ı belirler.
                            }
                            return Task.CompletedTask; //İşlem başarılı diye dümenden mesaj
                        }
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