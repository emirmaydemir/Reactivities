import { SyntheticEvent } from "react";
import { observer } from "mobx-react-lite";
import { Button, Reveal } from "semantic-ui-react";
import { useStore } from "../../app/stores/store";
import { Profile } from "../../app/models/profile";

interface Props {
  profile: Profile;
}

export default observer(function FollowButton({ profile }: Props) {
  const { profileStore, userStore } = useStore();
  const { updateFollowing, loading } = profileStore;

  if (userStore.user?.username === profile.userName) return null;

  function handleFollow(e: SyntheticEvent, username: string) {
    //Şimdi biz tıklama olayını kontrol ediyoruz. Profile ekranından tıklarsak bir sorun yok ancak
    e.preventDefault(); //Ancak ProfileCard ekranında takip butonuna tıklarsak hem takip edip hem de kullanıcının profiline gidecektir. Biz bunu istemiyoruz sadece takip etsin ve Profile gitmeden aynı ekranda kalsın istiyoruz. preventDefault() kullanarak bu varsayılan davranışı engelliyoruz. Bu sayede istediğimiz oluyor.
    profile.following
      ? updateFollowing(username, false)
      : updateFollowing(username, true);
  }

  return (
    <Reveal animated="move">
      <Reveal.Content visible style={{ width: "100%" }}>
        <Button
          fluid
          color="teal"
          content={profile.following ? "Following" : "Not Following"}
        />
      </Reveal.Content>
      <Reveal.Content hidden>
        <Button
          loading={loading}
          fluid
          basic
          color={profile.following ? "red" : "green"}
          content={profile.following ? "Unfollow" : "Follow"}
          onClick={(e) => handleFollow(e, profile.userName)}
        />
      </Reveal.Content>
    </Reveal>
  );
});
