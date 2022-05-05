/*
 * 
 * based on solution from: https://github.com/kensnyder/quill-image-resize-module/issues/10#issuecomment-381345361
 */

import { EmbedBlot } from 'quill/node_modules/parchment/dist/parchment';
import { sanitize } from 'quill/formats/link';

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

class Image extends EmbedBlot {
  static create(value) {
    const node = super.create(value);
    if (typeof value === 'string') {
      node.setAttribute('src', this.sanitize(value));
    }
    return node;
  }

  static formats(domNode) {
    return ATTRIBUTES.reduce(function (formats, attribute) {
      if (domNode.hasAttribute(attribute)) {
        formats[attribute] = domNode.getAttribute(attribute);
      }
      return formats;
    }, {});
  }

  static match(url) {
    return /\.(jpe?g|gif|png)$/.test(url) || /^data:image\/.+;base64/.test(url);
  }

  static register() {
    if (/Firefox/i.test(navigator.userAgent)) {
      setTimeout(() => {
        // Disable image resizing in Firefox
        document.execCommand('enableObjectResizing', false, false);
      }, 1);
    }
  }

  static sanitize(url) {
    return sanitize(url, ['http', 'https', 'data']) ? url : '//:0';
  }

  static value(domNode) {
    return domNode.getAttribute('src');
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

Image.blotName = 'image';
Image.tagName = 'IMG';
Image.className = 'img-post';

export default Image;