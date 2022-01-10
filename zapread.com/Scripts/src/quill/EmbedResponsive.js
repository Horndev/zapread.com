import { Quill } from 'react-quill';                        // [✓]

const BlockEmbed = Quill.import("blots/block/embed");
const Link = Quill.import("formats/link");

class EmbedResponsive extends BlockEmbed {
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
EmbedResponsive.blotName = "embed-responsive";
EmbedResponsive.tagName = "DIV";
EmbedResponsive.className = "embed-responsive";

Quill.register(EmbedResponsive);