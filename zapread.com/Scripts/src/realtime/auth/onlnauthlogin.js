/**
 * Callback after user authenticated - go to the next step to access website
 * @param {any} callback
 * @param {any} token
 */
export function onlnauthlogin(callback, token) {
  appInsights.trackEvent({
    name: 'lnurl-auth login authenticated',
    properties: {
      pubkey: token
    }
  });

  // Go to callback with login
  var reqCallback = "/lnauth/callback";
  if (callback !== reqCallback) {
    // could be someone trying to hijack login - https://github.com/Horndev/zapread.com/issues/838
    appInsights.trackEvent({
      name: 'lnurl-auth login hijack attempt',
      properties: {
        pubkey: token
      }
    });
  }

  appInsights.flush(); // send now
  window.location.replace(reqCallback + "?code=" + token + "&state=" + state);
}