﻿/**
 * 
 **/

import * as tata from 'tata-js'

export function onusermessage(content, reason, clickUrl) {
    if(/^(https?:\/\/|\/)/.test(clickUrl))
        clickUrl = "https://" + clickUrl;
    tata.info(reason, content, {
        position: "br",
        duration: 10000,
        onClick: function () {
            window.open(clickUrl, '_blank');
        }
    });
}
