/*
 * 
 */
import '../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';
import Swal from 'sweetalert2';
import Quill from 'quill';
import 'quill/dist/quill.core.css'
import 'quill/dist/quill.snow.css'
import '../css/quill/quillcustom.css'; // Some custom overrides

import 'quill-mention'
//import 'quill-mention/dist/quill.mention.css'  // Not importing since the styles are in quillcustom.css

import ImageResize from 'quill-image-resize-module';
Quill.register('modules/imageResize', ImageResize);

//import { ImageUpload } from 'quill-image-upload';
//Quill.register('modules/imageUpload', ImageUpload);

//import QuillImageDropAndPaste from 'quill-image-drop-and-paste'
//Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste)

import { getAntiForgeryToken } from '../utility/antiforgery';
import { postData } from '../utility/postData';
import { applyHoverToChildren } from '../utility/userhover'
import { sendFile } from '../utility/sendfile';

var icons = Quill.import('ui/icons');
icons['submit'] = '<i class="fa fa-save"></i> Submit';
icons['cancel'] = '<i class="fa fa-times"></i> Cancel';

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

/**
 * Handle the loading of the comment reply into the DOM, and creation of
 * the text editor, submission, toolbars, etc.
 * 
 * @example <XX onclick="replyComment(1,5);" \>
 *
 * [X] Native JS implementation
 *
 * @param {number} commentId The invoice string
 * @param {number} postId New user's balance (if deposit)
 **/
export async function replyComment(commentId, postId) {
    var el = document.getElementById('c_reply_' + commentId.toString());
    el.style.display = '';

    var url = '/Comment/GetInputBox' + "/" + commentId.toString();
    fetch(url).then(data => data.text()).then(data => {
        el.innerHTML = data
    }).then(function () {
        var quill = new Quill('#editor-container_' + commentId.toString(), {
            modules: {
                //imageUpload: {
                //    url: '', // server url. If the url is empty then the base64 returns
                //    method: 'POST', // change query method, default 'POST'
                //    name: 'image', // custom form name
                //    withCredentials: true, // withCredentials
                //    headers: getAntiForgeryToken(), // add custom headers, example { token: 'your-token'}
                //    //csrf: { token: 'token', hash: '' }, // add custom CSRF
                //    //customUploader: () => { }, // add custom uploader
                //    // personalize successful callback and call next function to insert new url to the editor
                //    callbackOK: (serverResponse, next) => {
                //        next(serverResponse);
                //    },
                //    // personalize failed callback
                //    callbackKO: serverError => {
                //        alert(serverError);
                //    },
                //    // optional
                //    // add callback when a image have been chosen
                //    checkBeforeSend: (file, next) => {
                //        console.log(file);
                //        next(file); // go back to component and send to the server
                //    }
                //},
                imageResize: {},
                mention: {
                    minChars: 1,
                    onSelect: function onSelect(item, insertItem) {
                        item.value = `<span class='userhint'>${item.value}</span>`;
                        insertItem(item);
                    },
                    allowedChars: /^[A-Za-z\sÅÄÖåäö]*$/,
                    mentionDenotationChars: ["@"/*, "#"*/],
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

        // configure toolbar
        var toolbar = quill.getModule('toolbar');
        // rename container id to allow multiple comment boxes (UX)
        toolbar.container.id = toolbar.container.id + '_' + commentId.toString()

        // cancel button clicked
        var cancelButton = document.querySelector('.ql-cancel');
        cancelButton.addEventListener('click', function () {
            // remove the editor
            var replyEl = document.getElementById('c_reply_' + commentId.toString());
            replyEl.innerHTML = '';//.parentNode.removeChild(replyEl);
        });

        // submit button clicked
        var submitButton = document.querySelector('.ql-submit');
        submitButton.addEventListener('click', function () {
            showCommentLoadingById('sc_' + commentId.toString());

            //This is the container element for the comment HTML
            var contentEl = document.getElementById('editor-container_' + commentId.toString()).querySelectorAll('.ql-editor').item(0);
            var commentHTML = contentEl.innerHTML;

            // Submit comment
            postData("/Comment/AddComment/", {
                CommentContent: commentHTML,
                CommentId: commentId,
                PostId: postId,
                IsReply: true
            }).then((data) => {
                hideCommentLoadingById('sc_' + commentId.toString());
                if (data.success) {
                    // remove the editor
                    var replyEl = document.getElementById('c_reply_' + commentId.toString());
                    replyEl.parentNode.removeChild(replyEl);

                    // and replace with HTML
                    var commentsEl = document.getElementById('rcomments_' + commentId.toString());
                    commentsEl.innerHTML = data.HTMLString + commentsEl.innerHTML;

                    // If user inserted any at mentions - they become hoverable.
                    applyHoverToChildren(commentsEl, '.userhint');

                    // [ ] TODO: Make new comment quotable
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