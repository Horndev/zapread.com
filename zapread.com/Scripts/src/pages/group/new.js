/*
 * Groups/New
 * 
 * [ ] No javascript
 */
import "../../shared/shared";
import "../../realtime/signalr";
import Swal from 'sweetalert2';
import React, { useState, useEffect, useRef } from "react";
import { Row, Col, Form, Button, Container } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../components/PageHeading";
import { MultiSelect, SimpleSelect } from "react-selectize";
import { getAntiForgeryToken } from "../../utility/antiforgery";
import { postJson } from "../../utility/postData";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";
import "../../shared/sharedlast";

function Page() {
  const [groupName, setgroupName] = useState("");
  const [captchaValue, setCaptchaValue] = useState("");
  const [captchaValueValid, setCaptchaValueValid] = useState(false);
  const [groupNameValid, setGroupNameValid] = useState(true);
  const [groupNameError, setGroupNameError] = useState("");
  const [tags, setTags] = useState([]);
  const [languages, setLanguages] = useState([]); // These are the language options
  const [language, setLanguage] = useState(null);
  const [imageId, setImageId] = useState(0); // This is the image used for the group
  const [validated, setValidated] = useState(false);
  const [captchaImgB64, setCaptchaImgB64] = useState("");

  const inputFile = useRef(null);
  const captchaAudioRef = useRef(null);

  // Async load languages for options
  useEffect(() => {
    async function getLanguages() {
      const response = await fetch("/api/v1/core/languages/list/");
      const json = await response.json();
      const newData = json.Languages;
      setLanguages(
        newData.map(o => {
          const l = o.split(":");
          return { label: l[1], value: l[0] };
        })
      );
    }

    //async function getCaptcha() {
    //  const response = await fetch("/Group/CaptchaImage/");
    //  const json = await response.json();
    //  setCaptchaImgB64(json.B64Image);
    //}

    getLanguages();
    //getCaptcha();
  }, []);

  function defaultLanguage() {
    return language !== null ? language : languages.find(o => o.value === "en");
  }

  async function handleSubmit(event) {
    console.log("tags", tags);
    event.preventDefault();
    event.stopPropagation();
    //if (form.checkValidity() === false) {
    //  event.preventDefault();
    //  event.stopPropagation();
    //}
    setValidated(true);

    var tagstring = tags.map(t => t.value).join(",");

    // Check if captcha entered
    if (captchaValue != "") {
      setCaptchaValueValid(true);
    } else {
      setCaptchaValueValid(false);
    }


    // Check if group is valid
    postJson("/api/v1/groups/checkexists/", {
      GroupName: groupName
    })
      .then(response => {
        if (response.success) {
          if (response.exists) {
            setGroupNameValid(false);
            setGroupNameError("This group already exists");
            return false;
          } else {
            setGroupNameValid(true);
            setGroupNameError("");
            return true;
          }
        }
      })
      .then(isvalid => {
        // If everything is valid - submit for group creation
        if (isvalid) {
          console.log("valid group name");
          var newGroupData = {
            GroupName: groupName,
            ImageId: imageId,
            Tags: tagstring,
            Language: language !== null ? language.value : "en",
            Captcha: captchaValue
          };
          postJson("/api/v1/groups/add/", newGroupData).then(response => {
            console.log(response);
            if (response.success) {
              // Go to new group
              console.log("groups add success");
              window.location.href = "/Group/GroupDetail/" + response.GroupId;
            }
          }).catch(error => {
            console.log("error", error);
            if (error instanceof Error) {
              Swal.fire("Error", `${error.message}`, "error");
            }
            error.json().then(data => {
              Swal.fire("Error", `${data.Message}`, "error");
            })
          });
        }
      });
  }

  function handleFileChange(selectorFiles) {
    var file = selectorFiles[0];

    var fd = new FormData();
    fd.append("file", file);
    const xhr = new XMLHttpRequest();

    // updateImgId is from the react state
    xhr.open("POST", "/Img/Group/Icon/-1/", true);
    var headers = getAntiForgeryToken();

    for (var index in headers) {
      xhr.setRequestHeader(index, headers[index]);
    }

    // listen callback
    xhr.onload = () => {
      if (xhr.status === 200) {
        var data = JSON.parse(xhr.responseText);
        console.log(data.imgId);
        setImageId(data.imgId); // This is the new image id which the user just uploaded
      }
    };

    // Execute the request
    xhr.send(fd);
  }

  /**
   * initializes the user to select an icon to upload for the group image
   * @param {any} id
   * @param {any} e
   */
  function updateIcon(id, e) {
    inputFile.current.click();
  }

  return (
    <>
      <PageHeading
        title="Groups"
        controller="Home"
        method="Groups"
        function="New"
      />
      <Row>
        <Col lg={12}>
          <div className="wrapper wrapper-content animated fadeInRight">
            {/*<div className="ibox float-e-margins">*/}
            {/*  <div className="wrapper wrapper-content">*/}
            <Row>
              <Col lg={3}></Col>
              <Col lg={6}>
                <div className="ibox-title">
                  <h5>Create a new group</h5>
                </div>
                <div className="ibox-content">
                  <Form noValidate validated={validated} onSubmit={handleSubmit}>
                    <Form.Group controlId="GroupName">
                      <Form.Label>Group Name</Form.Label>
                      <Form.Control
                        placeholder="Enter group name"
                        value={groupName}
                        type="text"
                        onChange={({ target: { value } }) => setgroupName(value)}
                        isValid={groupName && groupNameValid}
                        isInvalid={validated && (!groupName || !groupNameValid)}
                      />
                      <Form.Control.Feedback type="invalid">
                        Please provide a unique group name. {groupNameError}
                      </Form.Control.Feedback>
                      <Form.Text id="groupNameBlock" muted>
                        The group name should be short and clear. A good name will
                        help users discover the group.
                      </Form.Text>
                    </Form.Group>
                    <Form.Group controlId="Tags">
                      <Form.Label>Tags</Form.Label>
                      <MultiSelect
                        style={{ width: "100%" }}
                        values={tags}
                        onValuesChange={tags => {
                          setTags(tags);
                          console.log(tags);
                        }}
                        // createFromSearch :: [Item] -> [Item] -> String -> Item?
                        createFromSearch={function (options, values, search) {
                          var labels = values.map(function (value) {
                            return value.label;
                          });
                          if (
                            search.trim().length == 0 ||
                            labels.indexOf(search.trim()) != -1
                          )
                            return null;
                          return { label: search.trim(), value: search.trim() };
                        }}
                      />
                      <small id="multiselHelp" className="form-text text-muted">
                        Tags help people search and filter groups.
                      </small>
                    </Form.Group>
                    <Form.Group>
                      <Form.Label>Icon</Form.Label>
                      <Form.Row>
                        <Container fluid>
                          <Row className="align-items-center">
                            <Col xs="auto" sm="auto" md="auto">
                              <input
                                type="file"
                                id="file"
                                ref={inputFile}
                                accept="image"
                                onChange={e => handleFileChange(e.target.files)}
                                style={{ display: "none" }}
                              />
                              <img
                                src={`/Img/Group/IconById/${imageId}/?s=100`}
                              />
                            </Col>
                            <Col xs="auto" sm="auto" md="auto">
                              {/*This shows the image which the group will be assigned*/}
                            </Col>
                            <Col>
                              <Button
                                size="sm"
                                variant="outline-primary"
                                onClick={e => updateIcon(-1, e)}
                              >
                                Change Icon
                              </Button>
                            </Col>
                          </Row>
                        </Container>
                      </Form.Row>
                      <Form.Text id="groupIconBlock" muted>
                        The icon is a small visual branding for the group.
                      </Form.Text>
                    </Form.Group>
                    <Form.Group controlId="GroupName">
                      <Form.Label>Default Language</Form.Label>
                      <SimpleSelect
                        style={{ width: "100%" }}
                        placeholder="Select the group default language"
                        options={languages}
                        value={defaultLanguage()}
                        onValueChange={option => {
                          setLanguage(option);
                          console.log(option);
                        }}
                      />
                    </Form.Group>
                    <Form.Group controlId="GroupName">
                      <div className="captcha-box" id="Captcha">
                        <img src={"data:image/png;base64, " + captchaImageB64} />
                        <div className="captcha-audio-overlay" id="CaptchaAudioButton" onClick={() => {
                          var audio = captchaAudioRef.current;
                          audio.play();
                        }}>
                          <span className="captcha-audio-btn">
                            <a className="btn btn-sm btn-outline-primary captcha-audio-btn">
                              <i className="fa fa-volume-up"></i>
                            </a>
                          </span>
                        </div>
                        <audio ref={captchaAudioRef} id="CaptchaAudio" src='/Account/CaptchaAudio' type='audio/mpeg' preload='none'></audio>
                      </div>
                      {/*<Form.Label>Enter Captcha</Form.Label>*/}
                      <Form.Control
                        autoComplete="off"
                        placeholder="Enter the Captcha from the image above"
                        value={captchaValue}
                        type="text"
                        onChange={({ target: { value } }) => setCaptchaValue(value)}
                        isValid={captchaValue && captchaValueValid}
                        isInvalid={validated && (!captchaValue || !captchaValueValid)}
                      />
                      <Form.Control.Feedback type="invalid">
                        Please enter the Captcha
                      </Form.Control.Feedback>
                    </Form.Group>
                    <Button block variant="primary" type="submit">
                      Create Group
                    </Button><br /><div style={{height: "40px"}}></div>
                  </Form>
                </div>
              </Col>
              <Col lg={3}></Col>
            </Row>
            {/*  </div>*/}
            {/*</div>*/}
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
