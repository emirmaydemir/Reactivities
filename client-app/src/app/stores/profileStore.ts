import { makeAutoObservable, reaction, runInAction } from "mobx";
import { Photo, Profile } from "../models/profile";
import agent from "../api/agent";
import { toast } from "react-toastify";
import { store } from "./store";

export default class ProfileStore {
  profile: Profile | null = null;
  loadingProfile = false;
  uploading = false;
  loading = false;
  followings: Profile[] = [];
  loadingFollowings = false;
  activeTab = 0;

  constructor() {
    makeAutoObservable(this); //Bu yukarıda oluşturduğumuz değişkenleri otomatik olarak algılaması için kullanılan bir yapı.
    /*reaction Nedir?
      MobX'te reaction, bir "tepki" fonksiyonudur. Belirli bir observable (gözlemlenebilir) değeri izler ve bu değer her değiştiğinde bir yan etki (side effect) oluşturur. reaction ile iki ana bileşen vardır:

      İzlenen değer (reaction'in ilk argümanı): Bir fonksiyon döndürür ve MobX buradaki değişkenin (observable state) değişimini izler.
      Tepki fonksiyonu (reaction'in ikinci argümanı): İzlenen değer her değiştiğinde çalışacak bir fonksiyondur. */
    reaction(
      () => this.activeTab, // İzlenen değer -- bu her değiştiğinde reaction devreye girer.
      (activeTab) => {
        // Tepki fonksiyonu
        if (activeTab === 3 || activeTab === 4) {
          const predicate = activeTab === 3 ? "followers" : "following";
          this.loadFollowings(predicate); // Tab değişimi takipçi veya takip edilenlerin bilgilerini yüklemek için API çağrısı yapar.
        } else {
          this.followings = []; // Eğer diğer tab'lar aktifse followings listesi temizlenir.
        }
      }
    );
  }

  //ProfileContent ekranında takipçi veya takip edilenler sekmesine bastığımızda bize index numarası geliyor onu bu fonksiyon ile değişkenimize atıyoruz.
  setActiveTab = (activeTab: any) => {
    this.activeTab = activeTab;
  };

  //Mobx sınıflarını kendi içerisinde de çağırabiliriz ya da react kodları arasında çağırabiliriz.
  //Burada mobx sınıflarımızdan userStoreyi kullanarak şu an giriş yapan kullanı adını çektik ve bu ad profiline baktığımız kişi ile aynı kişi ise (yani kişi kendi profiline mi bakıyor diye kontrol ediyoruz) true döndü değilse false dönüyor.
  get isCurrentUser() {
    if (store.userStore.user && this.profile) {
      return store.userStore.user.username === this.profile.userName;
    }
    return false;
  }

  loadProfile = async (username: string) => {
    this.loadingProfile = true;
    try {
      const profile = await agent.Profiles.get(username);
      runInAction(() => {
        //Yukarıda bulunan değişkenleri güncellerken kullanılması gerekiyor.
        this.profile = profile;
        this.loadingProfile = false;
      });
    } catch (error) {
      toast.error("Problem loading profile");
      runInAction(() => {
        this.loadingProfile = false;
      });
    }
  };

  uploadPhoto = async (file: Blob) => {
    this.uploading = true;
    try {
      const response = await agent.Profiles.uploadPhoto(file);
      const photo = response.data;
      runInAction(() => {
        // Fotoğrafı yükleme başarılı ise profile nesnemizede ekliyoruz bu sayede mevcut kullanıcının fotğrafları arasında gözükecek fotoğrafımız. Sayfayı yenilemesine gerek kalmayacak. Mobx ile erişebilecek.
        if (this.profile) {
          this.profile.photos?.push(photo);
          //Eğer foto mevcut kullancınınn profil fotoğrafı ise onu da anlık olarak güncelliyoruz mobx üzerinden erişip foto anlık olarak güncellensin diye.
          if (photo.isMain && store.userStore.user) {
            store.userStore.setImage(photo.url);
            this.profile.image = photo.url;
          }
        }
        this.uploading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.uploading = false));
    }
  };

  //Gelen foto nesnesi ile eski main fotoğrafın main özelliğini devre dışı bırakıp yeni fotoyu main olarak belirliyoruz.
  setMainPhoto = async (photo: Photo) => {
    this.loading = true;
    try {
      await agent.Profiles.setMainPhoto(photo.id); //Fotoyu main yapması için apimize gönderdik ama mobxte de yapmamız lazım o yüzden aşağıdaki adımları yapıyoruz.
      store.userStore.setImage(photo.url); //Kullanıcının profile ekranındaki fotosunu güncellemeden önce user yani profil fotosunun bulunduğu ekrandakini de güncelliyoruz mobx kısmında.
      store.activityStore.setImage(photo.url); //Kullanıcının profile ekranındaki fotosunu güncellemeden önce activity yani aktivite ekrandakini fotosunuda güncelliyoruz mobx kısmında.
      runInAction(() => {
        if (this.profile && this.profile.photos) {
          this.profile.photos.find((a) => a.isMain)!.isMain = false; //Eskiden main olan fotonun main özelliğini kaldırdık.
          this.profile.photos.find((a) => a.id === photo.id)!.isMain = true; //Yeni fotoyu main yapıyoruz burada.
          this.profile.image = photo.url; //Mevcut kullanıcının profil fotosunu yeni foto yapıyoruz api ile istek atarak yapmıştık ama sayfayı yenilemeye gerek kalmasın diye mobx ile de yaptık.
          this.loading = false;
        }
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.loading = false));
    }
  };

  //Fotoğrafı silmek için
  deletePhoto = async (photo: Photo) => {
    this.loading = true;
    try {
      await agent.Profiles.deletePhoto(photo.id);
      //Burada photos listemizden silinen fotoyu filter methodu ile kaldırıyoruz ve bu sayede mobx değişkenimiz olan profile nesnesi güncel kalıyor kullanıcının sayfayı yenileyip tekrar api isteği atmasına gerek kalmıyor. Bu, sayfayı yenilemeden verilerin güncellenmesini ve gösterilmesini sağlar.
      runInAction(() => {
        if (this.profile) {
          this.profile.photos = this.profile.photos?.filter(
            (a) => a.id !== photo.id
          );
          this.loading = false;
        }
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.loading = false));
    }
  };

  updateProfile = async (profile: Partial<Profile>) => {
    this.loading = true;
    try {
      await agent.Profiles.updateProfile(profile); //API tarafında profilimizi güncelliyoruz fakat sayfayı yenilemedende verilerin değişmesi gerekdiği için mobx tarafını da güncelliyoruz mobx güncellemesi runinaction içerisinde yapılır.
      runInAction(() => {
        //Eğer display name değiştirildiyse profile tarafında kullanıcının profil resminin oradaki ve aktivitelerdeki adı değişmesi gerektiği için user tarafında da güncelleme yapıyoruz.
        if (
          profile.displayName &&
          profile.displayName !== store.userStore.user?.displayName
        ) {
          store.userStore.setDisplayName(profile.displayName); //profile tarafının adını güncelledik fakat kullanıcının profil fotosnun orada yazan adıda güncellememiz lazım yoksa sayfayı yenilemeden güncellenmez. Bunun için userStore içerisinde yazdığım fonksiyonu çağırıyorum.
          store.activityStore.setDisplayName(profile.displayName); //profile tarafının adını güncelledik fakat aktivite hostları ve katılımcılar tarafında yazan adıda güncellememiz lazım yoksa sayfayı yenilemeden güncellenmez. Bunun için activityStore içerisinde yazdığım fonksiyonu çağırıyorum.
        }
        this.profile = { ...this.profile, ...(profile as Profile) }; //Bu iki nesneyi birleştirir ve yeni bir nesne oluşturur. Yeni nesnede, profile nesnesindeki özellikler this.profile nesnesindekileri geçersiz kılar. Yani eski profile nesnesini yeni profile nesnesinde değişen değerler ile günceller.
        this.loading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.loading = false));
    }
  };

  //Bu method takip etme veya takipten çıkma sonrası hem db tarafında hem görünen ekranda ilgili güncellemeleri yapar. Yani hem apiye istek atar hem de mobx değişikliklerini yapar.
  updateFollowing = async (username: string, following: boolean) => {
    this.loading = true;
    try {
      await agent.Profiles.updateFollowing(username); // burada apiye istek atarak db de takip bilgilerini güncelliyoruz.
      store.activityStore.updateAttendeeFollowing(username); // Burada aktivite tarafında tüm takip bilgilerini güncelliyoruz. Db tarafında güncellendi fakat sayfayı yenilemeden ekranda değişikliklerin güncellenmesi için bunu yapmamız gerekiyor.
      //RunInAction mobx değişkenlerini değiştireceğimiz zaman kullanmamız gereken ifadedir yani en üstte bulunan değişkenleri değiştireceğimiz zaman lazım oluyor.
      //Burada profil tarafında takip bilgilerini güncelliyoruz db de değişti fakat sayfayı yenilemeye gerek kalmadan profil tarafında da güncel olması gerekiyor bilgilerin.
      runInAction(() => {
        //takip ettiğimiz veya takipten çıktığımız kişinin profiline bakıyorsak ekranın anlık olarak güncellenmesini sağlayan kod
        if (
          this.profile &&
          this.profile.userName !== store.userStore.user?.username &&
          this.profile.userName === username
        ) {
          following
            ? this.profile.followersCount++
            : this.profile.followersCount--;
          this.profile.following = !this.profile.following;
        }
        //takip ettikten veya takipten çıktıktan sonra kendi profilimize bakıyorsak ekranın anlık olarak güncellenmesini sağlayan kod
        if (
          this.profile &&
          this.profile.userName === store.userStore.user?.username
        ) {
          following
            ? this.profile.followingCount++
            : this.profile.followingCount--;
        }
        //takip ettikten veya takipten çıktıktan sonra tüm profillerde takip ve takipçi bilgilerinin anlık olarak güncellenmesini sağlayan kod. Sayfayı yenileyince hepsi güncellenir fakat biz sayfayı yenilemeden güncel kalsın istiyoruz.
        this.followings.forEach((profile) => {
          if (profile.userName === username) {
            profile.following
              ? profile.followersCount--
              : profile.followersCount++;
            profile.following = !profile.following;
          }
        });
        this.loading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.loading = false));
    }
  };

  //Takip ettiklerini ya da takipçilerini döndürmek için kullanıyoruz.
  loadFollowings = async (predicate: string) => {
    this.loadingFollowings = true; //Ufak bir gecikme efekti vermek için.
    try {
      //burada agent.ts üzerinden apiye istek atarak takip edilenleri ya da takipçileri döndürüyoruz.
      const followings = await agent.Profiles.listFollowings(
        this.profile!.userName,
        predicate
      );
      //mobx değişkeni olan followings içerisine de takipçileri atıyoruz çünkü db de yer alan bu veriler bize lazımk sayfayı yenilemeye gerek kalmadan bazı işlemler yapabilmek için.
      runInAction(() => {
        this.followings = followings;
        this.loadingFollowings = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => (this.loadingFollowings = false));
    }
  };
}
