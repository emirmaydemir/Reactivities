import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "semantic-ui-css/semantic.min.css";
import "./index.css";

//createRoot ile  fonksiyonu, React uygulamasının başlatılması için bir kök (root) oluşturur. Bu root index.html de bulunan dive verilmiştir.
// render içerisinde ise html dönecek react elementini belirtiriz.
//Sonuç olarak, bu kod parçası, root adlı HTML elementine React uygulamasını render etmek için ReactDOM.createRoot ve render fonksiyonlarını kullanır.
ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
