import { Button, Header, Segment } from "semantic-ui-react";
import axios from "axios";
import { useState } from "react";
import ValidationError from "./ValidationError";

export default function TestErrors() {
  const baseUrl = "http://localhost:5000/api/";
  const [errors, setErrors] = useState(null);

  /*const testData = {  //handleValidationError boş dizi yerine bunu ekleyerek hatayı analiz edebilirsin.
    title: "Test Activity",
    date: "2024-06-12",
    category: "Test Category",
    description: "This is a test description.",
    city: "Test City",
    venue: "Test Venue"
  };*/

  function handleNotFound() {
    axios
      .get(baseUrl + "buggy/not-found")
      .catch((err) => console.log(err.response));
  }

  function handleBadRequest() {
    axios
      .get(baseUrl + "buggy/bad-request")
      .catch((err) => console.log(err.response));
  }

  function handleServerError() {
    axios
      .get(baseUrl + "buggy/server-error")
      .catch((err) => console.log(err.response));
  }

  function handleUnauthorised() {
    axios.get(baseUrl + "buggy/unauthorised").catch((err) => console.log(err));
  }

  function handleBadGuid() {
    axios
      .get(baseUrl + "activities/notaguid")
      .catch((err) => console.log(err.response));
  }

  function handleValidationError() {
    axios.post(baseUrl + "activities", {}).catch((err) => setErrors(err));
  }

  return (
    <>
      <Header as="h1" content="Test Error component" />
      <Segment>
        <Button.Group widths="7">
          <Button onClick={handleNotFound} content="Not Found" basic primary />
          <Button
            onClick={handleBadRequest}
            content="Bad Request"
            basic
            primary
          />
          <Button
            onClick={handleValidationError}
            content="Validation Error"
            basic
            primary
          />
          <Button
            onClick={handleServerError}
            content="Server Error"
            basic
            primary
          />
          <Button
            onClick={handleUnauthorised}
            content="Unauthorised"
            basic
            primary
          />
          <Button onClick={handleBadGuid} content="Bad Guid" basic primary />
        </Button.Group>
      </Segment>
      {errors && <ValidationError errors={errors} />}
    </>
  );
}
