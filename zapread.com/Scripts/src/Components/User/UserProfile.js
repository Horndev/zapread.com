/*
 * 
 */

import React, { Suspense, useState, useEffect } from 'react';
import { Container, Row, Col, Button, Dropdown } from "react-bootstrap";
import Swal from 'sweetalert2';
import Tippy from '@tippyjs/react';
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
const ProfileImageModal = React.lazy(() => import("./ProfileImageModal"));
const AboutMeModal = React.lazy(() => import("./AboutMeModal"));

import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';

export default function UserProfile(props) {
  const [aboutMe, setAboutMe] = useState("");
  const [achievements, setAchievements] = useState([]);
  const [showProfileImageModal, setShowProfileImageModal] = useState(false);
  const [showAboutMeModal, setShowAboutMeModal] = useState(false);
  const [name, setName] = useState("");
  const [numFollowing, setNumFollowing] = useState(0);
  const [numFollowers, setNumFollowers] = useState(0);
  const [numPosts, setNumPosts] = useState(0);
  const [reputation, setReputation] = useState(0);
  const [userProfileImageVersion, setUserProfileImageVersion] = useState(null);

  function updateImagesOnPage(ver) {
    document.getElementById("userImageLarge").setAttribute("src", "/Home/UserImage/?size=500&v=" + ver);
    var elements = document.querySelectorAll(".user-image-30");
    Array.prototype.forEach.call(elements, function (el, _i) {
      el.setAttribute("src", "/Home/UserImage/?size=30&v=" + ver);
    });
    elements = document.querySelectorAll(".user-image-45");
    Array.prototype.forEach.call(elements, function (el, _i) {
      el.setAttribute("src", "/Home/UserImage/?size=45&v=" + ver);
    });
    elements = document.querySelectorAll(".user-image-15");
    Array.prototype.forEach.call(elements, function (el, _i) {
      el.setAttribute("src", "/Home/UserImage/?size=15&v=" + ver);
    });
  }

  const onRotateImage = () => {
    postJson('/Manage/RotateProfileImage/').then(response => {
      if (response.success) {
        console.log('rotated');
        updateImagesOnPage(response.version); // Reload images
      } else {
        // Did not work
        Swal.fire("Error updating: " + data.message, "error");
      }
    }).catch((error) => {
      if (error instanceof Error) {
        Swal.fire("Error", `${error.message}`, "error");
      }
      else {
        error.json().then(data => {
          Swal.fire("Error", `${data.message}`, "error");
        })
      }
    });
  };

  const generateRobot = (set) => {
    postJson('/Home/SetUserImage/',
      {
        set: set
      }).then((response) => {
        if (response.success) {
          // Reload images
          updateImagesOnPage(response.version);
        } else {
          // Did not work
          Swal.fire({
            icon: 'error',
            title: 'Image Update Error',
            text: "Error updating image: " + response.message
          })
        }
      }).catch((error) => {
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        else {
          error.json().then(data => {
            Swal.fire("Error", `${data.message}`, "error");
          })
        }
      });
    return false; // Prevent jump to top of page
  }

  const updateAlias = () => {
    Swal.fire({
      title: "Change your alias",
      input: "text",
      inputAttributes: {
        autocapitalize: 'off'
      },
      showCancelButton: true,
      showLoaderOnConfirm: true,
      preConfirm: (dataval) => {
        return postJson("/Manage/UpdateUserAlias", {
          alias: dataval
        });
      },
      allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
      if (result.isConfirmed) {
        if (result.value.success) {
          setName(result.value.newName);
          //document.getElementById('labelUsername').innerHTML = result.value.newName;
          var navbarNameEl = document.getElementById('navbarUserName');
          if (navbarNameEl) {
            navbarNameEl.innerHTML = result.value.newName;
          }
          Swal.fire("Update successful", `your alias is now ${result.value.newName}`, "success");
        } else {
          Swal.fire("Error", `${response.value.message}`, "error");
        }
      }
    });
  };

  const updateAboutMe = () => {
    setShowAboutMeModal(true);
  };

  useEffect(() => {
    async function initialize() {
      getJson("/api/v1/user/current/")
        .then((response) => {
          if (response.success) {
            setName(response.Name);
            setUserProfileImageVersion(response.UserProfileImageVersion);
            setAboutMe(response.AboutMe);
            setReputation(response.Reputation);
            setAchievements(response.Achievements);
            setNumPosts(response.NumPosts);
            setNumFollowing(response.NumFollowing);
            setNumFollowers(response.NumFollowers);
          }
        }).catch((error) => {
          console.log(error);
        });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <Suspense fallback={<></>}>
        <ProfileImageModal
          show={showProfileImageModal}
          onUpdated={(value) => { updateImagesOnPage(value);}}
        />
      </Suspense>
      <Suspense fallback={<></>}>
        <AboutMeModal
          show={showAboutMeModal}
          aboutMe={aboutMe}
          onUpdated={(value) => { setAboutMe(value); }} />
      </Suspense>
      
      <div className="ibox float-e-margins">
        <div className="ibox-title">
          <h5>Profile Detail</h5>
          <div className="zr-tools">
            <Dropdown className="zr-small-dropdown">
              <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white">
                <i className="fa fa-wrench"></i>
              </Dropdown.Toggle>
              <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right m-t-xs">
                <Dropdown.Item as="li" onClick={() => { setShowProfileImageModal(true); }}>
                  <button className="btn btn-link btn-sm">
                    Update Profile Image
                  </button>
                </Dropdown.Item>
                <Dropdown.Item as="li" onClick={onRotateImage}>
                  <button className="btn btn-link btn-sm">
                    Rotate Profile Image
                  </button>
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </div>
        </div>
        <div className="ibox-content no-padding border-left-right">
          <img id="userImageLarge" className="img-fluid" src={"/Home/UserImage/?size=500" + (userProfileImageVersion ? "&v=" + userProfileImageVersion : "")} />
        </div>

        <div className="ibox-content profile-content">
          <div>
            <strong>
              <img className="img-circle user-image-30" src={"/Home/UserImage/?size=30" + (userProfileImageVersion ? "&v=" + userProfileImageVersion : "")} />
              <big><span id="labelUsername">{" "}{name}{" "}</span></big>
            </strong>

            <Dropdown className="zr-small-dropdown">
              <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white">
                <i className="fa fa-cog"></i>
              </Dropdown.Toggle>
              <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right m-t-xs">
                <Dropdown.Item as="li" onClick={() => { generateRobot(1) }}>
                  <button className="btn btn-link btn-sm">Generate Profile Image (Robot)</button>
                </Dropdown.Item>
                <Dropdown.Item as="li" onClick={() => { generateRobot(2) }}>
                  <button className="btn btn-link btn-sm">Generate Profile Image (Cat)</button>
                </Dropdown.Item>
                <Dropdown.Item as="li" onClick={() => { generateRobot(3) }}>
                  <button className="btn btn-link btn-sm">Generate Profile Image (Human)</button>
                </Dropdown.Item>
                <Dropdown.Item as="li" onClick={() => { generateRobot(4) }}>
                  <button className="btn btn-link btn-sm">Generate Profile Image (Monster)</button>
                </Dropdown.Item>
                <Dropdown.Item as="li" onClick={() => { updateAlias() }}>
                  <button className="btn btn-link btn-sm">Change Alias</button>
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>

          </div>
          <br />
          <p><i className="fa fa-star"></i> Reputation { reputation }</p>
          <div>
            <big>About me</big>
            <Dropdown className="zr-small-dropdown">
              <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white">
                <i className="fa fa-cog"></i>
              </Dropdown.Toggle>
              <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right m-t-xs">
                <Dropdown.Item as="li" onClick={() => { updateAboutMe() }}>
                  <button className="btn btn-link btn-sm">Update About Me</button>
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </div>
          <p>
            { aboutMe }
          </p>
          <br />
          <Row>
            <Col md={4}><h5><strong>{numPosts}</strong> Posts</h5></Col>
            <Col md={4}><h5><strong>{numFollowing}</strong> Following</h5></Col>
            <Col md={4}><h5><strong>{numFollowers}</strong> Followers</h5></Col>
          </Row>
          <Row className="m-t-lg">
            <Col lg={12}>
              <h5>Achievements</h5>
              {achievements.map((a, index) => (
                <Tippy
                  key={a.Id}
                  theme="light-border"
                  interactive={true}
                  interactiveBorder={30}
                  content={
                    <Container>
                      <Row>
                        <Col>
                          <img className="img-circle" src={"/Img/AchievementImage/" + a.Id + "/"} />
                          <span style={{ display: "inline" }}><strong>{a.Name}</strong></span>
                        </Col>
                      </Row>
                      <Row>
                        <Col>
                          {a.Description}
                          <h5>Achieved On</h5>
                          {new Date(a.DateAchieved).toLocaleDateString()}
                        </Col>
                      </Row>
                    </Container>
                  }>
                    <img 
                      className="ach-hover"
                      data-achid={a.Id}
                      src={"/Img/AchievementImage/" + a.ImageId + "/"}
                      title={a.Name} />
                  </Tippy>
                ))}
            </Col>
          </Row>
        </div>
      </div>
    </>
  );
}