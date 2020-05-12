/**
 * 
 */

import { getScript } from '../getScript';       // [✓]

/**
 * [✓] Native JS
 **/
export function scan() {
    getScript('https://cdnjs.cloudflare.com/ajax/libs/webrtc-adapter/6.4.0/adapter.min.js', function () {
        getScript('/Scripts/instascan.min.js', function () {
            let scanner = new Instascan.Scanner({ video: document.getElementById('preview') });
            scanner.addListener('scan', function (content) {
                console.log(content);
                document.getElementById("lightningWithdrawInvoiceInput").value = content;// $('#lightningWithdrawInvoiceInput').val(content);
                document.getElementById("preview").style.display = "none";//$("#preview").hide();
                scanner.stop();
            });
            Instascan.Camera.getCameras().then(function (cameras) {
                if (cameras.length > 0) {
                    scanner.start(cameras[0]);
                } else {
                    console.error('No cameras found.');
                }
            }).catch(function (e) {
                console.error(e);
            });
            document.getElementById("preview").style.display = '';//$("#preview").show();
        }, true);
    }, true);
}
window.scan = scan;