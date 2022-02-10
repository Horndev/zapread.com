/**
 * 
 * 
 * From: https://github.com/contentco/quill-snow-sticky-toolbar
 */
import ResizeSensor from 'css-element-queries/src/ResizeSensor.js';

class StickyToolbar {
    constructor(quill, props) {
        console.log("StickyToolbar Loaded");

        this.quill = quill;
        this.toolbar = quill.getModule('toolbar');
    	let obj = this.toolbar.container;
    	let editorParent = obj.parentNode.parentNode.parentNode;//ql-toolbar=>div=>ng-quill=>div
    	let objTop = editorParent.getBoundingClientRect().top;
        window.addEventListener('scroll', function (evt) { 
            console.log(evt);
            //if (evt.target) {
            //    if (evt.target.classList.contains('resize-sensor-expand') || evt.target.classList.contains('resize-sensor-shrink')) {
            //        return;
            //    }
            //}
            console.log(obj);
            let distanceFromTop = evt.target.scrollTop;
            if (distanceFromTop > objTop - 40) {
                obj.style.position = "fixed";
                obj.style.top = (objTop - 1) + 'px';
                obj.style.width = editorParent.clientWidth + 'px';
                obj.style.background = "#FFF";
                obj.style.zIndex = "80";
            }
            else{
                obj.style.position = "relative";
                obj.style.width = editorParent.clientWidth + 'px';
                obj.style.top = '0px';
            }
        }, true);

        new ResizeSensor(editorParent, function() {
            obj.style.width = editorParent.clientWidth + 'px';
        });
    }
}
Quill.register('modules/sticky_toolbar', StickyToolbar);