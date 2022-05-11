/*
 * Admin Panel Reaction Icons
 * 
 * [ ] Set the default group icon
 * [ ] Change a group icon
 * [ ] Remove group icon
 */

import "../../shared/shared"; // [✓]
import "../../realtime/signalr"; // [✓]
import React, {
  useCallback,
  useMemo,
  useRef,
  useEffect,
  useState
} from "react";
import ReactDOM from "react-dom";
import Swal from 'sweetalert2';
import { Row, Col, Form, Button, Container } from "react-bootstrap";
import PageHeading from "../../components/PageHeading";
import { getAntiForgeryToken } from "../../utility/antiforgery";
import { postJson } from "../../utility/postData";
import UserAutosuggest from "../../Components/UserAutosuggest";
import "../../shared/sharedlast"; // [✓]

const baseStyle = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  padding: "20px",
  borderWidth: 2,
  borderRadius: 2,
  borderColor: "#eeeeee",
  borderStyle: "dashed",
  backgroundColor: "#fafafa",
  color: "#bdbdbd",
  outline: "none",
  transition: "border .24s ease-in-out"
};

const activeStyle = {
  borderColor: "#2196f3"
};

const acceptStyle = {
  borderColor: "#00e676"
};

const rejectStyle = {
  borderColor: "#ff1744"
};

function Page() {
  const [reactions, setReactions] = useState([]);
  const [isLoaded, setIsLoaded] = useState(false);
  const [validated, setValidated] = useState(false);
  const [userAppId, setUserAppId] = useState("");
  const [userName, setUserName] = useState("");
  const [reactionName, setReactionName] = useState("");
  const [reactionIcon, setReactionIcon] = useState("");
  const [reactionDescription, setReactionDescription] = useState("");

  async function getReactions() {
    await fetch('/api/v1/admin/reactions/list/')
      .then(response => response.json())
      .then(json => {
        setReactions(json.Reactions);
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await getReactions();
      }
    }
    initialize();
  }, []); // Fire once

  async function handleSubmit(event) {
    event.preventDefault();
    event.stopPropagation();
    setValidated(true);

    var newReactionData = {
      ReactionName: reactionName,
      ReactionDescription: reactionDescription,
      ReactionIcon: reactionIcon
    };

    postJson("/api/v1/admin/reactions/add", newReactionData).then(response => {
      console.log(response);
      if (response.success) {
        const newReactions = [...reactions, response.Reaction];
        setReactions(newReactions);
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

  function onSelected(values) {
    console.log("selected", values);
    setUserAppId(values.UserAppId);
    setUserName(values.userName);
  }

  function grant(reactionId) {

    var grantData = {
      UserAppId: userAppId,
      ReactionId: reactionId
    }

    postJson("/api/v1/admin/reactions/grant/", grantData).then(response => {
      console.log(response);
      if (response.success) {
        Swal.fire("success", "Reaction Granted", "success");
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

  return (
    <>
      <PageHeading
        title="ZapRead Reaction Icons"
        controller="Admin"
        method="Reaction Icons"
        function="Edit"
      />
      <Container>
        <Row>
          <Col>
            <Form noValidate validated={validated} onSubmit={handleSubmit}>
              <Form.Group controlId="ReactionName">
                <Form.Label>Reaction Name</Form.Label>
                <Form.Control
                  placeholder="Enter reaction name"
                  value={reactionName}
                  type="text"
                  onChange={({ target: { value } }) => setReactionName(value)}
                />
              </Form.Group>
              <Form.Group controlId="ReactionIcon">
                <Form.Label>Reaction Icon</Form.Label>
                <Form.Control
                  placeholder="Enter reaction icon"
                  value={reactionIcon}
                  type="text"
                  onChange={({ target: { value } }) => setReactionIcon(value)}
                />
              </Form.Group>
              <Form.Group controlId="ReactionDescription">
                <Form.Label>Reaction Description</Form.Label>
                <Form.Control
                  placeholder="Enter reaction description"
                  value={reactionDescription}
                  type="text"
                  onChange={({ target: { value } }) => setReactionDescription(value)}
                />
              </Form.Group>
              <Button block variant="primary" type="submit">
                Add Reaction
              </Button><br /><div style={{ height: "40px" }}></div>
            </Form>
          </Col>
        </Row>
        <Row><Col>
          <UserAutosuggest label="User Name" onSelected={onSelected} url="/api/v1/user/search" />
        </Col></Row>
        {reactions.map((reaction, index) => (
          <Row key={reaction.ReactionId}>
            <Col>
              {reaction.ReactionId}
            </Col>
            <Col>
              {reaction.ReactionName}
            </Col>
            <Col style={{fontSize: "22px"}}>
              <div dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }} />
            </Col>
            <Col>
              {reaction.reactionDescription}
            </Col>
            <Col>
              <Button>
                Delete
              </Button>
              <Button>
                Grant to All Users
              </Button>
              <Button onClick={() => { grant(reaction.ReactionId); }}>
                Grant to User
              </Button>
            </Col>
          </Row>
        ))}
        <Col><Row><br /></Row></Col>
        <Col><Row><br /></Row></Col>
      </Container>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));