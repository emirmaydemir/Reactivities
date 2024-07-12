import { Grid, GridColumn } from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";
import { useEffect } from "react";
import ActivityDetailedHeader from "./ActivityDetailedHeader";
import ActivityDetailedInfo from "./ActivityDetailedInfo";
import ActivityDetailedChat from "./ActivityDetailedChat";
import ActivityDetailedSidebar from "./ActivityDetailedSidebar";

export default observer(function ActivityDetails() {
  const { activityStore } = useStore();
  const {
    selectedActivity: activity,
    loadActivity,
    loadingInitial,
    clearSelectedActivity,
  } = activityStore; //storede yer alan selectedActivitye activity ismini verdim daha rahat kullanım için
  const { id } = useParams();

  //Şimdi use effect ekrana ilk girdiğimizde çalışıyor return kısmı ise ekrandan çıktığımızda çalışacak yani ekrandan çıkınca seçilen aktiviteyi bellekten sileceğiz.
  useEffect(() => {
    if (id) loadActivity(id);
    return () => clearSelectedActivity();
  }, [id, loadActivity, clearSelectedActivity]);

  if (loadingInitial || !activity)
    return <LoadingComponent content="Loading App" />;

  return (
    <Grid>
      <GridColumn width={10}>
        <ActivityDetailedHeader activity={activity} />
        <ActivityDetailedInfo activity={activity} />
        <ActivityDetailedChat activityId={activity.id} />
      </GridColumn>
      <GridColumn width={6}>
        <ActivityDetailedSidebar activity={activity} />
      </GridColumn>
    </Grid>
  );
});
