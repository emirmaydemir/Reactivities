import { useField } from "formik";
import { Form, Label } from "semantic-ui-react";

interface Props {
  placeholder: string;
  name: string;
  rows: number;
  label?: string;
}

export default function MyTextArea(props: Props) {
  //useField hook'u, formik form alanını kontrol etmek için kullanılır ve iki değer döner: field: Giriş alanına uygulanacak özellikler (örneğin, value, onChange, onBlur). meta: Giriş alanının meta verilerini içerir (örneğin, error, touched).
  const [field, meta] = useField(props.name);
  return (
    <Form.Field error={meta.touched && !!meta.error}>
      <label> {props.label} </label>
      {/*Bunun açıklaması mytextinputta var.*/}
      <textarea {...field} {...props} />
      {/*meta.touched: Kullanıcı bu form alanına dokundu mu? meta.error: Bu form alanında bir doğrulama hatası var mı? */}
      {meta.touched && meta.error ? (
        <Label basic color="red">
          {meta.error}
        </Label>
      ) : null}
    </Form.Field>
  );
}
