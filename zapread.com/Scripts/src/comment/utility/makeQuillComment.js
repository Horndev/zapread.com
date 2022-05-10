/**
 * This file contains the common functions for commenting using Quill
 **/

import 'quill/dist/quill.core.css'
import 'quill/dist/quill.snow.css'
import '../../css/quill/quilledit.css';                              // [✓]
import '../../css/quill/quillcustom.css'; // Some custom overrides

const getSwal = () => import('sweetalert2');
import Quill from 'quill';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { postData } from '../../utility/postData';
import Mention from '../../quill/quill-mention/src/quill.mention';
import ImageResize from '../../quill-image-resize-module/src/ImageResize';
import { ImageUpload } from 'quill-image-upload';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from 'quill-image-drop-and-paste';

Quill.register({
  'modules/imageUpload': ImageUpload,
  'modules/autoLinks': AutoLinks,
  'modules/mention': Mention,
  'modules/imageResize': ImageResize,
  'modules/imageDropAndPaste': QuillImageDropAndPaste
}, true); // import with warning suppression (i.e. overwriting existing function)

const BaseImage = Quill.import('formats/image');

const ATTRIBUTES = [
  'alt',
  'height',
  'width',
  'style'
];

const WHITE_STYLE = [
  'margin',
  'margin-left',
  'margin-right',
  'display',
  'float'
];

class StyledImage extends BaseImage {
  static formats(domNode) {
    return ATTRIBUTES.reduce(function (formats, attribute) {
      if (domNode.hasAttribute(attribute)) {
        formats[attribute] = domNode.getAttribute(attribute);
      }
      return formats;
    }, {});
  }

  format(name, value) {
    if (ATTRIBUTES.indexOf(name) > -1) {
      if (value) {
        if (name === 'style') {
          value = this.sanitize_style(value);
        }
        this.domNode.setAttribute(name, value);
      } else {
        this.domNode.removeAttribute(name);
      }
    } else {
      super.format(name, value);
    }
  }

  sanitize_style(style) {
    let style_arr = style.split(";")
    let allow_style = "";
    style_arr.forEach((v, i) => {
      if (WHITE_STYLE.indexOf(v.trim().split(":")[0]) !== -1) {
        allow_style += v + ";"
      }
    })
    return allow_style;
  }
}
StyledImage.className = 'img-post';
Quill.register('formats/image', StyledImage, true);

//var BaseImage = Quill.import('formats/image');
//BaseImage.className = 'img-post';
//Quill.register(BaseImage, true);

//Quill.register('formats/image', Image, true);

var toolbarOptions = {
  container: [
    ['submit', 'cancel'],
    ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
    ['blockquote'/*, 'code-block'*/],
    //[{ 'header': 1 }, { 'header': 2 }],               // custom button values
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    //[{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
    //[{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
    //[{ 'direction': 'rtl' }],                         // text direction
    //[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
    //[{ 'header': [1, 2, 3, 4, 5, 6, false] }],
    [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
    ['link', 'image' /*,'video'*/ /*,'formula'*/],          // add's image support
    //[{ 'font': [] }],
    //[{ 'align': [] }],
    ['clean']                                         // remove formatting button
  ],
  handlers: {
    'cancel': function () { },  //dummy - will be updated
    'submit': function () { }   //dummy - will be updated
  }
};

var icons = Quill.import('ui/icons');
icons['submit'] = '<i class="fa fa-save"></i> Submit';
icons['cancel'] = '<i class="fa fa-times"></i> Cancel';

export function makeQuillComment(options) {
  var quill = new Quill('#' + options.selector, {
    modules: {
      imageUpload: {
        url: '/Img/UploadImage/', // server url. If the url is empty then the base64 returns
        method: 'POST', // change query method, default 'POST'
        name: 'file', // custom form name
        withCredentials: true, // withCredentials
        headers: getAntiForgeryToken(), // add custom headers, example { token: 'your-token'}
        customUploader: (file, dataUrl) => {
          sendImage(file);
        },
        //callbackOK: (serverResponse, insertURL) => {
        //  var index = (quill.getSelection() || {}).index || quill.getLength();
        //  if (index) {
        //    quill.insertEmbed(index, 'image', '/i/' + serverResponse.imgIdEnc, 'user');
        //  }
        //  //insertURL('/Img/Content/' + serverResponse.imgId + '/');//serverResponse);
        //},
        // personalize failed callback
        callbackKO: serverError => {
          alert(serverError);
        },
        // optional
        // add callback when a image have been chosen
        checkBeforeSend: (file, next) => {
          //console.log(file);
          next(file); // go back to component and send to the server
        }
      },
      uploader: {
        mimetypes: [],
        handler: (range, files) => { } // Disable uploading with default module since we have the image drop and paste module
      },
      imageDropAndPaste: {
        // add an custom image handler
        handler: imageHandler
      },
      autoLinks: true,
      imageResize: {},
      //imageRotate: {},
      mention: {
        minChars: 1,
        onSelect: function onSelect(item, insertItem) {
          item.value = `<span class='userhint'>${item.value}</span>`;
          insertItem(item);
        },
        allowedChars: /^[A-Za-z\sÅÄÖåäö]*$/,
        mentionDenotationChars: ["@"],
        source: async function (searchTerm, renderList) {
          const matchedUsers = await suggestUsers(searchTerm);
          renderList(matchedUsers);
        }
      },
      toolbar: toolbarOptions
    },
    placeholder: 'Write a comment...',
    theme: 'snow'
  });

  //quill.clipboard.dangerouslyPasteHTML(options.content);
  if (typeof (options.content) !== 'undefined') {
    quill.container.firstChild.innerHTML = options.content
  }
  quill.focus();
  quill.setSelection(9999);

  /**
   * Upload a file with loading bar
   * @param {any} file
   */
  function sendImage(file) {
    var fd = new FormData();
    fd.append('file', file);
    var uploadEl = document.getElementById("progressUpload");
    if (uploadEl != null) {
      uploadEl.style.display = "flex";
    }
    var uploadBarEl = document.getElementById("progressUploadBar");
    if (uploadBarEl != null) {
      uploadBarEl.style.width = "1%";
    }
    // upload image
    const xhr = new XMLHttpRequest();
    // init http query
    xhr.open('POST', '/Img/UploadImage/', true);
    // add custom headers
    var headers = getAntiForgeryToken();
    for (var index in headers) {
      xhr.setRequestHeader(index, headers[index]);
    }

    // progress bar
    xhr.upload.addEventListener("progress", function (evt) {
      if (evt.lengthComputable) {
        var percentComplete = evt.loaded / evt.total;
        percentComplete = parseInt(percentComplete * 100);
        if (uploadBarEl != null) {
          document.getElementById("progressUploadBar").style.width = percentComplete.toString() + "%";
        }
        if (percentComplete === 100) {
          if (uploadBarEl != null) {
            document.getElementById("progressUploadBar").style.width = "100%";
          }
        }
      }
    }, false);

    // listen callback
    xhr.onload = () => {
      if (uploadEl != null) {
        document.getElementById("progressUpload").style.display = "none";
      }
      if (xhr.status === 200) {
        var data = JSON.parse(xhr.responseText);
        var index = (quill.getSelection() || {}).index || quill.getLength();
        if (index) {
          quill.insertEmbed(index, 'image', '/i/' + data.imgIdEnc, 'user');
        } else {
          console.log({
            code: xhr.status,
            type: xhr.statusText,
            body: xhr.responseText
          });
        }
      };
    }
    xhr.send(fd);
  }

  /**
  * Do something to our dropped or pasted image
  * @param.imageDataUrl {string} - image's dataURL
  * @param.type {string} - image's mime type
  * @param.imageData {object} - provided more functions to handle the image
  *   - imageData.toBlob() {function} - convert image to a BLOB Object
  *   - imageData.toFile(filename) {function} - convert image to a File Object
  *   - imageData.minify(options) {function)- minify the image, return a promise
  *      - options.maxWidth {number} - specify the max width of the image, default is 800
  *      - options.maxHeight {number} - specify the max width of the image, default is 800
  *      - options.quality {number} - specify the quality of the image, default is 0.8
  */
  function imageHandler(imageDataUrl, type, imageData) {
    var filename = 'pastedImage.png'
    var file = imageData.toFile(filename)
    sendImage(file);
  }

  // configure toolbar
  var toolbar = quill.getModule('toolbar');
  // rename container id to allow multiple comment boxes (UX)
  toolbar.container.id = toolbar.container.id + options.uid;

  // cancel button clicked
  var cancelButton = document.querySelector('.ql-cancel');
  cancelButton.addEventListener('click', options.cancelCallback);

  // submit button clicked
  var submitButton = document.getElementById(options.uid).querySelectorAll('.ql-submit').item(0) // button for this post
  // var submitButton = document.querySelector('.ql-submit'); // This version doesn't work when there are multiple boxes
  submitButton.addEventListener('click', function () {
    options.preSubmitCallback();
    if (options.showloading) {
      showCommentLoadingById('s' + options.uid);
    }

    //This is the container element for the comment HTML
    var contentEl = document.getElementById(options.selector/*'editor-container' + options.uid*/).querySelectorAll('.ql-editor').item(0);
    var commentHTML = contentEl.innerHTML;

    options.submitCallback(commentHTML)
      .then((data) => {
        if (options.showloading) {
          hideCommentLoadingById('s' + options.uid);
        }
        if (data.success) {
          options.onSubmitSuccess(data);
        } else {
          // handle small error
        }
      })
      .catch((error) => {
        if (error instanceof Error) {
          hideCommentLoadingById('s' + options.uid);
          getSwal().then(({ default: Swal }) => {
            Swal.fire("Error", `Error submitting comment: ${error.message}`, "error");
          });
        }
        else {
          error.json().then(data => {
            hideCommentLoadingById('s' + options.uid);
            getSwal().then(({ default: Swal }) => {
              Swal.fire("Error", `Error submitting comment: ${data.message}`, "error");
            });
          })
        }
      })
      .finally(() => {
        // nothing?
        hideCommentLoadingById('s' + options.uid);
      });
  });
}

export async function suggestUsers(searchTerm) {
  var matchedUsers = [];
  var data = await postData("/Comment/Mentions/", {
    searchstr: searchTerm.toString() // not sure if toString is needed here...
  });
  matchedUsers = data.users;
  return matchedUsers;
}

function showCommentLoadingById(id) {
  var el = document.getElementById(id);
  el.querySelectorAll('.ibox-content').item(0).classList.add('sk-loading');
}

function hideCommentLoadingById(id) {
  var el = document.getElementById(id);
  if (el != null) {
    var loadingEl = el.querySelectorAll('.ibox-content').item(0);
    if (loadingEl != null) {
      loadingEl.classList.remove('sk-loading');
    }
  }
}