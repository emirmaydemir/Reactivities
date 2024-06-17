import { Container } from "semantic-ui-react";
import NavBar from "./NavBar";
import { observer } from "mobx-react-lite";
import { Outlet, useLocation } from "react-router-dom";
import HomePage from "../../features/home/HomePage";
import { ToastContainer } from "react-toastify";
import { useStore } from "../stores/store";
import { useEffect } from "react";
import LoadingComponent from "./LoadingComponent";
import ModalContainer from "../common/modals/ModalContainer";

function App() {
  const location = useLocation(); // bu bize url yolunu yani hangi sayfada olduğumuzu söyleyecektir.
  const { commonStore, userStore } = useStore();

  //Özetle, bu kod parçası, uygulamanın yüklenme sürecini yönetir. commonStore.token mevcutsa, kullanıcı verileri yüklenir ve yükleme tamamlandığında commonStore.setAppLoaded() çağrılarak uygulamanın yüklendiği bildirilir. Token yoksa, doğrudan uygulamanın yüklendiği bildirilir. Yükleme tamamlanana kadar kullanıcıya bir yükleme bileşeni gösterilir.
  useEffect(() => {
    if (commonStore.token) {
      userStore.getUser().finally(() => commonStore.setAppLoaded());
    } else {
      commonStore.setAppLoaded();
    }
  }, [commonStore, userStore]);

  if (!commonStore.appLoaded)
    return <LoadingComponent content="Loading app..." />;

  return (
    <>
      <ModalContainer />
      {/* Hata yaparlarsa uyarabilmek için kullandığımız bileşenin nerede ve ne renk olacağı bilgileri. */}
      <ToastContainer position="bottom-right" hideProgressBar theme="colored" />
      {/*Burada eğer url adresimiz '/' ise yani ana ekransa homepageyi çağır değilse normal aktivite ekranlarım outlet ile işlensin diyorum yani ana sayfayı ayırdım diğer yapıdan aynı zamanda routes.tsx içerisinde parametre olarak eklememe gerek kalmadı. */}
      {location.pathname === "/" ? (
        <HomePage />
      ) : (
        <>
          <NavBar />
          <Container style={{ marginTop: "7em" }}>
            {/*<Outlet />, şu anki rota hangi bileşeni gerektiriyorsa onu render eder. Main.tsx dosyasında bunun yolu verildi ve Routes.tsx dosyasında ayarı yapıldı bunun üstündeki bileşenler sürekli gözükecek ama burada sadece çağırılan sayfalar render edilecek */}
            <Outlet />
          </Container>
        </>
      )}
    </>
  );
}

export default observer(App);
