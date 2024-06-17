import { makeAutoObservable, reaction } from "mobx";
import { ServerError } from "../models/serverError";

export default class CommonStore {
  error: ServerError | null = null;
  token: string | null | undefined = localStorage.getItem("jwt"); // web localine aldığımız bir token varsa direkt kaydeder yoksa undefined olarak başlar.
  appLoaded = false;

  constructor() {
    makeAutoObservable(this);

    //Reaction sadece değişim olunca çalışır yani tokenin başlangıç değeri var mı bilmez sadece hareket olunca çalışır.
    //Mağazamız başlatılır başlatılmaz otomatik çalıştırma adı verilen farklı bir tepki türü,
    //reaction fonksiyonu, iki argüman alır. İlk argüman, gözlemlenecek bir ifadedir (this.token), ikinci argüman ise bu ifadede bir değişiklik olduğunda çalıştırılacak bir etkidir (bir callback fonksiyonu).
    //Özetle, bu kod parçası this.token'daki değişiklikleri izler ve bu değişikliklere göre yerel depolamaya token'ı kaydeder veya siler. this.token değiştiğinde: Eğer this.token bir değere sahipse, bu değer "jwt" anahtarıyla yerel depolamaya kaydedilir. Eğer this.token değeri yoksa, yerel depolamadan "jwt" anahtarıyla kaydedilmiş olan değer silinir.
    reaction(
      () => this.token,
      (token) => {
        if (token) {
          localStorage.setItem("jwt", token);
        } else {
          localStorage.removeItem("jwt");
        }
      }
    );
  }

  setServerError(error: ServerError) {
    this.error = error;
  }

  //Yerel Depolamaya Veri Yazma: localStorage.setItem("jwt", token); ifadesi, token değişkeninin değerini tarayıcının yerel depolama alanına "jwt" anahtarıyla kaydeder. Yerel depolama, verileri tarayıcıda depolamak ve bu verilere farklı sayfalardan erişmek için kullanılır. "jwt" burada JSON Web Token'ı temsil edebilir.
  setToken = (token: string | null) => {
    //if (token) localStorage.setItem("jwt", token);
    this.token = token;
  };

  setAppLoaded = () => {
    this.appLoaded = true;
  };
}
