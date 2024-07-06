import { observer } from "mobx-react-lite";
import { Card, Header, Tab, Image, Grid, Button } from "semantic-ui-react";
import { Photo, Profile } from "../../app/models/profile";
import { useStore } from "../../app/stores/store";
import { SyntheticEvent, useState } from "react";
import PhotoUploadWidget from "../../app/common/imageUpload/PhotoUploadWidget";

interface Props {
  profile: Profile;
}

export default observer(function ProfilePhotos({ profile }: Props) {
  const {
    profileStore: {
      isCurrentUser,
      uploadPhoto,
      uploading,
      loading,
      setMainPhoto,
      deletePhoto,
    },
  } = useStore();
  const [addPhotoMode, setAddPhotoMode] = useState(false);
  const [target, setTarget] = useState("");

  function handlePhotoUpload(file: Blob) {
    uploadPhoto(file).then(() => setAddPhotoMode(false));
  }

  function handleSetMainPhoto(
    photo: Photo,
    e: SyntheticEvent<HTMLButtonElement>
  ) {
    setTarget(e.currentTarget.name); //e sayesinde hangı fotoya tıklandığımı takip ediyoruz.
    setMainPhoto(photo); //fotoyu main yapıyoruz.
  }

  function handleDeletePhoto(
    photo: Photo,
    e: SyntheticEvent<HTMLButtonElement>
  ) {
    setTarget(e.currentTarget.name); //e sayesinde hangı fotoya tıklandığımı takip ediyoruz.
    deletePhoto(photo); //Mobx içerisinde yazdığımız deletePhoto çağırıp fotoyu siliyoruz.
  }

  return (
    <Tab.Pane>
      <Grid>
        <Grid.Column width={16}>
          <Header floated="left" icon="image" content="Photos" />
          {isCurrentUser && (
            <Button
              floated="right"
              basic
              content={addPhotoMode ? "Cancel" : "Add Photo"}
              onClick={() => setAddPhotoMode(!addPhotoMode)}
            />
          )}
        </Grid.Column>
        <Grid.Column width={16}>
          {addPhotoMode ? (
            <PhotoUploadWidget
              uploadPhoto={handlePhotoUpload}
              loading={uploading}
            />
          ) : (
            <Card.Group itemsPerRow={5}>
              {profile.photos?.map((photo) => (
                <Card key={photo.id}>
                  <Image src={photo.url} />
                  {isCurrentUser && (
                    <Button.Group fluid widths={2}>
                      <Button
                        basic
                        color="green"
                        content="Main" //name özelliğinin açıklaması aşağıdaki buttonda var.
                        name={"main" + photo.id} //main + photo id yapmamızın sebebi aşağıda da photo.id ile kimlik verdik butonlara bunlar çakışacağı için hangisine bastığımızı anlayamayacak ve kargaşa oluşacaktı bunun önüne geçmek için bu buton için main + id şeklinde bir kimliklendirme yaptık.
                        loading={target === "main" + photo.id && loading} //target === photo.id: Bu kısım, hangi fotoya yükleme animasyonu vereceğimizi belirler.
                        disabled={photo.isMain} //Foto main ise main fotoyu tekrar main yapmamak için devre dışı bırakıyoruz.
                        onClick={(e) => handleSetMainPhoto(photo, e)} //e sayesinde hangı fotoya tıklandığımı takip ediyoruz.
                      />
                      <Button
                        basic
                        color="red"
                        icon="trash"
                        loading={target === photo.id && loading} //target === photo.id: Bu kısım, hangi fotoya yükleme animasyonu vereceğimizi belirler.
                        onClick={(e) => handleDeletePhoto(photo, e)} //e sayesinde hangı fotoya tıklandığımı takip ediyoruz.
                        disabled={photo.isMain} //Foto main ise main fotoyu silememesi için devre dışı bırakıyoruz.
                        name={photo.id} //name özelliği, her butona benzersiz bir kimlik (ID) vermek için kullanılıyor. Bu örnekte, photo.id değeri butonun name özelliğine atanıyor. name özelliği, butona benzersiz bir tanımlayıcı vermek için kullanılabilir ve bu sayede, belirli bir butona tıklanıp tıklanmadığını takip edebilirsiniz.
                      />
                    </Button.Group>
                  )}
                </Card>
              ))}
            </Card.Group>
          )}
        </Grid.Column>
      </Grid>
    </Tab.Pane>
  );
});
