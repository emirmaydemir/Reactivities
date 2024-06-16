using API.Extensions;
using API.Middleware;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Buradaki kod sayesinde tüm controllerimize auth işlemlerini yapacak yani her işlemde token soracak postman üzerinden verilerimize erişemezler çünkü müşterilerimize verdiğimiz tokeni ve keyi bilmiyorlar.
//Controller içerisine bunları yazmasaydık her controllerin başına [Authorize] yazmak zorunda kalırdık.
builder.Services.AddControllers(opt => 
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
//burada kendi oluşturduğumuz extension sınıfımızdan aldık servisleri yeni servis ekledikçe oraya ekliyoruz bu sayede program.cs içerisi temiz gözüküyor.
builder.Services.AddApplicationServices(builder.Configuration);
//burada kendi oluşturduğumuz identity sınıfımızdan aldık servisleri burada auth işlemleri hizmetleri falan var.
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

//Kendi yazdığım ara yazılımı entegre ettim.
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy"); //tarayıcının güvenmesi için ara katman olarakta ekledik bu cors hizmetini

app.UseAuthentication(); //Önce kimlik doğrulama gelir. Çünkü kullanıcı doğru kullanıcı mı kontrol edilir burada. Sonra alttaki servis çalışır.

app.UseAuthorization(); //Sonra yetkilendirme gelir.

app.MapControllers();
//Bu hizmet kapsamı, genellikle bir istekle gelen HTTP talebinin işlenme süresi boyunca geçerlidir. Dolayısıyla, her istek için yeni bir hizmet kapsamı oluşturulur ve bu kapsam içinde çalışan hizmetler, işlem sona erdiğinde temizlenir.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
