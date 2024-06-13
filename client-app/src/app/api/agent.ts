// Bu sınıf api istelerimizi gruplamak ve kolaylaştırmak için var.
import axios, { AxiosError, AxiosResponse } from "axios";
import { Activity } from "../models/activity";
import { toast } from "react-toastify";
import { router } from "../router/Routes";
import { store } from "../stores/store";

// Promise, gelecekte tamamlanacak (veya başarısız olacak) bir işlemi temsil eder. resolve: İşlem başarılı olduğunda çağrılır.
const sleep = (delay: number) => {
  return new Promise((resolve) => {
    setTimeout(resolve, delay);
  });
};

axios.defaults.baseURL = "http://localhost:5000/api"; //temel url miz bir yere url yazınca başına bu gelecek sonuna ise fonksiyona parametre olarak gelen değer gelecek.

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

const request = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: {}) =>
    axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
  del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Activities = {
  list: () => request.get<Activity[]>("/activities"),
  details: (id: string) => request.get<Activity>(`/activities/${id}`),
  create: (activity: Activity) => request.post<void>("/activities", activity),
  update: (activity: Activity) =>
    request.put<void>(`/activities/${activity.id}`, activity),
  delete: (id: string) => request.del<void>(`/activities/${id}`),
};

const agent = {
  Activities,
};

export default agent;
