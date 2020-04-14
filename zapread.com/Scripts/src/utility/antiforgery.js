/*
 * 
 */

export function getAntiForgeryToken() {
    var form = document.querySelectorAll('#__AjaxAntiForgeryForm').item(0);
    var token = form.querySelectorAll('input[name="__RequestVerificationToken').item(0).getAttribute('value');
    var headers = {};
    headers['__RequestVerificationToken'] = token;
    return headers;
}

export function getAntiForgeryTokenValue() {
    var form = document.querySelectorAll('#__AjaxAntiForgeryForm').item(0);
    var token = form.querySelectorAll('input[name="__RequestVerificationToken').item(0).getAttribute('value');
    return token;
}