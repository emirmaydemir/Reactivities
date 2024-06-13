import { makeAutoObservable, runInAction } from "mobx";
import { Activity } from "../models/activity";
import agent from "../api/agent";
import { v4 as uuid } from "uuid";
import { format } from "date-fns";

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
    activity.date = new Date(activity.date!);
    this.activityRegistry.set(activity.id, activity);
  };

  private getActivity = (id: string) => {
    return this.activityRegistry.get(id);
  };

  setLoadingInitial = (state: boolean) => {
    this.loadingInitial = state;
  };

  createActivity = async (activity: Activity) => {
    this.loading = true;
    activity.id = uuid();
    try {
      await agent.Activities.create(activity);
      runInAction(() => {
        this.activityRegistry.set(activity.id, activity);
        this.selectedActivity = activity;
        this.editMode = false;
        this.loading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => {
        this.loading = false;
      });
    }
  };

  updateActivity = async (activity: Activity) => {
    this.loading = true;
    try {
      await agent.Activities.update(activity);
      runInAction(() => {
        this.activityRegistry.set(activity.id, activity);
        this.selectedActivity = activity;
        this.editMode = false;
        this.loading = false;
      });
    } catch (error) {
      console.log(error);
      runInAction(() => {
        this.loading = false;
      });
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
}
