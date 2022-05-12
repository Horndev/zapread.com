/**
 * 
 **/

import React, { useCallback, useState, useRef } from 'react';           // [✓]
import ReactQuill, { Quill } from '../../../quill/react-quill/src/index';                        // [✓]
import Delta from 'quill-delta';
import { getAntiForgeryToken } from '../../../utility/antiforgery';     // [✓]
//import 'react-quill/dist/quill.snow.css';                               // [✓]
import '../../../css/quill/quilledit.css';                              // [✓]
import ImageResize from '../../../quill-image-resize-module/src/ImageResize';           // [✓] Import from source
import { ImageUpload } from '../../../quill/image-upload';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from '../../../quill/QuillImageDropAndPaste';
import Toolbar from '../../../quill/zr-toolbar';

Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste, true);
Quill.register('modules/imageUpload', ImageUpload, true);
Quill.register('modules/autoLinks', AutoLinks, true);
Quill.register('modules/imageResize', ImageResize, true);

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

const BlockEmbed = Quill.import("blots/block/embed");
const Link = Quill.import("formats/link");

class EmbedResponsive extends BlockEmbed {
  static blotName = "video";
  static tagName = "DIV";
  static className = "embed-responsive";

  static create(value) {
    const node = super.create(value);
    node.classList.add("embed-responsive-16by9");
    const child = document.createElement("iframe");
    child.setAttribute('frameborder', '0');
    child.setAttribute('allowfullscreen', true);
    child.setAttribute('src', this.sanitize(value));
    child.classList.add("embed-responsive-item");
    node.appendChild(child);
    return node;
  }

  static sanitize(url) {
    return Link.sanitize(url);
  }

  static value(domNode) {
    const iframe = domNode.querySelector('iframe');
    return iframe.getAttribute('src');
  }
}
Quill.register(EmbedResponsive, true);

var FontAttributor = Quill.import('attributors/class/font');
//console.log(FontAttributor.whitelist);
//console.log(FontAttributor);
FontAttributor.whitelist = [
  'serif', 'monospace', 'arial', 'calibri', 'courier', 'georgia', 'lucida',
  'open', 'roboto', 'tahoma', 'times', 'trebuchet', 'verdana'
];
Quill.register(FontAttributor, true);

Quill.register('modules/toolbar', Toolbar, true);

var icons = Quill.import('ui/icons');
icons['submit'] = '<i class="fa fa-check"></i> Submit';
icons['save'] = '<i class="fa fa-save"></i> Save';
Quill.register(icons,true);

window.change = new Delta();
window.editcontent = "";

export default class Editor extends React.Component {
  constructor(props) {
    super(props)
    this.quillRef = null;      // Quill instance
    this.reactQuillRef = null; // ReactQuill component
    this.formats = [
      'header',
      'bold', 'italic', 'underline', 'strike', 'blockquote',
      'list', 'bullet', 'indent',
      'link', 'image', 'video'
    ]

    var self = this;
    this.modules = {
      toolbar: {
        container: [
          ['submit', 'save'],
          [{ 'header': [1, 2, 3, false] }],
          ['bold', 'italic', 'underline', 'strike', 'blockquote', 'code-block'],
          [{
            'font': [
              'serif', 'monospace', 'arial', 'calibri', 'courier', 'georgia', 'lucida',
              'open', 'roboto', 'tahoma', /*'times',*/ 'trebuchet', 'verdana'
            ]
          }],
          [{ 'align': [] }],
          [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
          [{ color: [] }, { background: [] }],
          ['link', 'image', 'video'],
          ['clean']],
        handlers: {
          'save': function () {
            //console.log('save clicked');
            self.props.onSaveDraft();
          },
          'submit': function () {
            //console.log('submit clicked');
            self.props.onSubmitPost();
          }
        }
      },
      uploader: {
        mimetypes: [],
        handler: (range, files) => { } // Disable uploading with default module since we have the image drop and paste module
      },
      //videoResize: {
      //},
      imageUpload: {
        url: '/Img/UploadImage/', // server url. If the url is empty then the base64 returns
        method: 'POST', // change query method, default 'POST'
        name: 'file', // custom form name
        withCredentials: true, // withCredentials
        headers: getAntiForgeryToken(), // add custom headers, example { token: 'your-token'}
        customUploader: (file, dataUrl) => {
          this.sendImage(file);
        },
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
      imageDropAndPaste: {
        // add an custom image handler
        handler: this.imageHandler.bind(this)
      },
      autoLinks: true,
      imageResize: {},
    }

    window.handleSaveDraft = this.props.onSaveDraft;
    window.handleUpdateContent = this.props.setValue;
  }

  componentDidMount() {
    this.attachQuillRefs()

    var self = this;
    // Save periodically
    setInterval(function () {
      //console.log("save");
      if (window.change.length() > 0) {
        document.getElementById("savingnotification").style.display = "";
        self.props.onSaveDraft();
        window.change = new Delta();

        // Simulate save
        setTimeout(() => {
          document.getElementById("savingnotification").style.display = "none";
        }, 2000);
      }
    }, 10 * 1000);
  }

  componentDidUpdate() {
    this.attachQuillRefs()
  }

  attachQuillRefs() {
    if (typeof this.reactQuillRef.getEditor !== 'function') return;
    this.quillRef = this.reactQuillRef.getEditor();
  }

  /**
   * Upload a file with loading bar
   * @param {any} file
   */
  sendImage(file) {
    var fd = new FormData();
    fd.append('file', file);
    document.getElementById("progressUpload").style.display = "flex";
    document.getElementById("progressUploadBar").style.width = "1%";
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
        document.getElementById("progressUploadBar").style.width = percentComplete.toString() + "%";
        if (percentComplete === 100) {
          document.getElementById("progressUploadBar").style.width = "100%";
        }
      }
    }, false);

    // listen callback
    xhr.onload = () => {
      document.getElementById("progressUpload").style.display = "none";
      if (xhr.status === 200) {
        var data = JSON.parse(xhr.responseText);
        var index = (this.quillRef.getSelection() || {}).index || this.quillRef.getLength();
        if (index) {
          this.quillRef.insertEmbed(index, 'image', '/i/' + data.imgIdEnc, 'user');
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
  imageHandler(imageDataUrl, type, imageData) {
    var filename = 'pastedImage.png';
    var file = imageData.toFile(filename);
    this.sendImage(file);
  }

  handleChange(content, delta, source, editor) {
    window.editcontent = content;
    window.change = window.change.compose(delta);
    window.handleUpdateContent(content);
    //console.log(content);
    //console.log(delta);
  }

  render() {
    return (
      <>
        <div id="progressUpload" className="progress" style={{ display:"none" }}>
          <div id="progressUploadBar" className="progress-bar progress-bar-striped progress-bar-animated" style={{ width:"0%"}}>
          </div>
        </div>
        <ReactQuill
          ref={(el) => { this.reactQuillRef = el }}
          theme="snow"
          value={this.props.value}
          scrollingContainer="body"
          onChange={this.handleChange}
          placeholder={"Compose a great post..."}
          modules={this.modules}
        />
      </>
    )
  }
}