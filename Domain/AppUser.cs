using Microsoft.AspNetCore.Identity;

namespace Domain
{
    //Sadece DisplayName ve Bio yazdık çünkü kullanıcı adı mail vb özelliklerin hepsi hizmetlerimiz sayesinde gelecek ve tablolarda yer alacak merak etme bu identitye özel bir hizmet.
    public class AppUser : IdentityUser
    {
        public string DisplayName {get; set;}
        public string Bio {get; set;}
        public ICollection<ActivityAttendee> Activities {get; set;}
    }
}