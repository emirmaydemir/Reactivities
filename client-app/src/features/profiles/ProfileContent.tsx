import { Tab } from "semantic-ui-react";
import ProfilePhotos from "./ProfilePhotos";
import { Profile } from "../../app/models/profile";
import { observer } from "mobx-react-lite";
import ProfileAbout from "./ProfileAbout";
import ProfileFollowings from "./ProfileFollowings";
import { useStore } from "../../app/stores/store";
/*Tab Bileşeni: semantic-ui-react kütüphanesinden gelen bir bileşendir. Bu bileşen, sekmeli arayüzler oluşturmak için kullanılır. panes adlı bir dizi alır ve her öğe bir sekmeyi temsil eder. Her sekmenin menüde görünen bir başlığı (menuItem) ve tıklanınca görüntülenecek içeriği (render) vardır.

fluid: menu prop'una verilen fluid: true, menünün tam genişlikte olmasını sağlar. Yani, menu prop'u içinde belirtilen menünün, içinde bulunduğu kapsayıcının tamamını kaplamasını sağlar. Bu, özellikle dikey menülerde, menünün kapsayıcının tam yüksekliğini kaplaması anlamına gelir.

menuPosition: menuPosition="right" ise sekmelerin sağda görüntüleneceğini belirtir. Eğer left olarak ayarlarsanız, menü sol tarafa taşınır. */

interface Props {
  profile: Profile;
}

export default observer(function ProfileContent({ profile }: Props) {
  const { profileStore } = useStore();

  const panes = [
    { menuItem: "About", render: () => <ProfileAbout /> },
    { menuItem: "Photos", render: () => <ProfilePhotos profile={profile} /> },
    { menuItem: "Events", render: () => <Tab.Pane>Events Content</Tab.Pane> },
    { menuItem: "Followers", render: () => <ProfileFollowings /> },
    { menuItem: "Following", render: () => <ProfileFollowings /> },
  ];

  return (
    <Tab
      menu={{ fluid: true, vertical: true }}
      menuPosition="right"
      panes={panes}
      //Burada hangi sekmeye tıkladıysak onun indeksini profileStoreye gönderiyoruz bu sayede takipçilere bastıysak 3 numaralı index takip edilenlere bastıysak 4 numaralı index döndürülüyor. Bunu profileStorede ayıklayıp hangi listeyi dönmemiz gerektiğini anlıyoruz.
      onTabChange={(_e, data) => profileStore.setActiveTab(data.activeIndex)}
    />
  );
});
