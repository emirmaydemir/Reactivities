import { useEffect, useState } from "react";
import { Container } from "semantic-ui-react";
import { Activity } from "../models/activity";
import NavBar from "./NavBar";
import ActivityDashboard from "../../features/activities/dashboard/ActivityDashboard";
import { v4 as uuid } from "uuid";
import agent from "../api/agent";
import LoadingComponent from "./LoadingComponent";

function App() {
  const [activities, setActivities] = useState<Activity[]>([]);
  const [selectedActivity, setSelectedActivity] = useState<
    Activity | undefined
  >(undefined);

  const [editMode, setEditMode] = useState(false);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    agent.Activities.list().then((response) => {
      let activities: Activity[] = [];
      response.forEach((activity) => {
        activity.date = activity.date.split("T")[0];
        activities.push(activity);
      });
      setActivities(activities);
      setLoading(false);
    });
  }, []);

  function handleSelectActivity(id: string) {
    setSelectedActivity(activities.find((x) => x.id == id));
  }

  function handleCancelSelectActivity() {
    setSelectedActivity(undefined);
  }

  function handleFormOpen(id?: string) {
    id ? handleSelectActivity(id) : handleCancelSelectActivity();
    setEditMode(true);
  }

  function handleFormClose() {
    setEditMode(false);
  }

  //Bu fonksiyonda activity id mevcut mu diye bakıyoruz yani activity varsa güncellemek için yoksa eklemek için istek gelmiştir demektir.
  //Sonrasında filter fonksiyonu ile elimizde bulunan activity dışında olan diziyi getirip güncellenmiş activityi içerisine ekliyoruz
  //Bu sayede eski activity silinip yerine güncellenmiş hali geliyor. ((x) => x.id !== activity.id), activity) bu kısım ona yarıyor yani
  //Ayrıca ...activities diyerek tüm aktiviteleri getiriyor sonuna da düzenlenen aktiviteyi ekliyor virgülden sonra koyduğumuz.
  function handleCreateOrEditActivity(activity: Activity) {
    setSubmitting(true); // Aktivite oluştururken veya güncellerken yükleme çubuğu gözükmesi ve kapanması için kullanıyoruz bu değişkeni.

    //Eğer idsi varsa var olan bir aktivitedir yani güncelleme yapacağız eğer id bilgisi yoksa yeni bir aktivite oluşturacağız demektir. 1 saniye api gecikmesinden sonra geri false olacak
    if (activity.id) {
      agent.Activities.update(activity).then(() => {
        setActivities([
          ...activities.filter((x) => x.id !== activity.id),
          activity,
        ]);
        setSelectedActivity(activity);
        setEditMode(false);
        setSubmitting(false);
      });
    } else {
      activity.id = uuid();
      agent.Activities.create(activity).then(() => {
        setActivities([...activities, activity]); //..activity diyerek activitynin tüm özelliklerini ele aldık id, description vs vs
        setSelectedActivity(activity);
        setEditMode(false);
        setSubmitting(false);
      });
    }
  }

  function handleDeleteActivity(id: string) {
    setSubmitting(true);
    agent.Activities.delete(id).then(() => {
      setActivities([...activities.filter((x) => x.id !== id)]);
      setSubmitting(false);
    });
  }

  if (loading) return <LoadingComponent content="Loading App" />;

  return (
    <>
      <NavBar openForm={handleFormOpen} />
      <Container style={{ marginTop: "7em" }}>
        <ActivityDashboard
          activities={activities}
          selectedActivity={selectedActivity}
          selectActivity={handleSelectActivity}
          cancelSelectActivity={handleCancelSelectActivity}
          editMode={editMode}
          openForm={handleFormOpen}
          closeForm={handleFormClose}
          createOrEdit={handleCreateOrEditActivity}
          deleteActivity={handleDeleteActivity}
          submitting={submitting}
        />
      </Container>
    </>
  );
}

export default App;
