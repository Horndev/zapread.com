import { Quill } from 'react-quill';                        // [✓]

const Link = Quill.import("formats/link");
import { EmbedResponsive } from './EmbedResponsive';

const ATTRIBUTES = [
  'height',
  'width'
];

class Video extends EmbedResponsive {
  static create(value) {
    let node = super.create(value);
    node.setAttribute('frameborder', '0');
    node.setAttribute('allowfullscreen', true);
    node.setAttribute('src', this.sanitize(value));
    return node;
  }

  static formats(domNode) {
    return ATTRIBUTES.reduce(function(formats, attribute) {
      if (domNode.hasAttribute(attribute)) {
        formats[attribute] = domNode.getAttribute(attribute);
      }
      return formats;
    }, {});
  }

  static sanitize(url) {
    return Link.sanitize(url);
  }

  static value(domNode) {
    return domNode.getAttribute('src');
  }

  format(name, value) {
    if (ATTRIBUTES.indexOf(name) > -1) {
      if (value) {
        this.domNode.setAttribute(name, value);
      } else {
        this.domNode.removeAttribute(name);
      }
    } else {
      super.format(name, value);
    }
  }
}
Video.blotName = 'video';
Video.className = 'ql-video';
Video.tagName = 'DIV';


export default VideoResponsive;
