/**
 * This file contains the common functions for commenting using Quill
 **/

import Swal from 'sweetalert2';
import Quill from 'quill';
import 'quill/dist/quill.core.css'
import 'quill/dist/quill.snow.css'
import '../../css/quill/quillcustom.css'; // Some custom overrides

import { getAntiForgeryToken } from '../../utility/antiforgery';
import { postData } from '../../utility/postData';

import 'quill-mention'; // This auto-registers
//import 'quill-mention/dist/quill.mention.css'  // Not importing since the styles are in quillcustom.css
import ImageResize from 'quill-image-resize-module';
import { ImageUpload } from 'quill-image-upload';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from 'quill-image-drop-and-paste';

Quill.register('modules/imageUpload', ImageUpload);
Quill.register('modules/autoLinks', AutoLinks);
Quill.register('modules/imageResize', ImageResize);
Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste)

var toolbarOptions = {
    container: [
        ['submit', 'cancel'],
        ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
        ['blockquote', 'code-block'],
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
    var quill = new Quill('#'+options.selector, {
        modules: {
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
    if (typeof (options.content) !== 'undefined')
    {
        quill.container.firstChild.innerHTML = options.content
    }
    quill.focus();
    quill.setSelection(9999);
    
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

        // generate a form data
        var fd = new FormData()
        fd.append('file', file)

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
                var index = (quill.getSelection() || {}).index || quill.getLength();
                if (index) {
                    quill.insertEmbed(index, 'image', '/Img/Content/' + data.imgId + '/', 'user');
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

    // configure toolbar
    var toolbar = quill.getModule('toolbar');
    // rename container id to allow multiple comment boxes (UX)
    toolbar.container.id = toolbar.container.id + options.uid;

    // cancel button clicked
    var cancelButton = document.querySelector('.ql-cancel');
    cancelButton.addEventListener('click', options.cancelCallback);

    // submit button clicked
    var submitButton = document.querySelector('.ql-submit');
    submitButton.addEventListener('click', function () {
        options.preSubmitCallback();
        if (options.showloading) {
            showCommentLoadingById('s' + options.uid);
        }

        //This is the container element for the comment HTML
        var contentEl = document.getElementById(options.selector/*'editor-container' + options.uid*/).querySelectorAll('.ql-editor').item(0);
        var commentHTML = contentEl.innerHTML;

        options.submitCallback(commentHTML).then((data) => {
            if (options.showloading) {
                hideCommentLoadingById('s' + options.uid);
            }
            if (data.success) {
                options.onSubmitSuccess(data);
            } else {
                // handle error
            }
        })
        .catch((error) => {
            Swal.fire("Error", `Error loading posts: ${error}`, "error");
        })
        .finally(() => {
            // nothing?
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
    el.querySelectorAll('.ibox-content').item(0).classList.remove('sk-loading');
}