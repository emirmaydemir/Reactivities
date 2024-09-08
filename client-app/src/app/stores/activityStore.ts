import { makeAutoObservable, runInAction } from "mobx";
import { Activity, ActivityFormValues } from "../models/activity";
import agent from "../api/agent";
import { v4 as uuid } from "uuid";
import { format } from "date-fns";
import { store } from "./store";
import { Profile } from "../models/profile";

export default class ActivityStore {
  activityRegistry = new Map<string, Activity>();
  selectedActivity: Activity | undefined = undefined;
  editMode = false;
  loading = false;
  loadingInitial = false;

  constructor() {
    makeAutoObservable(this);
  }

  //Aktiviteleri tarih bilgilerine göre sıralar.
  get activitiesByDate() {
    return Array.from(this.activityRegistry.values()).sort(
      (a, b) => a.date!.getTime() - b.date!.getTime()
    );
  }

  /*Adım Adım İşleyişi:
    1.Adım
    Başlangıçta: activities nesnesi boş {} olarak başlar.
    2.Adım
    İlk Eleman (activity1):
    activity1.date alınır (örneğin "2023-01-01").
    activities nesnesinde bu tarih yoksa, yeni bir liste oluşturulur ve activity1 bu listeye eklenir.
    activities nesnesi şu hale gelir:  
    {
      "2023-01-01": [activity1]
    }
    3.Adım
    İkinci Eleman (activity2):
    activity2.date alınır (örneğin "2023-01-01").
    activities nesnesinde bu tarih varsa, activity2 mevcut listenin sonuna eklenir.
    activities nesnesi şu hale gelir
    {
      "2023-01-01": [activity1, activity2]
    }
    4.Adım
    activity3.date alınır (örneğin "2023-01-02").
    activities nesnesinde bu tarih yoksa, yeni bir liste oluşturulur ve activity3 bu listeye eklenir.
    activities nesnesi şu hale gelir
    {
    "2023-01-01": [activity1, activity2],
    "2023-01-02": [activity3]
    }
    Bu işlem dizinin sonuna kadar devam eder ve tüm aktiviteler tarihlerine göre gruplandırılır.
  */
  get groupedActivities() {
    return Object.entries(
      //Bu fonksiyon dizi listesi tutar yani bir nesne var içerisinde bir sürü dizi oluşacak. Aynı tarihteki aktiviteler aynı dizide toplanacak.
      this.activitiesByDate.reduce((activities, activity) => {
        const date = format(activity.date!, "dd MMM yyyy"); // Mevcut aktivitenin tarihini alır. Ama split sayesinde sadece 23.08.2000 gibi bir tarih alır.
        activities[date] = activities[date]
          ? [...activities[date], activity] // Eğer 'activities' nesnesinde bu tarih için bir giriş varsa, mevcut aktiviteyi bu tarihe ait aktiviteler listesine ekler.
          : [activity]; // Eğer yoksa, yeni bir liste oluşturur ve aktiviteyi bu listeye ekler.
        return activities; // Güncellenmiş 'activities' nesnesini döndürür.
      }, {} as { [key: string]: Activity[] })
    );
  }

  loadActivities = async () => {
    this.setLoadingInitial(true);
    try {
      const activities = await agent.Activities.list();
      activities.forEach((activity) => {
        this.setActivity(activity);
      });
      this.setLoadingInitial(false);
    } catch (error) {
      console.log(error);
      this.setLoadingInitial(false);
    }
  };

  loadActivity = async (id: string) => {
    let activity = this.getActivity(id);
    if (activity) {
      this.selectedActivity = activity;
      return activity;
    } else {
      this.setLoadingInitial(true);
      try {
        activity = await agent.Activities.details(id);
        this.setActivity(activity);
        runInAction(() => (this.selectedActivity = activity));
        this.setLoadingInitial(false);
        return activity;
      } catch (error) {
        console.log(error);
        this.setLoadingInitial(false);
      }
    }
  };

  private setActivity = (activity: Activity) => {
    const user = store.userStore.user; //Şu anki mevcut kullanıcıyı alıyoruz istekte bulunan kullanıcıyı yani.
    //Yapacağımız ilk şey kullanıcı nesnesine sahip olup olmadığımızı kontrol etmek olacak.
    if (user) {
      //activity.attendees listesindeki her bir katılımcının userName özelliği, mevcut kullanıcının username özelliği ile karşılaştırılıyor. Eğer eşleşme bulunursa, activity.isGoing değeri true olur, aksi takdirde false olur.
      activity.isGoing = activity.attendees!.some(
        (a) => a.userName === user.username
      );
      //Etkinliğin hostUsername değeri, mevcut kullanıcının username değeri ile karşılaştırılıyor. Eğer eşleşme bulunursa, activity.isHost değeri true olur, aksi takdirde false olur.
      activity.isHost = activity.hostUsername === user.username;
      //activity.attendees listesindeki katılımcılar arasında userName değeri activity.hostUsername ile eşleşen katılımcı bulunur ve bu katılımcı activity.host değişkenine atanır.
      activity.host = activity.attendees?.find(
        (x) => x.userName === activity.hostUsername
      );
    }
    activity.date = new Date(activity.date!);
    this.activityRegistry.set(activity.id, activity);
  };

  private getActivity = (id: string) => {
    return this.activityRegistry.get(id);
  };

  setLoadingInitial = (state: boolean) => {
    this.loadingInitial = state;
  };

  createActivity = async (activity: ActivityFormValues) => {
    activity.id = uuid();
    const user = store.userStore.user; //şu anki mwvcut kullanıcı getiriliyor.
    const attendee = new Profile(user!); //katılımcılar değişkenine şu anki kullanıcı ekleniyor
    try {
      //Önce aktivite oluşturuluyor sonra aktivite hostu ve seçilen aktivite önbelleğimize alınıyor. Çünkü aktivitenin katılımcı bilgileri updateAttendance fonksiyonunda ayarlanıyor burada sadece aktiviteye ait bilgiler güncelleniyor.
      await agent.Activities.create(activity);
      const newActivity = new Activity(activity);
      newActivity.hostUsername = user!.username;
      newActivity.attendees = [attendee];
      this.setActivity(newActivity);
      runInAction(() => {
        this.selectedActivity = newActivity;
      });
    } catch (error) {
      console.log(error);
    }
  };

  updateActivity = async (activity: ActivityFormValues) => {
    try {
      await agent.Activities.update(activity);
      runInAction(() => {
        if (activity.id) {
          //Güncellenmiş aktivite değişkeni oluşturuyor bu satır değerleri eşleyerek güncel değeri değişkene atıyor.
          //Bu satır, mevcut bir aktivite nesnesini (getActivity(activity.id)) ve yeni aktivite değerlerini (activity) birleştirerek güncellenmiş bir aktivite nesnesi oluşturur.
          const updateActivity = {
            ...this.getActivity(activity.id),
            ...activity,
          };
          this.activityRegistry.set(activity.id, updateActivity as Activity);
          this.selectedActivity = updateActivity as Activity;
        }
      });
    } catch (error) {
      console.log(error);
    }
  };

  deleteActivity = async (id: string) => {
    this.loading = true;
    try {
      await agent.Activities.delete(id);
      runInAction(() => {
        this.activityRegistry.delete(id);
        this.loading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => {
        this.loading = false;
      });
    }
  };

  updateAttendance = async () => {
    const user = store.userStore.user;
    this.loading = true;
    try {
      await agent.Activities.attend(this.selectedActivity!.id);
      runInAction(() => {
        //2 Butonada updateAttendance fonksiyonunu verdiğimiz için kullanıcı isGoing ise aktiviteden çıkartıyoruz isGoing değil ise aktiviteye ekliyoruz.
        if (this.selectedActivity?.isGoing) {
          //şu anda oturum açmış olan kullanıcıyı katılımcılar dizisinden filtreleyecektir. Yani attendeesin içerisinden şu anda istek atan kullacınıyı çıkaracaktır. Bu da eğer kullanıcı isGoing yani katılımcı ise yapılıyor çünkü katılımcı ise aktiviteden çıkmak istiyor anlamına gelmektedir. Eğer katılımcı değil ise aktiviteye katılmak istiyor demektir. Bu durumda aktiviteden çıkarılacaktır.
          this.selectedActivity.attendees =
            this.selectedActivity.attendees?.filter(
              (a) => a.userName !== user?.username
            );
          this.selectedActivity.isGoing = false;
        } else {
          //Burada isGoing false ise aktiviteye katılmak istiyor demektir aktiviteye ekliyoruz kullanıcıyı.
          const attendee = new Profile(user!);
          this.selectedActivity?.attendees?.push(attendee);
          this.selectedActivity!.isGoing = true;
        }
        this.activityRegistry.set(
          this.selectedActivity!.id,
          this.selectedActivity!
        );
      });
    } catch (error) {
      console.log("error");
    } finally {
      runInAction(() => (this.loading = false));
    }
  };

  cancelActivityToggle = async () => {
    this.loading = true; // Aktivite iptal edilirken geçici bir bekleme süresi imajı vermek için.
    try {
      //UpdateAttendance methodunda isteği atan kişi host ise aktivite iptal edilir katılımcı ise aktiviteden çıkarılır hiçbiri ise aktiviteye katılımcı olarak eklenir. Bu yüzden sadece aktivite id bilgisini iletmemiz yeterli geri kalanı application kısmı yapıyor.
      await agent.Activities.attend(this.selectedActivity!.id);
      runInAction(() => {
        //runInAction mobx değişken değişikliklerinin algılanması için kullanılır.
        //Burada aktivite iptali ne durumdaysa tersine çeviriyoruz mesela aktifti kullanıcı basınca pasif oldu. Pasifti kullanıcı basınca aktif olacak.
        this.selectedActivity!.isCancelled =
          !this.selectedActivity?.isCancelled;
        //İşlem sonrası seçilen aktivitemiz ayarlandı.
        this.activityRegistry.set(
          this.selectedActivity!.id,
          this.selectedActivity!
        );
      });
    } catch (error) {
      console.log(error);
    } finally {
      runInAction(() => (this.loading = false));
    }
  };

  clearSelectedActivity = () => {
    this.selectedActivity = undefined;
  };

  ///////////////////////////////////////////////////////////////////////////////////////////
  //setImage ve setDisplayName TAMAMEN BEN YAZDIM
  //Burası sadece profileStore içerisindeki setMainPhoto fonksiyonu tetiklenince çalışacaktır.
  //Kullanıcı profileStore kısmında fotoğrafını güncellerse o güncellemenin aktivitelerede yansıması gerekiyor o yüzden burayıda güncelledik mobx tarafında. Aslında veritabanına kaydettiği için yansıyor bu kod olmadan ama kullanıcı sayfayı yenilemeden göremiyeceği için mobx üzerinde de güncelledik.
  setImage(image: string) {
    const username = store.userStore.user?.username; //Sistemdeki mevcut kullanıcıyı çekiyoruz.
    this.activityRegistry.forEach((activity) => {
      //Aktivitelerin listesini dönüyoruz ve host olunan aktivite ile sistemdeki kullanıcı aynı kişi ise fotoğrafı güncelliyoruz. Host kontrolü yapılmasının sebebi sadece fotosunu değiştiren kişinin aktivite fotosu değişmeli yoksa herkesin ki değişir.
      if (activity.hostUsername === username) activity.host!.image = image;
      // Attendees listesindeki hostun resmini güncelliyoruz. Mevcut kullanıcının ismini katılımcılarda aratıp o aktivitede ismi varsa güncelliyoruz.
      activity.attendees!.forEach((attendee) => {
        if (attendee.userName === username) {
          attendee.image = image;
        }
      });
    });
  }

  setDisplayName = (name: string) => {
    const username = store.userStore.user?.username; //Sistemdeki mevcut kullanıcıyı çekiyoruz.
    this.activityRegistry.forEach((activity) => {
      //Aktivitelerin listesini dönüyoruz ve host olunan aktivite ile sistemdeki kullanıcı aynı kişi ise adı güncelliyoruz. Host kontrolü yapılmasının sebebi sadece adı değiştiren kişinin aktivite adı değişmeli yoksa herkesin ki değişir.
      if (activity.hostUsername === username) activity.host!.displayName = name;
      // Attendees listesindeki hostun adını güncelliyoruz. Mevcut kullanıcının ismini katılımcılarda aratıp o aktivitede ismi varsa güncelliyoruz.
      activity.attendees!.forEach((attendee) => {
        if (attendee.userName === username) {
          attendee.displayName = name;
        }
      });
    });
  };
  ///////////////////////////////////////////////////////////////////////////////////////////

  //Burası react tarafındaki aktiviteler yani burada yapılan değişiklik db tarafında değil front-end tarafında yapılıyor. ama apiye istek atsaydık db de etkilenirdi.
  //db değiştirme işlemini profileStore.ts içerisinde updateFollowing methodu yapıyor zaten burayıda çağırıyor.
  //Bir insanı takip edince veya takipten çıkınca tüm aktivitelerde o kişiyi bulup front-endi güncellememiz gerekiyordu.
  //Biz de o yüzden tüm aktivitilerin içerisindeki katılımcılara sırayla baktık ve bizim aradığımız katılımcı var ise işlem yaptık.
  //Takip ettiğimiz veya takipten çıktığımız insan aktivitenin katılımcısı ise takip ediyorsak takipçisini 1 arttırdık takipten çıktıysak 1 azalttık ve takip edip etmeme durumumuzu güncelledik.
  updateAttendeeFollowing = (username: string) => {
    this.activityRegistry.forEach((activity) => {
      activity.attendees.forEach((attendee: Profile) => {
        if (attendee.userName === username) {
          attendee.following
            ? attendee.followersCount--
            : attendee.followersCount++;
          attendee.following = !attendee.following;
        }
      });
    });
  };
}
