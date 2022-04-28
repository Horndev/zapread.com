/**
 * User vote functions - controlling modal and ui interface
 * 
 **/

/**
 * This should be IE compatible
 * @param {any} eventName
 */
function createNewEvent(eventName) {
  var event;
  if (typeof Event === 'function') {
    event = new Event(eventName);
  } else {
    event = document.createEvent('Event');
    event.initEvent(eventName, true, true);
  }
  return event;
}

var userVote = { id: 0, d: 0, t: 0, amount: 1, tx: 0, b: 0 };
var userTip = { username: "", amount: 1 };
var isTip = false;
var voteReadyEvent = createNewEvent('voteReady');

window.userVote = userVote;
window.userTip = userTip;
window.isTip = isTip;
window.voteReadyEvent = voteReadyEvent;