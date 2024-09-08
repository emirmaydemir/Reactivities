import { Profile } from "./profile";

export interface Activity {
  id: string;
  title: string;
  date: Date | null;
  description: string;
  category: string;
  city: string;
  venue: string;
  hostUsername: string;
  isCancelled: boolean;
  isGoing: boolean; //Katılımcı mı
  isHost: boolean; //Etkinliği oluşturan kişi mi
  host?: Profile;
  attendees: Profile[];
}

//Etkinlik oluştururken kullandığımız alan için yapıyoruz bunu. ! koymak zorunda kalmıyoruz bu sayede. Her yere ünlem koya koya kodlar karışmıştı. Ve başlangıç değeri atamış olduk.
//Bunu oluşturma sebebimiz değerler yoksa null olarak uygulamada hata vermesin biz boş olarak başlatıyoruz en kötü durumda.
export class ActivityFormValues {
  id?: string = undefined;
  title: string = "";
  category: string = "";
  description: string = "";
  date: Date | null = null;
  city: string = "";
  venue: string = "";

  constructor(activity?: ActivityFormValues) {
    if (activity) {
      this.id = activity.id;
      this.title = activity.title;
      this.category = activity.category;
      this.description = activity.description;
      this.date = activity.date;
      this.venue = activity.venue;
      this.city = activity.city;
    }
  }
}

export class Activity implements Activity {
  constructor(init?: ActivityFormValues) {
    Object.assign(this, init);
  }
}
