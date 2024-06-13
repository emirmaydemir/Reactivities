import { useField } from "formik";
import { Form, Label } from "semantic-ui-react";
import DatePicker, { ReactDatePickerProps } from "react-datepicker";

//Kendi propsu olduğu için datepickerin bizim oluşturmamıza gerek kalmadı. Partial yapmamızın sebebi tüm propsları zorunlu kılmasın diye. Sadece istediğimiz props özelliklerini kullanırız bu sayede.
export default function MyDateInput(props: Partial<ReactDatePickerProps>) {
  //useField hook'u, formik form alanını kontrol etmek için kullanılır ve iki değer döner: field: Giriş alanına uygulanacak özellikler (örneğin, value, onChange, onBlur). meta: Giriş alanının meta verilerini içerir (örneğin, error, touched).
  const [field, meta, helpers] = useField(props.name!);
  return (
    <Form.Field error={meta.touched && !!meta.error}>
      <DatePicker
        {...field}
        {...props}
        selected={(field.value && new Date(field.value)) || null}
        onChange={(value) => helpers.setValue(value)}
      />
      {/*meta.touched: Kullanıcı bu form alanına dokundu mu? meta.error: Bu form alanında bir doğrulama hatası var mı? */}
      {meta.touched && meta.error ? (
        <Label basic color="red">
          {meta.error}
        </Label>
      ) : null}
    </Form.Field>
  );
}
