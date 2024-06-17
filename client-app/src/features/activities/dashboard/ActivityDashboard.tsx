import { Grid } from "semantic-ui-react";
import ActivityList from "./ActivityList";
import { useStore } from "../../../app/stores/store";
import { observer } from "mobx-react-lite";
import { useEffect } from "react";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import ActivityFilters from "./ActivityFilters";

export default observer(function ActivityDashboard() {
  const { activityStore } = useStore();
  const { loadActivities, activityRegistry } = activityStore;

  useEffect(() => {
    if (activityRegistry.size <= 1) loadActivities(); // bu sayede aktivitelerimiz bir kere yüklendiğinde hafızadan çekiyoruz artık. sürekli yeniden yükleyip yormuyoruz. sürekli api isteğinde bulunmamış oluyoruz web sitesini yenilemedikçe sayfalar arası geçişte aktivitelerimiz ekranda olacak zaten.
  }, [activityRegistry.size]); // <= 1 dememizin sebebi 1 aktivite seçip sayfayı yenilersek dizimizde tek aktivite kalır ve api isteği atmadığı için tek aktivite gözükür ekranda ama bu çözüm sayesinde bu sorundan kurtulduk.

  if (activityStore.loadingInitial)
    return <LoadingComponent content="Loading activities..." />;

  return (
    <Grid>
      <Grid.Column width="10">
        <ActivityList />
      </Grid.Column>
      <Grid.Column width="6">
        <ActivityFilters />
      </Grid.Column>
    </Grid>
  );
});
