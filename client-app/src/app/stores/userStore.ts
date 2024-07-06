import { makeAutoObservable, runInAction } from "mobx";
import { User, UserFormValues } from "../models/user";
import agent from "../api/agent";
import { store } from "./store";
import { router } from "../router/Routes";

export default class UserStore {
  user: User | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  get isLoggedIn() {
    return !!this.user;
  }

  login = async (creds: UserFormValues) => {
    try {
      const user = await agent.Account.login(creds); //formu doldurulan kullanıcı bilgileri token ile birlikte geldi.
      store.commonStore.setToken(user.token); //kullanıcının token bilgisi kaydedildi ve içeriğine bakarsan setTokenin locale kaydediyor.
      runInAction(() => (this.user = user)); //Api işlemlerinden sonra ayarlama işlemleri run içerisinde yapılmalıdır.
      router.navigate("/activities");
      store.modalStore.closeModal(); //Login olursa giriş modelini kapatıyoruz sayfanın ortasında beliren küçük kutuyu kapatmazsak sürekli açık kalır çünkü.
    } catch (error) {
      throw error;
    }
  };

  register = async (creds: UserFormValues) => {
    try {
      //Ayrıca kayıt olurken aynı mail kullanıcı adı kullanılmasın diye controllerde kontrol yapılıyor merak etme.
      const user = await agent.Account.register(creds); //formu doldurulan kullanıcı bilgileri token ile birlikte geldi.
      store.commonStore.setToken(user.token); //kullanıcının token bilgisi kaydedildi ve içeriğine bakarsan setTokenin locale kaydediyor.
      runInAction(() => (this.user = user)); //Api işlemlerinden sonra ayarlama işlemleri run içerisinde yapılmalıdır.
      router.navigate("/activities");
      store.modalStore.closeModal(); //Login olursa giriş modelini kapatıyoruz sayfanın ortasında beliren küçük kutuyu kapatmazsak sürekli açık kalır çünkü.
    } catch (error) {
      throw error;
    }
  };

  logout = () => {
    store.commonStore.setToken(null); //token sıfırlandı
    this.user = null;
    router.navigate("/"); // ana sayfaya dönüldü
  };

  getUser = async () => {
    //şu an hangi kullanıcı ile sistemdeysek onu getirir token vs tüm bilgilerini getirir.
    try {
      const user = await agent.Account.current();
      runInAction(() => (this.user = user));
    } catch (error) {
      console.log(error);
    }
  };

  //Bu fonksiyon mevcut kullanıcı profil fotoğrafını değiştirirse anlık olarak değişsin diye var.
  setImage = (image: string) => {
    if (this.user) this.user.image = image;
  };
  //Bu fonksiyon profile tarafında displayname değiştirilince mevcut kullanıcının adının da değişmesini sağlar.
  setDisplayName = (name: string) => {
    if (this.user) this.user.displayName = name;
  };
}
