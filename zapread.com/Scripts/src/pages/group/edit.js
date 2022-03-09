/*
 * Group/Edit
 * 
 * [✓] No jquery
 */
import "../../shared/shared";                                            // [✓]
import "../../realtime/signalr";                                         // [✓]
import React, { useCallback, useEffect, useState, useRef } from 'react'; // [✓]
import { Row, Col, Form, Button, Container } from "react-bootstrap";     // [✓]
import ReactDOM from "react-dom";                                        // [✓]
import { useLocation, BrowserRouter as Router } from 'react-router-dom'; // [✓]
import PageHeading from "../../components/page-heading";
import { MultiSelect, SimpleSelect } from "react-selectize";
import { getAntiForgeryToken } from "../../utility/antiforgery";
import { postJson } from "../../utility/postData";
import { putJson } from "../../utility/putData";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";
import "../../shared/sharedlast";

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [groupName, setgroupName] = useState("");
  const [groupId, setGroupId] = useState(-1);
  const [groupNameValid, setGroupNameValid] = useState(true);
  const [groupNameError, setGroupNameError] = useState("");
  const [tags, setTags] = useState([]);
  const [languages, setLanguages] = useState([]); // These are the language options
  const [language, setLanguage] = useState({ label: "English", value: "en" });   // Default
  const [imageId, setImageId] = useState(0); // This is the image used for the group
  const [validated, setValidated] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);

  const inputFile = useRef(null);

  let query = useQuery();

  useEffect(() => {
    async function initialize() {
      if (languages.length < 1) {
        const response = await fetch("/api/v1/core/languages/list/");
        const json = await response.json();
        const newData = json.Languages;
        var languageOptions = newData.map(o => {
          const l = o.split(":");
          return { label: l[1], value: l[0] };
        })
        setLanguages(languageOptions);
        let qgroupId = query.get("groupId");
        if (qgroupId != null & qgroupId > 0 & !isLoaded) {
          setIsLoaded(true);
          console.log("Editing: ", qgroupId)
          setGroupId(qgroupId);
          loadGroup(qgroupId, languageOptions);
        }
      }
    }
    initialize();
  }, [query]); // Fire once

  // Async load languages for options
  //useEffect(() => {


  //}, []);

  function loadGroup(groupId, languageOptions) {
    postJson("/api/v1/groups/load/", {
      groupId: groupId
    }).then((response) => {
      //console.log(response);
      if (response.success) {
        var groupLanguage = languageOptions.length > 0 ? languageOptions.find(o => o.value === response.group.DefaultLanguage) : { label: "", value: response.group.DefaultLanguage };
        setLanguage(groupLanguage);
        console.log("set group language", groupLanguage, languageOptions);

        setGroupId(groupId);
        setgroupName(response.group.Name);

        // Tags are a list of strings.
        setTags(response.group.Tags.map(o => {
          return { label: o, value: o };
        }));

        setImageId(response.group.IconId);
      }
    });
  }

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
    postJson("/api/v1/groups/checkexists/", {
      GroupName: groupName,
      GroupId: groupId
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
          var tagstring = tags.map(t => t.value).join(",");

          // Submit update
          var groupData = {
            GroupId: groupId,
            GroupName: groupName,
            ImageId: imageId,
            Tags: tagstring,
            Language: language !== null ? language.value : "en"
          };

          putJson("/api/v1/groups/update/", groupData).then(response => {
            console.log(response);
            if (response.success) {
              // Go to new group
              console.log("group update success");
              window.location.href = "/Group/GroupDetail/" + response.GroupId;
              // [TODO] pupup - success
            } else {
              console.log("group update failure");
              // [TODO] pupup - failure message
            }
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
    xhr.open("POST", "/Img/Group/Icon/" + groupId + "/", true);
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
        function="Edit"
      />
      <Row>
        <Col lg={12}>
          <div className="wrapper wrapper-content animated fadeInRight">
            <div className="ibox float-e-margins">
              <div className="ibox-title">
                <h5>Edit group</h5>
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
                            This shows the image which the group will be assigned
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
                      placeholder="Select the group default language"
                      options={languages}
                      value={language}
                      onValueChange={option => {
                        setLanguage(option);
                        console.log(option);
                      }}
                    />
                  </Form.Group>
                  <Button variant="primary" type="submit">
                    Update Group
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

ReactDOM.render(
  <Router>
    <Page path="/Group/Edit" />
  </Router>
  , document.getElementById("root"));
