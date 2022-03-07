/**
 * 
 * from https://www.falldowngoboone.com/blog/talk-to-your-react-components-with-custom-events/#fn1
 */

/**
 * 
 * @param {any} eventType
 * @param {any} listener
 */
function on(eventType, listener) {
  document.addEventListener(eventType, listener);
}

function off(eventType, listener) {
  document.removeEventListener(eventType, listener);
}

function once(eventType, listener) {
  on(eventType, handleEventOnce);

  function handleEventOnce(event) {
    listener(event);
    off(eventType, handleEventOnce);
  }
}

function trigger(eventType, data) {
  const event = new CustomEvent(eventType, { detail: data });
  document.dispatchEvent(event);
}

export { on, once, off, trigger };