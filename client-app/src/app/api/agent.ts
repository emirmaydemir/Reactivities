// Bu sınıf api istelerimizi gruplamak ve kolaylaştırmak için var.
import axios, { AxiosError, AxiosResponse } from "axios";
import { Activity, ActivityFormValues } from "../models/activity";
import { toast } from "react-toastify";
import { router } from "../router/Routes";
import { store } from "../stores/store";
import { User, UserFormValues } from "../models/user";
import { Photo, Profile } from "../models/profile";

// Promise, gelecekte tamamlanacak (veya başarısız olacak) bir işlemi temsil eder. resolve: İşlem başarılı olduğunda çağrılır.
const sleep = (delay: number) => {
  return new Promise((resolve) => {
    setTimeout(resolve, delay);
  });
};

axios.defaults.baseURL = "http://localhost:5000/api"; //temel url miz bir yere url yazınca başına bu gelecek sonuna ise fonksiyona parametre olarak gelen değer gelecek.

//Bu kod parçası, Axios isteklerinde bir interceptor (ara kesici) kullanarak her istekten önce belirli bir işlemi gerçekleştirir. Özetle, bu kod, her Axios isteği gönderilmeden önce istek yapılandırmasını kontrol eder ve Authorization başlığına bir JWT (JSON Web Token) ekler. İşte adım adım açıklaması:
//Bunu yapmazsak istekten sonra tokeni kontrol edebilir ve yetkisiz işlem hatası alırız.
axios.interceptors.request.use((config) => {
  const token = store.commonStore.token;
  if (token && config.headers) config.headers.Authorization = `Bearer ${token}`; //Bearer diye başlar jwt tokenler o yüzden onu yazdık başa.
  return config;
});

//Bu kod parçasında, bir response interceptor kullanılarak API cevapları geciktirilmektedir. Request interceptors ise api isteğinden önce çalışır.
//Api cevaplarını geciktirmek için kullanıyoruz.
axios.interceptors.response.use(
  async (response) => {
    await sleep(1000);
    return response;
  },
  (error: AxiosError) => {
    // Burada kısaca 400 hatası varsa gelen hata bilgilerini modalstate isimli bir dizide döndürüp bunları react tarafında kullanıcıya göstereceğiz. Flat ise dizide 2 veya daha fazla elemanlı başka diziler varsa yani dizi içinde dizi varsa bunları teke indirger yani dizi içinde dizi olmasını engeller.
    const { data, status, config } = error.response as AxiosResponse;
    switch (status) {
      case 400:
        // Kullanıcılar, URL'leri veya form alanlarını manipüle ederek geçersiz id değerleri gönderebilirler. Bu, güvenlik açısından önemli olabilir çünkü kötü niyetli bir kullanıcı, sistemdeki varlıkları keşfetmek veya özel bilgilere erişmeye çalışabilir.
        //Hata yanıtındaki data.errors nesnesinin id adında bir özelliği olup olmadığını kontrol eder. Ve İsteğin HTTP metodunun GET olup olmadığını kontrol eder.
        if (config.method === "get" && data.errors.hasOwnProperty("id")) {
          router.navigate("/not-found");
        }
        if (data.errors) {
          const modalStateErrors = [];
          for (const key in data.errors) {
            if (data.errors[key]) {
              modalStateErrors.push(data.errors[key]);
            }
          }
          throw modalStateErrors.flat();
        } else {
          toast.error(data);
        }
        break;
      case 401:
        toast.error("unauthorised");
        break;
      case 403:
        toast.error("forbidden");
        break;
      case 404:
        router.navigate("/not-found");
        break;
      case 500:
        store.commonStore.setServerError(data);
        router.navigate("/server-error");
        break;
    }
    return Promise.reject(error); // bu konsolda görmek için yukarıdakiler ekranda uyarı mesajı vermek için toast library.
  }
);

const responseBody = <T>(response: AxiosResponse<T>) => response.data; //bu api isteklerinde response.data yazmak yerine sadece response yazmamızı sağlayacak.

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: {}) =>
    axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
  del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Activities = {
  list: () => requests.get<Activity[]>("/activities"),
  details: (id: string) => requests.get<Activity>(`/activities/${id}`),
  create: (activity: ActivityFormValues) =>
    requests.post<void>("/activities", activity),
  update: (activity: ActivityFormValues) =>
    requests.put<void>(`/activities/${activity.id}`, activity),
  delete: (id: string) => requests.del<void>(`/activities/${id}`),
  attend: (id: string) => requests.post<void>(`/activities/${id}/attend`, {}),
};

const Account = {
  current: () => requests.get<User>("/account"),
  login: (user: UserFormValues) => requests.post<User>("/account/login", user),
  register: (user: UserFormValues) =>
    requests.post<User>("/account/register", user),
};

const Profiles = {
  get: (username: string) => requests.get<Profile>(`/profiles/${username}`),
  //Application katmanına bakacak olursan bu iş için Add.cs isimli bir sınıfın var ve FormData verisi alıyor. FormData HTTP isteklerinde dosya yüklemelerini yönetmek için kullanılan bir arayüzdür. Parametre olarak gelen dosyamızı apiye göndereceğiz.
  //Burada form data eklediğimiz için request değilde axios kullanıyoruz çünkü requests içinde tanımladığımız body için uygun değil bunun bodysi
  uploadPhoto: (file: any) => {
    let formData = new FormData(); //Öncelikle boş bir FormData nesnesi oluşturuyoruz.
    formData.append("File", file); //ÖNEMLİ!! - Sonra dosyamızı formdata nesnesine ekliyoruz fakat ismini File yaptık çünkü Add.cs içerisinde bulunan IFormFile File değişkeni ile eşleşmeli ismi.
    return axios.post<Photo>("photos", formData, {
      //photos controllerine formdatamızı gönderiyoruz ve Burada Content-Type olarak multipart/form-data belirtilmiştir, bu da sunucuya form verisi gönderileceğini bildirir.
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  //Ana fotoğrafı güncellemek için.
  setMainPhoto: (id: string) => requests.post(`/photos/${id}/setMain`, {}),
  //Ana fotoğrafı silmek için.
  deletePhoto: (id: string) => requests.del(`/photos/${id}`),
  //Biografi ve ad bilgilerini güncelleyebilmek için.
  //Partial açıklama: Bu durumda, updateProfile fonksiyonu Partial<Profile> türünde bir nesne alır ve bu nesne sadece bazı Profile özelliklerini içerebilir. Bu, profilde yalnızca değişiklik yapmak istediğiniz alanları belirterek bir profil güncellemesi yapmanıza olanak tanır.
  //Partial: Yani tüm profile nesnesini almak yerine sadece bio ve display nameyi alacağız.
  updateProfile: (profile: Partial<Profile>) =>
    requests.put<void>(`/profiles`, profile),
  //Takip etmek veya takibi bırakmak için.
  updateFollowing: (username: string) =>
    requests.post(`/follow/${username}`, {}),
  //Takip edilenlerin veya takipçilerin listesini döndürür. Predicate hangisini döndüreceğimize karar verir.
  listFollowings: (username: string, predicate: string) =>
    requests.get<Profile[]>(`/follow/${username}?predicate=${predicate}`),
};

const agent = {
  Activities,
  Account,
  Profiles,
};

export default agent;
