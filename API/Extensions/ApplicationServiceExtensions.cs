using Application.Activities;
using Application.Core;
using Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Photos;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        //burada 2 parametre gözüküyor ama program.cs te tek parametre alıyoruz bunun sebebi this olması buradakini kullan parametereye gerek yok diyor.
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
         IConfiguration config)
         {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            //dbcontextimizi servis olarak sunduk bu sayede dependencies enjection ile her yerde DataContext yazarak ulaşabiliriz buna
            services.AddDbContext<DataContext>(opt => {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            //Burada ise tarayıcımızın apimize güvenmesini sağladık http://localhost:3000 adresinden gelen tüm post put delete başlıklarına ve methodlarına izin ver dedik cors policy
            services.AddCors(opt =>{
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000");
                });
            });
            //Bu kod parçası, bir ASP.NET Core uygulamasında MediatR kütüphanesini kullanmanıza yardımcı olur.
            //Bu kullanım, belirli bir assembly içindeki tüm MediatR handler'larını otomatik olarak kaydetmek için RegisterServicesFromAssemblies yöntemini kullanır. Yani, List.Handler sınıfının bulunduğu assembly içindeki tüm MediatR handler'larını otomatik olarak kaydeder.
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            //oto doğrulama servisini ekledik.
            //services.AddFluentValidationAutoValidation() metodu, FluentValidation'u ASP.NET Core model doğrulama sistemine entegre eder. Bu, ASP.NET Core'un model bağlama süreci sırasında FluentValidation doğrulayıcılarının otomatik olarak çalıştırılmasını sağlar.
            services.AddFluentValidationAutoValidation();
            //Burada create ile editin doğrulama özellikleri aynı olduğu için birini tanısa yetiyor o yüzden createyi verdik önbelleğine kaydetmesi için ve dependency injection yaptık.
            //Create burada sadece bir örnektir. Amacı, FluentValidation'un hangi assembly'yi tarayacağını belirtmektir. Sizin örneğinizde Create yerine başka bir sınıf da kullanılabilir. Önemli olan, bu sınıfın, validator sınıflarınızın bulunduğu aynı assembly'de olmasıdır. Eğer Edit doğrulama kuralları da aynı assembly'deyse, Create sınıfını kullanmanız yeterlidir, çünkü AddValidatorsFromAssemblyContaining<Create>() tüm assembly'yi tarar ve tüm validator sınıflarını bulur.
            services.AddValidatorsFromAssemblyContaining<Create>();
            //AddHttpContextAccessor(): IHttpContextAccessor hizmetini DI (Dependency Injection) konteynerine ekler. Bu, herhangi bir sınıfın HTTP bağlamına erişebilmesini sağlar.
            services.AddHttpContextAccessor();
            //arayüzünü uygulayan UserAccessor sınıfını DI konteynerine ekler. Bu sayede, bir istek kapsamı boyunca aynı UserAccessor örneği kullanılır.
            services.AddScoped<IUserAccessor, UserAccessor>();

            // JSON serileştirme ayarları: Bu ayar sayesinde json olarak dönen adlarının pascal case olarak kalmasını sağlayacaktır. Ben adı UserName yapınca userName dönüyor çünkü.
            /*services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });*/

            services.AddScoped<IPhotoAccessor, PhotoAccessor>();
            //appsettings.jsondan cloudinary fotoğraf bulutunun ayralarının bulunduğu alanı çekip - CloudinarySettings sınıfımız türünde döndürüyoruz.
            services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
            //Chat için kullandığımız servis.
            services.AddSignalR();

            return services;
         }
    }
}