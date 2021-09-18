/*
 * Groups/New
 * 
 * [ ] No javascript
 */
import "../../shared/shared";
import "../../realtime/signalr";

import React, { useState, useEffect, useRef } from "react";
import { Row, Col, Form, Button } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../components/page-heading";
import { MultiSelect, SimpleSelect } from "react-selectize";
import { getAntiForgeryToken } from "../../utility/antiforgery";
import { postJson } from "../../utility/postData";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

//import "selectize/dist/js/standalone/selectize";
//import "selectize/dist/css/selectize.css";
//import "selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css";
//import Swal from "sweetalert2";

import "../../shared/sharedlast";

function Page() {
  const [groupName, setgroupName] = useState("");
  const [groupNameValid, setGroupNameValid] = useState(true);
  const [groupNameError, setGroupNameError] = useState("");
  const [tags, setTags] = useState([]);
  const [languages, setLanguages] = useState([]); // These are the language options
  const [language, setLanguage] = useState(null);
  const [imageId, setImageId] = useState(0); // This is the image used for the group
  const [validated, setValidated] = useState(false);

  const inputFile = useRef(null);

  // Async load languages for options
  useEffect(() => {
    async function getLanguages() {
      const response = await fetch("/api/v1/core/langages/list/");
      const json = await response.json();
      const newData = json.Languages;
      setLanguages(
        newData.map(o => {
          const l = o.split(":");
          return { label: l[1], value: l[0] };
        })
      );
    }
    getLanguages();
  }, []);

  function defaultLanguage() {
    return language !== null ? language : languages.find(o => o.value === "en");
  }

  async function handleSubmit(event) {
    //console.log("Submitted", event);
    //console.log("groupName", groupName);
    console.log("tags", tags);
    event.preventDefault();
    event.stopPropagation();

    //if (form.checkValidity() === false) {
    //  event.preventDefault();
    //  event.stopPropagation();
    //}
    setValidated(true);

    // Check if group is valid
    postJson("/Group/CheckExists/", {
      groupName: groupName
    }).then(response => {
      if (response.success) {
        if (response.exists) {
          setGroupNameValid(false);
          setGroupNameError("This group already exists");
          return true;
        } else {
          setGroupNameValid(true);
          setGroupNameError("");
          return false;
        }
      }
    });

    // If everything is valid - submit for group creation
    if (groupNameValid) {
      console.log("valid group name");
    }
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
    //console.log(e);
    //console.log(id);
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
            <div className="ibox float-e-margins">
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
                      values={tags}
                      onValuesChange={tags => {
                        setTags(tags);
                        console.log(tags);
                      }}
                      // createFromSearch :: [Item] -> [Item] -> String -> Item?
                      createFromSearch={function(options, values, search) {
                        var labels = values.map(function(value) {
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
                        <Row>
                          <Col xs="auto" sm="auto">
                            <input
                              type="file"
                              id="file"
                              ref={inputFile}
                              accept="image"
                              onChange={e => handleFileChange(e.target.files)}
                              style={{ display: "none" }}
                            />
                          </Col>
                          <Col
                            xs={{ span: 2, offset: 1 }}
                            sm={{ span: 2, offset: 1 }}
                          >
                            {/*This shows the image which the group will be assigned*/}
                            <img src={`/Img/Group/IconById/${imageId}/?s=50`} />
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
                      placeholder="Select the group default language"
                      options={languages}
                      value={defaultLanguage()}
                      onValueChange={option => {
                        setLanguage(option);
                        console.log(option);
                      }}
                    />
                  </Form.Group>
                  <Button variant="primary" type="submit">
                    Create Group
                  </Button>
                </Form>
              </div>
            </div>
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
