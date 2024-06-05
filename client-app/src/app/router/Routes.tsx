import { RouteObject, createBrowserRouter } from "react-router-dom";
import App from "../layout/App";
import HomePage from "../../features/home/HomePage";
import ActivityDashboard from "../../features/activities/dashboard/ActivityDashboard";
import ActivityForm from "../../features/activities/form/ActivityForm";
import ActivityDetails from "../../features/activities/details/ActivityDetails";

export const routes: RouteObject[] = [
  {
    path: "/",
    element: <App />,
    children: [
      { path: "activities", element: <ActivityDashboard /> },
      { path: "activities/:id", element: <ActivityDetails /> },
      { path: "createActivity", element: <ActivityForm key="create" /> }, // ve bu key parametresi sayesinde edite basınca içi dolu form create activitye basınca içi boş form gelecek. Tamamen react anlasın diye yaptık yani.
      { path: "manage/:id", element: <ActivityForm key="manage" /> }, //2 tane ActivityForm nesnemiz olduğu için react bunları birbirinden ayırt etsin diye key değişkenini kullanıyoruz.
    ],
  },
];

export const router = createBrowserRouter(routes);
