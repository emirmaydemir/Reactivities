import { useField } from "formik";
import { Form, Label, Select } from "semantic-ui-react";

interface Props {
  placeholder: string;
  name: string;
  options: { text: string; value: string }[];
  label?: string;
}

export default function MySelectInput(props: Props) {
  //props.name sayesinde hangi fieldin özelliklerine erişiceğine karar verebiliyor. Burada category için bakıyor.
  //useField hook'u, formik form alanını kontrol etmek için kullanılır ve iki değer döner: field: Giriş alanına uygulanacak özellikler (örneğin, value, onChange, onBlur). meta: Giriş alanının meta verilerini içerir (örneğin, error, touched).
  const [field, meta, helpers] = useField(props.name);

  return (
    <Form.Field error={meta.touched && !!meta.error}>
      <label> {props.label} </label>
      {/*Combobox gibi bir yapı kurduk burada kendi özellikleri var dökümanda yazan kafanı karıştırmasın yok onblur bs falan.*/}
      <Select
        clearable
        options={props.options}
        value={field.value || null}
        onChange={(_, d) => helpers.setValue(d.value)}
        onBlur={() => helpers.setTouched(true)}
        placeholder={props.placeholder}
      ></Select>
      {/*meta.touched: Kullanıcı bu form alanına dokundu mu? meta.error: Bu form alanında bir doğrulama hatası var mı? */}
      {meta.touched && meta.error ? (
        <Label basic color="red">
          {meta.error}
        </Label>
      ) : null}
    </Form.Field>
  );
}
