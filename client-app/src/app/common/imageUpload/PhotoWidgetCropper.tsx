import Cropper from "react-cropper";
import "cropperjs/dist/cropper.css";

interface Props {
  imagePreview: string; //Kırpılacak görüntünü
  setCropper: (cropper: Cropper) => void; //Kırpılmış görüntüye ait bilgileri içeren cropper nesnesi
}

//Görüntüyü kırptığımız widget
export default function PhotoWidgetCropper({
  imagePreview,
  setCropper,
}: Props) {
  return (
    <Cropper
      src={imagePreview} //Kırpılacak görüntünün kaynağını belirler
      style={{ height: 200, width: "100%" }}
      initialAspectRatio={1} //Başlangıç en boy oranını ayarlar (burada 1, yani kare).
      aspectRatio={1} //Kırpma çerçevesinin en boy oranını ayarlar (burada 1, yani kare).
      preview=".img-preview" //Kırpılan görüntünün önizlemesinin nerede gösterileceğini belirler (CSS seçici kullanarak).
      guides={false} // Kılavuz çizgilerini gizler veya gösterir (burada false, yani gizli).
      viewMode={1} //Görüntünün nasıl görüntüleneceğini belirler (burada 1, yani kısıtlı görünüm modu).
      autoCropArea={1} //Otomatik kırpma alanını ayarlar (burada 1, yani tam görüntü).
      background={false}
      onInitialized={(cropper) => setCropper(cropper)} //Kırpılan görüntüye ait bilgileri ayarlar
    />
  );
}
