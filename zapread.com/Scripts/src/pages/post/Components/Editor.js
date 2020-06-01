/**
 * 
 **/

import React, { useCallback, useState, useRef } from 'react';           // [✓]
import ReactQuill, { Quill } from 'react-quill';                        // [✓]
import Delta from 'quill-delta';
import { getAntiForgeryToken } from '../../../utility/antiforgery';     // [✓]
import 'react-quill/dist/quill.snow.css';                               // [✓]
import '../../../css/quill/quilledit.css'

//import '../../../quill/StickyToolbar/quill-sticky-toolbar';
import ImageResize from '../../../quill-image-resize-module';          // [✓] Import from source
//import { ImageUpload } from 'quill-image-upload';
import { ImageUpload } from '../../../quill/image-upload';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from '../../../quill/QuillImageDropAndPaste';
//import VideoResize from '../../../quill/VideoResize/VideoResize';

import Button from 'react-bootstrap/Button';

Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste)
Quill.register('modules/imageUpload', ImageUpload);
Quill.register('modules/autoLinks', AutoLinks);
Quill.register('modules/imageResize', ImageResize);
//Quill.register('modules/videoResize', VideoResize);

var Image = Quill.import('formats/image');
Image.className = 'img-fluid';
Quill.register(Image, true);

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
Quill.register(EmbedResponsive);

var FontAttributor = Quill.import('attributors/class/font');
console.log(FontAttributor.whitelist);
console.log(FontAttributor);
FontAttributor.whitelist = [
    'serif', 'monospace', 'arial', 'calibri', 'courier', 'georgia', 'lucida',
        'open', 'roboto', 'tahoma', 'times', 'trebuchet', 'verdana'
];
Quill.register(FontAttributor, true);

var icons = Quill.import('ui/icons');
icons['submit'] = '<i class="fa fa-check"></i> Submit';
icons['save'] = '<i class="fa fa-save"></i> Save';

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
                        ] }],
                    [{ 'align': [] }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
                    [{ color: [] }, { background: [] }],
                    ['link', 'image', 'video'],
                    ['clean']],
                handlers: {
                    'save': function () {
                        var self = this;
                        self.props.onSaveDraft();
                        //console.log('save clicked');
                    },
                    'submit': function () {
                        
                        console.log('submit clicked');
                    }
            }},
            //videoResize: {
            //},
            imageUpload: {
                url: '/Img/UploadImage/', // server url. If the url is empty then the base64 returns
                method: 'POST', // change query method, default 'POST'
                name: 'file', // custom form name
                withCredentials: true, // withCredentials
                headers: getAntiForgeryToken(), // add custom headers, example { token: 'your-token'}
                //csrf: { token: 'token', hash: '' }, // add custom CSRF
                //customUploader: () => { }, // add custom uploader
                // personalize successful callback and call next function to insert new url to the editor
                callbackOK: (serverResponse, insertURL) => {
                    insertURL('/Img/Content/' + serverResponse.imgId + '/');//serverResponse);
                },
                // personalize failed callback
                callbackKO: serverError => {
                    alert(serverError);
                },
                // optional
                // add callback when a image have been chosen
                checkBeforeSend: (file, next) => {
                    console.log(file);
                    next(file); // go back to component and send to the server
                }
            },
            imageDropAndPaste: {
                // add an custom image handler
                handler: this.imageHandler
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
                /* 
                Send entire document
                $.post('/your-endpoint', { 
                  doc: JSON.stringify(quill.getContents())
                });
                */
                window.change = new Delta();

                // Simulate save
                setTimeout(() => {
                    document.getElementById("savingnotification").style.display = "none";
                }, 2000);

                //document.getElementById("savingnotification").style.display = "none";
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

        // generate a form data
        var fd = new FormData();
        fd.append('file', file);

        // upload image
        //postData('/Img/UploadImage/')
        const xhr = new XMLHttpRequest();
        // init http query
        xhr.open('POST', '/Img/UploadImage/', true);
        // add custom headers
        var headers = getAntiForgeryToken();
        for (var index in headers) {
            xhr.setRequestHeader(index, headers[index]);
        }

        // listen callback
        xhr.onload = () => {
            if (xhr.status === 200) {
                var data = JSON.parse(xhr.responseText);
                var index = (this.quill.getSelection() || {}).index || this.quill.getLength();
                if (index) {
                    this.quill.insertEmbed(index, 'image', '/Img/Content/' + data.imgId + '/', 'user');
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