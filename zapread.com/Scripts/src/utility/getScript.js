/**
 * Dynamically fetches a script and loads it into the DOM
 * 
 * [✓] Native JS
 * 
 **/

// Used for caching scripts when loaded on-demand.
var LoadedScripts = new Array();
window.LoadedScripts = LoadedScripts;

///  https://stackoverflow.com/questions/16839698/jquery-getscript-alternative-in-native-javascript

/**
 * Dynamically load a script
 * 
 * [✓] Native JS
 * 
 * @param {any} url
 * @param {any} implementationCode
 * @param {any} cache
 */
export function getScript(url, implementationCode, cache) {
    //url is URL of external file, implementationCode is the code
    //to be called from the file, location is the location to 
    //insert the <script> element
    console.log('load: ' + url);

    var scriptTag = document.createElement('script');
    scriptTag.src = url;

    scriptTag.onload = implementationCode;
    scriptTag.onreadystatechange = implementationCode;

    document.body.appendChild(scriptTag);
};

//export function getScript(url, callback, cache) {
//    console.log('load: ' + url);
//    if (LoadedScripts.indexOf(url) !== -1) {
//    //if ($.inArray(url, LoadedScripts) > -1) {
//        callback();
//    }
//    else {
//        LoadedScripts.push(url);
//        fetch(url, {
//            method: 'GET', // *GET, POST, PUT, DELETE, etc.
//            //mode: 'same-origin', // no-cors, *cors, same-origin
//            cache: cache ? 'force-cache' : 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
//            credentials: 'omit', // include, *same-origin, omit
//        })
//        .then(() => {
//            console.log('fetched: ' + url);
//            callback();
//        });
//        //jQuery.ajax({
//        //    type: "GET",
//        //    url: url,
//        //    success: callback,
//        //    dataType: "script",
//        //    cache: cache
//        //});
//    }
//};