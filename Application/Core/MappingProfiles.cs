using Application.Activities;
using Application.Comments;
using AutoMapper;
using Domain;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            string currentUsername = null;
            CreateMap<Activity,Activity>();

            //EN ÖNEMLİ AÇILAMA bu map işlemi sayesinde sadece gerekli yerlerin maplenmesini ve çekilmesini sağladık mesela
            //MESELA ActivityAttendee ve Profile maplemesinde bilerek profile diye bir sınıf oluşturduk  ve sadece veritabanı sorgusunda userName, displayName ve bioyu çektik
            //Oysa ki ActivityAttendee içerisinde IdentityUserden kaynaklı bir sürü alan var kullanıcı mailinden tut şifresine kadar biz formember ile sadece bize lazım olanları mapledik
            //Eğer böyle yapmasaydık tüm alanları çekerdi ve sorgularımız yavaşlardı bizde aktivite listesi ve detay sayfası için kullanabileceğimiz bir mapleme yaptık.
            //VE KISACA MAPLEMEDE TABLOYA EŞDEĞER BİR SINIF OLUŞTURUP MAPLİYORUZ. SADECE GEREKEN ÖZELLİKLERİ ÇEKİYORUZ.

            //Bu satır şunu sağlıyor bizim Activity tablomuzda Id, title, date, description, category, city ve venue gibi özelliklerimiz bulunuyor fakat HostUsername bulunmuyordu.
            //Aslında bulunuyor Attendees nesnesi içinde bulunuyor ama bunun içini okuması mümkün değil o yüzden bizde diyoruz ki ActivityDto da yer alan HostUserName bizim Attendees altında yer Alan AppUserin bir özelliği onunla aynı işlevdedir diyoruz.
            //Peki burada da diyeceksin ki AppUserde UserName isimli bir değişken göremiyorum ben de sana diyeceğim ki IdentityUseri unuttun mu bize bir sürü tablo sağlıyordu ve sağladığı özellikler arasında kullanıcı adı, şifre ve mail gibi şeyler vardı.
            //Yani Username özelliği IdentityUserden geliyor zaten AspNetUsers tablosuna bakarsan göreceksin orada yer alıyor.
            /*
            Bu satır, Activity sınıfından ActivityDto sınıfına bir haritalama oluşturur. Burada HostUsername özelliği özel olarak yapılandırılmıştır:
            HostUsername, Activity nesnesinin Attendees koleksiyonundan ev sahibi (IsHost özelliği true olan) olan katılımcının kullanıcı adı (AppUser.UserName) olarak haritalanır.
            */
            CreateMap<Activity,ActivityDto>()
                .ForMember(d => d.HostUsername, o => o.MapFrom(s => s.Attendees
                    .FirstOrDefault(x => x.IsHost).AppUser.UserName));
            
            //Kısacası burası sayesinde aktivite katılımcılarında aşağıdaki özellikler yer alıyor ve image main foto olarak gözüküyor.
            //Şimdi şöyle ActivityAttendee sınıfında AppUser sınıfından türemiş başka bir nesne bulunuyor ve bu nesnenin DisplayName, UserName ve Bio adlı üç özelliğini kullanmak istiyoruz.
            //Bu yüzden diyoruz ki profilede yer alan DisplayName Appuserdeki ile aynı -- Keza UserName ve Bio da aynı şekilde diye belirtiyoruz.
            //Aktivite katılımcılarında image kısmına main kontrolü yaparar ana fotoğrafın kullanıcı profilinde olmasnı sağlıyoruz.
            /*
            Bu satır, ActivityAttendee sınıfından Profile sınıfına bir haritalama oluşturur. Burada çeşitli özellikler özel olarak yapılandırılmıştır:
            /////////////////////////////////////////////////////////////////////////////////////////////
            DisplayName, ActivityAttendee nesnesinin AppUser özelliğindeki DisplayName olarak haritalanır.
            UserName, ActivityAttendee nesnesinin AppUser özelliğindeki UserName olarak haritalanır.
            Bio, ActivityAttendee nesnesinin AppUser özelliğindeki Bio olarak haritalanır.
            */
            CreateMap<ActivityAttendee, AttendeeDto>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.AppUser.DisplayName))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.AppUser.UserName))
                .ForMember(d => d.Bio, o => o.MapFrom(s => s.AppUser.Bio))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.AppUser.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.AppUser.Followers.Count))
                .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.AppUser.Followings.Count))
                .ForMember(d => d.Following,
                    o => o.MapFrom(s => s.AppUser.Followers.Any(x => x.Observer.UserName == currentUsername))); //Burada şu an sayfasına baktığımız kullanıcının takipçiler listesinde mevcut giriş yapan kullanıcı bulunuyor mu diye true veya false dönüyoruz. Bu sayede sayfada takip ediliyor ya da takip ediniz diye bir ayrım yapabiliriz.
            
            //AppUser yani (Users) içerisinde Image değişkenimiz bulunmadığı için Profile ile mapliyoruz ve Image değişkenini belirtiyoruz.
            //İkisi arasındaki tek fark image zaten o yüzden onu mapledik User tablosu yerine Profile dtosunu kullanabilmek için 
            //Aynı zamanda imagenin main foto olmasını sağlıyoruz yani image değişkenini içerisine kullanıcının ana fotosunu koyuyoruz.
            CreateMap<AppUser, Profiles.Profile>()
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.Followers.Count))
                .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.Followings.Count))
                .ForMember(d => d.Following,
                    o => o.MapFrom(s => s.Followers.Any(x => x.Observer.UserName == currentUsername))); //Burada şu an sayfasına baktığımız kullanıcının takipçiler listesinde mevcut giriş yapan kullanıcı bulunuyor mu diye true veya false dönüyoruz. Bu sayede sayfada takip ediliyor ya da takip ediniz diye bir ayrım yapabiliriz.

            //Çok kısa ve öz açıklıyorum şimdi CommentDto türünde veri döndüreceğiz ama bu verilerin Comment veritabanı ile eşleşmesi gerekiyor
            //CreatedAt, Body ve Id gibi bilgiler birebir eşleştiği için bunları maplemeye gerek yok.
            //Ama Author ismini verdiğimiz AppUser nesnesi içerisindeki UserName, DisplayName ve Image değişkenlerini algılaması için maplememiz lazım çünkü bunlar Authorun içerisinde ve otomatik maplenemiyor elimizle yapmamız gerekiyor.
            //Peki neden Dto kullanıyoruz mesela kullanmasaydık Comments veritabanındaki tüm verileri aktarırdık Appuser nesnesinde bir sürü sütün var ama biz sadece lazım olanları aktarmak istediğimiz için Dto kullanıyoruz.
            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.Author.UserName))
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Author.DisplayName))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Author.Photos.FirstOrDefault(x => x.IsMain).Url));
        }
    }
}