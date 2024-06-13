import { useField } from "formik";
import { Form, Label } from "semantic-ui-react";

interface Props {
  placeholder: string;
  name: string;
  label?: string;
}

export default function MyTextInput(props: Props) {
  //useField hook'u, formik form alanını kontrol etmek için kullanılır ve iki değer döner: field: Giriş alanına uygulanacak özellikler (örneğin, value, onChange, onBlur). meta: Giriş alanının meta verilerini içerir (örneğin, error, touched).
  const [field, meta] = useField(props.name);
  /* 
      <input {...field} {...props} /> SATIRI İÇİN AÇIKLAMA
      Eğer props nesnesi aşağıdaki gibi ise:
      {
        placeholder: "Enter your name",
        name: "username",
        label: "Username"
       }
       field nesnesi de aşağıdaki gibi Formik tarafından döndürülen değerleri içeriyorsa:
       {
        value: "",
        onChange: function(event) { ... },
        onBlur: function(event) { ... }
        }
        <input {...field} {...props} /> satırı, aşağıdaki gibi bir input elementine dönüşür:
        <input
        placeholder="Enter your name"
        name="username"
        value=""
        onChange="function(event) { ... }"
        onBlur="function(event) { ... }"/>
        */
  return (
    <Form.Field error={meta.touched && !!meta.error}>
      <label> {props.label} </label>
      <input {...field} {...props} />
      {/*meta.touched: Kullanıcı bu form alanına dokundu mu? meta.error: Bu form alanında bir doğrulama hatası var mı? */}
      {meta.touched && meta.error ? (
        <Label basic color="red">
          {meta.error}
        </Label>
      ) : null}
    </Form.Field>
  );
}
