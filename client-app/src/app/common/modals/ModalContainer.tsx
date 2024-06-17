import { observer } from "mobx-react-lite";
import { Modal } from "semantic-ui-react";
import { useStore } from "../../stores/store";

export default observer(function ModalContainer() {
  const { modalStore } = useStore();
  //Ekranın ortasında açılan küçük bir kutu sağlıyor bu modal bize içerisinde mail ve şifre isteyen kutucuklarımız var LoginFormdan gelen.
  return (
    <Modal
      open={modalStore.modal.open} // kutu açılma değişkeni true olunca direkt sayfanın ortasında belirir bunu açmak için homepagedeki logine basmamız gerekiyor.
      onClose={modalStore.closeModal} //kutu dışında bir yere basılınca closemodal fonksiyonunu çağırıp giriş kutusunun kapanmasını sağlayan kod burası.
      size="mini"
    >
      <Modal.Content>{modalStore.modal.body}</Modal.Content>
    </Modal>
  );
});
