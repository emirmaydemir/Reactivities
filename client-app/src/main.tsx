import React from "react";
import ReactDOM from "react-dom/client";
import "semantic-ui-css/semantic.min.css";
import "./app/layout/styles.css";
import "react-calendar/dist/Calendar.css";
import { StoreContext, store } from "./app/stores/store";
import { RouterProvider } from "react-router-dom";
import { router } from "./app/router/Routes";

//createRoot ile  fonksiyonu, React uygulamasının başlatılması için bir kök (root) oluşturur. Bu root index.html de bulunan dive verilmiştir.
// render içerisinde ise html dönecek react elementini belirtiriz.
//Sonuç olarak, bu kod parçası, root adlı HTML elementine React uygulamasını render etmek için ReactDOM.createRoot ve render fonksiyonlarını kullanır.
ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <StoreContext.Provider value={store}>
      <RouterProvider router={router}></RouterProvider>
    </StoreContext.Provider>
  </React.StrictMode>
);
