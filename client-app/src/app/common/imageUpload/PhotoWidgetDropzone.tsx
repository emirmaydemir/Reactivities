import { useCallback } from "react";
import { useDropzone } from "react-dropzone";
import { Header, Icon } from "semantic-ui-react";

interface Props {
  setFiles: (files: any) => void; //Seçilen dosyaları bu değişkenin içerisine koyacağız.
}

//Dropzone kütüphanesini indirdik bu kütüphanenin sağladığı fonksiyonlar bizim belirli bir alana dosya bırakmamızı ve onu yükleyebilmemizi sağlıyor.
export default function PhotoWidgetDropzone({ setFiles }: Props) {
  const dzStyles = {
    border: "dashed 3px #eee",
    borderColor: "#eee",
    borderRadius: "5px",
    paddingTop: "30px",
    textAlign: "center" as "center",
    height: "200px",
  };

  //Sürükleyerek resmi bırakırken ilgili alana gelince imleç etraf yeşil yanacak.
  const dzActive = {
    borderColor: "green",
  };

  //Dropzone içerisine resim yerleştirdiğimizde 1 veya daha gazla bu fonksiyon çalışacaktır. Seçilen dosyaları setFiles değişkeninin içerisine koyacağız. Sonra bunların url bilgilerini dizi halinde ayarlıyoruz.
  const onDrop = useCallback(
    (acceptedFiles: any) => {
      setFiles(
        acceptedFiles.map((file: any) =>
          Object.assign(file, {
            preview: URL.createObjectURL(file),
          })
        )
      );
    },
    [setFiles]
  );
  const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop });

  return (
    /*getRootProps ve getInputProps -- Bunları dropzone ayarlamış düşünmene gerek yok sürükle bırak foto yükle işlemleri falan için. Zaten bu hazır kütüphane dropzone */
    <div
      {...getRootProps()}
      style={isDragActive ? { ...dzStyles, ...dzActive } : dzStyles} ///Sürükleyerek resmi bırakırken ilgili alana gelince imleç etraf yeşil yanacak. Onun harici klasik dizayn olacak. Yani sadece dzStyles olacak.
    >
      <input {...getInputProps()} /> <Icon name="upload" size="huge" />
      <Header content="Drop image here" />
    </div>
  );
}
