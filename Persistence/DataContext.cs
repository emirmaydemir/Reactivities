using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    //Identity için DbSet belirtmemize gerek kalmadı normalde her tablo için belirtilir ama bunun tablolarını sistem kendi algılıyor ve oluşturuyor.
    public class DataContext : IdentityDbContext<AppUser>
    {
        //base kelimesi, bir sınıfın temel sınıfına (base class) erişim sağlar. Yani, bir sınıf diğer bir sınıftan türemişse (inheritance), türeyen sınıf içinde base kelimesi kullanılarak temel sınıfın öğelerine (property, method, constructor vb.) erişebilirsiniz.
        //Dolayısıyla, : base(options) ifadesi, DbContext sınıfının constructor'ını çağırarak, DbContext sınıfının temel sınıfının (base class) constructor'ını yürütür ve bu sayede DbContext sınıfının başlatılmasını gerçekleştirir. Bu başlatma işlemi, içerisine geçirilen options parametresi aracılığıyla veritabanı bağlamının gerekli yapılandırmalarını alır. 
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        //Activity türündeki nesnelerin veritabanındaki Activities tablosunu temsil ettiğini belirtiyoruz.
        //DbSet üzerinde çeşitli sorgular ve işlemler gerçekleştirilebilir. Örneğin, veritabanına yeni nesneler eklemek, varolan nesneleri sorgulamak veya güncellemek gibi işlemler DbSet üzerinden yapılır.
        public DbSet<Activity> Activities { get; set; } //Activities, veritabanındaki tablonun adını belirtirken DbSet<Activity> ifadesi, Domain altında yer alan Activity modelinin değişkenlerini kullanarak tablodaki sütunları temsil eder.
        public DbSet<ActivityAttendee> ActivityAttendees { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Comment> Comments { get; set; }

        //Bu yapılandırma, ActivityAttendee tablosunun AppUser ve Activity tabloları arasında bir ara tablo(join table) olarak hizmet vermesini sağlar, böylece bir kullanıcının birden fazla etkinliğe katılabilmesi ve bir etkinliğin birden fazla katılımcısı olabilmesi mümkün hale gelir.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ActivityAttendee>(x => x.HasKey(aa => new { aa.AppUserId, aa.ActivityId }));
            //Bire çok ilişkileri anlamak için --> https://www.youtube.com/watch?v=ZOkUgCAJXXk
            builder.Entity<ActivityAttendee>()
                .HasOne(u => u.AppUser)
                .WithMany(u => u.Activities)
                .HasForeignKey(aa => aa.AppUserId);

            builder.Entity<ActivityAttendee>()
                .HasOne(u => u.Activity)
                .WithMany(u => u.Attendees)
                .HasForeignKey(aa => aa.ActivityId);
            
            builder.Entity<Comment>()
                .HasOne(a => a.Activity)
                .WithMany(c => c.Comments)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}