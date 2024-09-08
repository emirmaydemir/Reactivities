import { observer } from "mobx-react-lite";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { Grid } from "semantic-ui-react";
import LoadingComponent from "../../app/layout/LoadingComponent";
import { useStore } from "../../app/stores/store";
import ProfileContent from "./ProfileContent";
import ProfileHeader from "./ProfileHeader";

export default observer(function ProfilePage() {
  /*useParams kancası (hook), React Router tarafından sağlanan bir kancadır ve URL'deki parametreleri almanızı sağlar.
  Örneğin, URL şu şekildeyse: http://localhost:3000/profile/johndoe
  ve React Router yapılandırmanız şu şekildeyse: <Route path="/profile/:username" element={<Profile />} />
  oradaki username bilgisini almayı sağlar.
  */
  const { username } = useParams<{ username: string }>();
  const { profileStore } = useStore();
  const { loadingProfile, loadProfile, profile, setActiveTab } = profileStore;

  /*Burada her profil yüklenmesinde veya kullanıcı adı değişikliğinde useEffect sorgusu ile yeni kullanıcı profilinin oluşmasını sağlıyoruz useEffect bir değişiklik olduğunda api çağrısı yapabilmek için kullanılır. */
  useEffect(() => {
    if (username) loadProfile(username);
    return () => {
      setActiveTab(0); //sekme değiştiğinde takipçi veya takip edilenlerin kalıntıları kalmasın diye ilgili sekmeleri sıfırlıyoruz. profilStore içerisine setActivetab değerini sıfırlayarak çözüyoruz durumu.
    };
  }, [loadProfile, username, setActiveTab]);

  if (loadingProfile)
    //apiye istek atılırsa ve loadingprofile profileStore içerisinde true yapılırsa apide verdiğimiz 1-2 saniyelik gecikme süresi boyunca ekranda Loading profile... yazmasını sağlıyoruz.
    return <LoadingComponent inverted content="Loading profile..." />;

  if (!profile) return <h2>Problem loading profile</h2>;

  return (
    <Grid>
      <Grid.Column width={16}>
        <ProfileHeader profile={profile} />
        <ProfileContent profile={profile} />
      </Grid.Column>
    </Grid>
  );
});
