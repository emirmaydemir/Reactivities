import { makeAutoObservable, runInAction } from "mobx";
import { Photo, Profile } from "../models/profile";
import agent from "../api/agent";
import { toast } from "react-toastify";
import { store } from "./store";

export default class ProfileStore {
  profile: Profile | null = null;
  loadingProfile = false;
  uploading = false;
  loading = false;

  constructor() {
    makeAutoObservable(this); //Bu yukarıda oluşturduğumuz değişkenleri otomatik olarak algılaması için kullanılan bir yapı.
  }

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
}
