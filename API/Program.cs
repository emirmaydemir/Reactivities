using API.Extensions;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//burada kendi oluşturduğumuz extension sınıfımızdan aldık servisleri yeni servis ekledikçe oraya ekliyoruz bu sayede program.cs içerisi temiz gözüküyor.
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");//tarayıcının güvenmesi için ara katman olarakta ekledik bu cors hizmetini

app.UseAuthorization();

app.MapControllers();
//Bu hizmet kapsamı, genellikle bir istekle gelen HTTP talebinin işlenme süresi boyunca geçerlidir. Dolayısıyla, her istek için yeni bir hizmet kapsamı oluşturulur ve bu kapsam içinde çalışan hizmetler, işlem sona erdiğinde temizlenir.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
