/**
 * Page for editing a post
 **/

import '../../shared/shared';
import '../../realtime/signalr';
import React, { Suspense, useCallback, useEffect, useState, useRef } from 'react';
import { Placeholder, Container, Row, Col, Form, CheckBox, FormGroup, FormLabel, FormCheck } from 'react-bootstrap';
import ReactDOM from 'react-dom';
import { useLocation, BrowserRouter as Router } from 'react-router-dom';
import { loadCaptchaEnginge, LoadCanvasTemplate, LoadCanvasTemplateNoReload, validateCaptcha } from 'react-simple-captcha';
import Swal from 'sweetalert2';
import withReactContent from 'sweetalert2-react-content'
import Input from '../../Components/Input/Input';
//import Editor from './Components/Editor';
const Editor = React.lazy(() => import("./Components/Editor"));
//import DraftsTable from './Components/DraftsTable';
const DraftsTable = React.lazy(() => import("./Components/DraftsTable"));
import GroupPicker from './Components/GroupPicker';
import LanguagePicker from './Components/LanguagePicker';
import { postJson } from '../../utility/postData';
import PageHeading from '../../components/page-heading';
import '../../shared/sharedlast';
import '../../css/quill/quillfont.css';
import 'quill/dist/quill.core.css'
import 'quill/dist/quill.snow.css'
import '../../css/quill/quilledit.css';                              // [✓]
import '../../css/quill/quillcustom.css'; // Some custom overrides

const RSwal = withReactContent(Swal)

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [postContent, setPostContent] = useState('');
  const [numSaves, setNumSaves] = useState(0);
  const [postId, setPostId] = useState(-1);
  const [groupId, setGroupId] = useState(1);
  const [groupName, setGroupName] = useState('');
  const [userAppId, setUserAppId] = useState('');
  const [userImgVer, setUserImgVer] = useState(0);
  const [postLanguage, setPostLanguage] = useState('English');
  const [postTitle, setPostTitle] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);
  const [postNSFW, setPostNSFW] = useState(false);
  const [postQuietly, setPostQuietly] = useState(false);

  let query = useQuery();

  useEffect(() => {
    let qpostId = query.get("postId");
    if (qpostId != null & qpostId > 0 & !isLoaded) {
      console.log("Editing: ", qpostId)
      setPostId(qpostId);
      loadPost(qpostId, false);
    }

    let qgroupId = query.get("groupId");
    if (qgroupId != null & qgroupId > 0 & !isLoaded) {
      //console.log("Group: ", qgroupId)
      setGroupId(qgroupId);
      postJson("/api/v1/groups/load/", {
        groupId: qgroupId
      }).then((response) => {
        if (response.success) {
          setGroupId(response.group.Id);
          setGroupName(response.group.Name);
        }
      });
    }

    let appid = document.getElementById('zruserinfo').getAttribute("data-appid");
    setUserAppId(appid);

    let upiver = document.getElementById('zruserinfo').getAttribute("data-piver");
    setUserImgVer(upiver);

    if (!isLoaded) {
      setIsLoaded(true);
    }
  }, [query]); // Fire once

  const handleSaveDraft = useCallback(() => {
    if (!isSaving) {
      setIsSaving(true);  // Lock the saving so the post submit doesn't get called twice

      var msg = {
        postId: postId,
        groupId: groupId,
        content: postContent,
        postTitle: postTitle,
        language: postLanguage,
        isDraft: true,
        isNSFW: postNSFW,
        postQuietly: postQuietly
      };

      postJson("/Post/Submit/", msg)
        .then((response) => {
          //console.log(response);
          setPostId(response.postId);
          setNumSaves(numSaves + 1);
          setIsSaving(false);         // Release the saving lock
        });
    }
  }, [postTitle, postContent, postLanguage, postId, groupId, postNSFW]);    // Save the draft if any of these variables update

  function handleDeleteDraft(postId) {
    Swal.fire({
      title: "Are you sure?",
      text: "This draft will be permanently deleted.",
      icon: "warning",
      showCancelButton: true
    }).then((willDelete) => {
      if (willDelete.value) {
        postJson("/Post/Draft/Delete/", {
          postId: postId,
        }).then((response) => {
          //console.log(response);
          if (response.success) {
            setNumSaves(numSaves + 1); // updates draft table
          }
          else {
            // ?
          }
        });
      } else {
        //console.log("cancelled load");
      }
    });
  }

  function loadPost(postId, isDraft) {
    postJson("/Post/Draft/Load/", {
      postId: postId,
      isDraft: isDraft,
    }).then((response) => {
      if (response.success) {
        setNumSaves(numSaves + 1);                  // updates draft table
        setPostContent(response.draftPost.Content); // set the editor content
        setPostId(postId);                          // this is the post we are now editing
        setGroupName(response.draftPost.GroupName);
        setPostTitle(response.draftPost.PostTitle);
      }
    });
  }

  function handleLoadPost(postId) {
    Swal.fire({
      title: "Are you sure?",
      text: "Any unsaved changes in the current post will be lost.",
      icon: "warning",
      showCancelButton: true
    }).then((willLoad) => {
      if (willLoad.value) {
        loadPost(postId, true);
      } else {
        //console.log("cancelled load");
      }
    });
  }

  function handleNSFWChange(evt) {
    //console.log("handleNSFWChange", evt);
    setPostNSFW(evt.target.checked);
  }

  async function doSubmit() {
    var msg = {
      postId: postId,
      groupId: groupId,
      content: postContent,
      postTitle: postTitle,
      language: postLanguage,
      isDraft: false,
      isNSFW: postNSFW,
      postQuietly: postQuietly
    };

    document.getElementById("submitnotification").style.display = "";

    await postJson("/Post/Submit/", msg)
      .then((response) => {
        if (response.success) {
          var newPostUrl = "/Post/Detail";
          newPostUrl = newPostUrl + '/' + response.postId;
          window.location.replace(newPostUrl);  // Navigate to the new post
        }
        document.getElementById("submitnotification").style.display = "none";
      }).catch((error) => {
        document.getElementById("submitnotification").style.display = "none";
        console.log(error);
      });
  }

  const handleSubmitPost = useCallback(() => {
    var rep = document.getElementById('zruserinfo').getAttribute("data-reputation");
    if (rep > 5000) {
      doSubmit();
    }
    else {
      RSwal.fire({
        title: <p>Verify you are human</p>,
        showCancelButton: true,
        html:
          <>
            <div>
              <LoadCanvasTemplate />
              <input type="text" id="user_captcha_input" className="swal2-input" autoComplete="off" />
            </div>
          </>,
        footer: <><div style={{ textAlign: "center" }}>Captcha is required when reputation is below 5000.  Your reputation is {rep}.</div></>,
        didOpen: () => {
          loadCaptchaEnginge(6);
        }
      }).then((result) => {
        if (result.isConfirmed) {
          let user_captcha_value = document.getElementById('user_captcha_input').value;
          if (validateCaptcha(user_captcha_value, false) == true) {
            doSubmit();
          } else {
            RSwal.fire({
              html: <p>Captcha does not match</p>,
              icon: 'error'
            });
          }
        }
      })
    }
  }, [postTitle, postContent, postLanguage, postId, groupId, postNSFW]);

  return (
    <div>
      <PageHeading title="New Post" controller="Post" method="Edit" function="New" />
      <div>
        <Row>
          <Col lg={2}></Col>
          <Col lg={8}>
            <div className="ibox-title" style={{ display: "inline-flex", width: "100%", marginTop: "8px" }}>
              <div className="social-avatar" style={{ paddingTop: "5px" }}>
                <img className="img-circle user-image-45" width="45" height="45" src={"/Home/UserImage/?size=45&UserId=" + userAppId + "&v=" + userImgVer} />
              </div>
              <Input
                id="postTitle"
                label="Title"
                value={postTitle}
                setValue={setPostTitle}
                predicted=""
                locked={false}
                active={false}
              />
            </div>
            <div className="ibox-title" style={{ display: "flex" }}>
              <GroupPicker
                label="Group"
                value={groupName}
                setValue={(v) => { setGroupName(v.groupName); setGroupId(v.groupId); }}
              />
              <div style={{
                paddingLeft: "20px",
                display: "flex"
              }}>
                <LanguagePicker
                  label={"\uf1ab"}
                  value={postLanguage}
                  setValue={setPostLanguage}
                />
              </div>
            </div>
            <div className="ibox-title" style={{ display: "flex" }}>
              <Form.Check type="checkbox" label="Mark post Not Safe For Work (NSFW)"
                checked={postNSFW}
                onChange={e => setPostNSFW(e.target.checked)}
              />
            </div>
            <div className="ibox-title" style={{ display: "flex" }}>
              <Form.Check type="checkbox" label="Post quietly (don't notify followers)"
                checked={postQuietly}
                onChange={e => { console.log("setPostQuietly", e.target.checked); setPostQuietly(e.target.checked) }}
              />
            </div>
          </Col>
        </Row>
      </div>
      <div className="wrapper wrapper-content">
        <Row>
          <Col lg={2}></Col>
          <Col lg={8}>
            <div className="submitoverlay" id="submitnotification" style={{ display: "none" }}>
              <div className="submit-sk-loading sk-loading">
                <div className="sk-spinner sk-spinner-three-bounce">
                  <div className="sk-bounce1"></div>
                  <div className="sk-bounce2"></div>
                  <div className="sk-bounce3"></div>
                </div>
              </div>
            </div>
            <div className="savingoverlay" id="savingnotification" style={{ display: "none" }}>
              <i className="fa-solid fa-circle-notch fa-spin"></i> saving...
            </div>
            <Suspense fallback={<></>}>
              {/*<Placeholder as="p" animation="glow">*/}
              {/*  <Placeholder xs={12} />*/}
              {/*</Placeholder>*/}
            {/*}>*/}
              <Editor
                value={postContent}
                setValue={setPostContent}
                onSaveDraft={handleSaveDraft}
                onSubmitPost={handleSubmitPost}
              />
            </Suspense>
          </Col>
        </Row>
        <Row><Col lg={12}><br /></Col></Row>
        <Row>
          <Col lg={2}></Col>
          <Col lg={8}>
            <Suspense fallback={<></>}>
              <DraftsTable
                title="Your saved drafts"
                numSaves={numSaves}
                onLoadPost={handleLoadPost}
                onDeleteDraft={handleDeleteDraft}
                pageSize={10} />
            </Suspense>
          </Col>
        </Row>
        <Row><Col lg={12}><br /></Col></Row>
      </div>
    </div>
  );
}

ReactDOM.render(
  <Router>
    <Page path="/post/edit" />
  </Router>
  , document.getElementById("root"));