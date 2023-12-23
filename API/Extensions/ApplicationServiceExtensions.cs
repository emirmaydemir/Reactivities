using Application.Activities;
using Application.Core;
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
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000");
                });
            });
            //Bu kod parçası, bir ASP.NET Core uygulamasında MediatR kütüphanesini kullanmanıza yardımcı olur.
            //Bu kullanım, belirli bir assembly içindeki tüm MediatR handler'larını otomatik olarak kaydetmek için RegisterServicesFromAssemblies yöntemini kullanır. Yani, List.Handler sınıfının bulunduğu assembly içindeki tüm MediatR handler'larını otomatik olarak kaydeder.
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            return services;
         }
    }
}