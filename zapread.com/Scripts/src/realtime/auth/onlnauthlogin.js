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
  appInsights.flush(); // send now

  // Go to callback with login
  window.location.replace(callback + "?code=" + token + "&state=" + state);
}