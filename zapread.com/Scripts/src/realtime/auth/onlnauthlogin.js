

export function onlnauthlogin(callback, token) {
  console.log("realtime callback: ", callback, token);

  window.location.replace(callback + "?code=" + token + "&state=" + state);
}