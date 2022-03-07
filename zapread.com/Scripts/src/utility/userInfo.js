/*
 * 
 */

/**
 * Helper function to update a set of keys in the userInfo global
 * @param {any} updated
 */
export function updateUserInfo(updated) {
  // Update global userInfo with balance
  if (!('userInfo' in window)) {
    window.userInfo = {
      balance: 0,
      defaultVote: 10
    };
  }

  window.userInfo = { ...window.UserInfo, ...updated };
  //window.userInfo.balance = data.balance;

  // Send event notifying updated
  const event = new Event('zapread:updatedUserInfo');
  document.dispatchEvent(event);
}