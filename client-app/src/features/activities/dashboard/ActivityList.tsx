import { Header } from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";
import { observer } from "mobx-react-lite";
import ActivityListItem from "./ActivityListItem";
import { Fragment } from "react";

export default observer(function ActivityList() {
  const { activityStore } = useStore();
  const { groupedActivities } = activityStore;

  return (
    /*Activity storeden gruplar halinde diziler gelmişti. Tarihlerine göre key almışlardı işte gruplar halinde ekranda gösterilmesini sağlayan yapı burada kuruldu. */
    /*Fragment ile gruplarına göre getirdik bu sayede aynı grupta olanlar aynı tarihin altında bitişik olarak geldi. */
    <>
      {groupedActivities.map(([group, activities]) => (
        <Fragment key={group}>
          <Header sub color="teal">
            {group}
          </Header>
          {activities.map((activity) => (
            <ActivityListItem key={activity.id} activity={activity} />
          ))}
        </Fragment>
      ))}
    </>
  );
});
