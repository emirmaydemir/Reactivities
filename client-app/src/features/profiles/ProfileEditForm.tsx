import { Form, Formik } from "formik";
import { observer } from "mobx-react-lite";
import { Button } from "semantic-ui-react";
import MyTextArea from "../../app/common/form/MyTextArea";
import MyTextInput from "../../app/common/form/MyTextInput";
import { useStore } from "../../app/stores/store";
import * as Yup from "yup";

interface Props {
  setEditMode: (editMode: boolean) => void;
}

export default observer(function ProfileEditForm({ setEditMode }: Props) {
  const {
    profileStore: { profile, updateProfile },
  } = useStore();
  return (
    <Formik
      //Profil düzenleme sekmesi açıldığında kullanıcı bilgileri varsa formun doldurulmasını sağladık. MyTextInput ve MyTextArea name özelliğine keyi verirken birebir aynı ismi vermeliyiz algılaması için.
      initialValues={{
        displayName: profile?.displayName,
        bio: profile?.bio || "",
      }}
      //Eğer butona basılırsa bio ve displayName keylerini verdiğimiz alanlardaki değerler alınacak ve updateProfile formundaki Profile ile eşleşecek o yüzden orayı Partial<Profile> yapmıştık sadece 2 alan gönderiyoruz.
      onSubmit={(values) => {
        updateProfile(values).then(() => {
          setEditMode(false);
        });
      }}
      //Displaynameyi zorunlu kılmasını söylüyoruz burada eğer girmezse hata mesajı çıkaracağız. bio ise opsiyonel.
      validationSchema={Yup.object({
        displayName: Yup.string().required(),
      })}
    >
      {({ isSubmitting, isValid, dirty }) => (
        <Form className="ui form">
          <MyTextInput placeholder="Display Name" name="displayName" />
          <MyTextArea rows={3} placeholder="Add your bio" name="bio" />
          <Button
            positive
            type="submit"
            loading={isSubmitting}
            content="Update profile"
            floated="right"
            disabled={!isValid || !dirty}
          />
        </Form>
      )}
    </Formik>
  );
});
