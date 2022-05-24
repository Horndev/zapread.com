/**
 * Post editor using Quill
 * 
 * TODO
 * [ ] Embed images from more sources
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
import Mention from '../../../quill/quill-mention/src/quill.mention';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from '../../../quill/QuillImageDropAndPaste';
import Toolbar from '../../../quill/zr-toolbar';
import { suggestUsers } from '../../../Components/utility/suggestUsers';
import { suggestTags } from '../../../Components/utility/suggestTags';

import BaseTheme, { BaseTooltip } from 'quill/themes/base';
import { Range } from 'quill/core/selection';
import LinkBlot from 'quill/formats/link';
import Emitter from 'quill/core/emitter';
import merge from 'lodash.merge';

Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste, true);
Quill.register('modules/imageUpload', ImageUpload, true);
Quill.register('modules/mention', Mention, true);
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

// video embed updates
const Theme = Quill.import("themes/snow");


class SnowTooltip extends BaseTooltip {
  constructor(quill, bounds) {
    super(quill, bounds);
    this.preview = this.root.querySelector('a.ql-preview');
  }

  listen() {
    super.listen();
    this.root.querySelector('a.ql-action').addEventListener('click', event => {
      if (this.root.classList.contains('ql-editing')) {
        this.save();
      } else {
        this.edit('link', this.preview.textContent);
      }
      event.preventDefault();
    });
    this.root.querySelector('a.ql-remove').addEventListener('click', event => {
      if (this.linkRange != null) {
        const range = this.linkRange;
        this.restoreFocus();
        this.quill.formatText(range, 'link', false, Emitter.sources.USER);
        delete this.linkRange;
      }
      event.preventDefault();
      this.hide();
    });
    this.quill.on(
      Emitter.events.SELECTION_CHANGE,
      (range, oldRange, source) => {
        if (range == null) return;
        if (range.length === 0 && source === Emitter.sources.USER) {
          const [link, offset] = this.quill.scroll.descendant(
            LinkBlot,
            range.index,
          );
          if (link != null) {
            this.linkRange = new Range(range.index - offset, link.length());
            const preview = LinkBlot.formats(link.domNode);
            this.preview.textContent = preview;
            this.preview.setAttribute('href', preview);
            this.show();
            this.position(this.quill.getBounds(this.linkRange));
            return;
          }
        } else {
          delete this.linkRange;
        }
        this.hide();
      },
    );
  }

  show() {
    super.show();
    this.root.removeAttribute('data-mode');
  }

  save() {
    let { value } = this.textbox;
    switch (this.root.getAttribute('data-mode')) {
      case 'link': {
        const { scrollTop } = this.quill.root;
        if (this.linkRange) {
          this.quill.formatText(
            this.linkRange,
            'link',
            value,
            Emitter.sources.USER,
          );
          delete this.linkRange;
        } else {
          this.restoreFocus();
          this.quill.format('link', value, Emitter.sources.USER);
        }
        this.quill.root.scrollTop = scrollTop;
        break;
      }
      case 'video': {
        value = extractVideoUrl(value);
      } // eslint-disable-next-line no-fallthrough
      case 'formula': {
        if (!value) break;
        const range = this.quill.getSelection(true);
        if (range != null) {
          const index = range.index + range.length;
          this.quill.insertEmbed(
            index,
            this.root.getAttribute('data-mode'),
            value,
            Emitter.sources.USER,
          );
          if (this.root.getAttribute('data-mode') === 'formula') {
            this.quill.insertText(index + 1, ' ', Emitter.sources.USER);
          }
          this.quill.setSelection(index + 2, Emitter.sources.USER);
        }
        break;
      }
      default:
    }
    this.textbox.value = '';
    this.hide();
  }
}
SnowTooltip.TEMPLATE = [
  '<a class="ql-preview" rel="noopener noreferrer" target="_blank" href="about:blank"></a>',
  '<input type="text" data-formula="e=mc^2" data-link="https://quilljs.com" data-video="Embed URL">',
  '<a class="ql-action"></a>',
  '<a class="ql-remove"></a>',
].join('');

class ModTheme extends Theme {
  constructor(quill, options) {
    super(quill, options);
  }

  extendToolbar(toolbar) {
    toolbar.container.classList.add('ql-snow');
    this.buildButtons(toolbar.container.querySelectorAll('button'), icons);
    this.buildPickers(toolbar.container.querySelectorAll('select'), icons);
    this.tooltip = new SnowTooltip(this.quill, this.options.bounds);
    if (toolbar.container.querySelector('.ql-link')) {
      this.quill.keyboard.addBinding(
        { key: 'k', shortKey: true },
        (range, context) => {
          toolbar.handlers.link.call(toolbar, !context.format.link);
        },
      );
    }
  }
}
ModTheme.DEFAULTS = merge({}, BaseTheme.DEFAULTS, {
  modules: {
    toolbar: {
      handlers: {
        link(value) {
          if (value) {
            const range = this.quill.getSelection();
            if (range == null || range.length === 0) return;
            let preview = this.quill.getText(range);
            if (
              /^\S+@\S+\.\S+$/.test(preview) &&
              preview.indexOf('mailto:') !== 0
            ) {
              preview = `mailto:${preview}`;
            }
            const { tooltip } = this.quill.theme;
            tooltip.edit('link', preview);
          } else {
            this.quill.format('link', false);
          }
        },
      },
    },
  },
});

function extractVideoUrl(url) {
  // Youtube
  let match =
    url.match(/^(?:(https?):\/\/)?(?:(?:www|m)\.)?youtube\.com\/watch.*v=([a-zA-Z0-9_-]+)/) ||
    url.match(/^(?:(https?):\/\/)?(?:(?:www|m)\.)?youtu\.be\/([a-zA-Z0-9_-]+)/);

  if (match) {
    return `${match[1] || 'https'}://www.youtube.com/embed/${match[2]
      }?showinfo=0`;
  }

  // Odysee
  match = url.match(/^(?:(https?):\/\/)?(?:(?:www|m)\.)?odysee\.com\/.*\/([a-zA-Z0-9_-]+)[:]/,)

  if (match) {
    return `${match[1] || 'https'}://odysee.com/$/embed/${match[2]}`;
  }

  // Vimeo
  match = url.match(/^(?:(https?):\/\/)?(?:www\.)?vimeo\.com\/(\d+)/);
  if (match) {
    return `${match[1] || 'https'}://player.vimeo.com/video/${match[2]}/`;
  }
  return url;
}

// override function
Quill.register("themes/zapread", ModTheme, true);

var FontAttributor = Quill.import('attributors/class/font');
FontAttributor.whitelist = [
  'serif', 'monospace', 'arial', 'calibri', 'courier', 'georgia', 'lucida',
  'open', 'roboto', 'tahoma', 'times', 'trebuchet', 'verdana'
];
Quill.register(FontAttributor, true);

Quill.register('modules/toolbar', Toolbar, true);

var icons = Quill.import('ui/icons');
icons['submit'] = '<i class="fa fa-check"></i> Submit';
icons['save'] = '<i class="fa fa-save"></i> Save';
Quill.register(icons, true);

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
              'open', 'roboto', 'tahoma', 'trebuchet', 'verdana'
            ]
          }],
          [{ 'align': [] }],
          [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
          [{ color: [] }, { background: [] }],
          ['link', 'image', 'video'],
          ['clean']],
        handlers: {
          'save': function () {
            self.props.onSaveDraft();
          },
          'submit': function () {
            self.props.onSubmitPost();
          }
        }
      },
      uploader: {
        mimetypes: [],
        handler: (range, files) => { } // Disable uploading with default module since we have the image drop and paste module
      },
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
          next(file); // go back to component and send to the server
        }
      },
      imageDropAndPaste: {
        // add an custom image handler
        handler: this.imageHandler.bind(this)
      },
      mention: {
        minChars: 1,
        onSelect: function onSelect(item, insertItem, mentionChar) {
          if (mentionChar === "@") {
            item.value = `<span class='userhint user-mention'>${item.value}</span>`;
          }
          if (mentionChar === "#") {
            item.value = `<span class='taghint'>${item.value}</span>`;
          }

          insertItem(item);
        },
        allowedChars: /^[A-Za-z\sÅÄÖåäö]*$/,
        mentionDenotationChars: ["@", "#"],
        dataAttributes: ["id", "value", "denotationChar", "link", "target", "disabled", "newtag"],
        source: async function (searchTerm, renderList, mentionChar) {
          if (mentionChar === "@") {
            const matchedUsers = await suggestUsers(searchTerm);
            renderList(matchedUsers, searchTerm);
          }

          if (mentionChar === "#") {
            const matchedTags = await suggestTags(searchTerm);
            renderList(matchedTags, searchTerm);
          }
        }
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
        <div id="progressUpload" className="progress" style={{ display: "none" }}>
          <div id="progressUploadBar" className="progress-bar progress-bar-striped progress-bar-animated" style={{ width: "0%" }}>
          </div>
        </div>
        <ReactQuill
          ref={(el) => { this.reactQuillRef = el }}
          theme="zapread"
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