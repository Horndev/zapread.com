/**
 * Custom hook to manage user information and events
 *
 * Data is stored in a global object window.userInfo.
 *
 * usage is to update the userInfo object, and then emit the event zapread:updatedUserInfo,
 * causing the data to propigate to any subscribing components
 *
 * Built with inspiration using the tutorial here: https://nimblewebdeveloper.com/blog/using-custom-react-hooks-to-listen-to-dom-events
 */

import { useState, useEffect } from "react";

export function useUserInfo() {
  const [userInfo, setUserInfo] = useState(getUserInfo());

  function getUserInfo() {
    //console.log('getUserInfo');
    if (!("userInfo" in window)) {
      //console.log('userInfo not in window');
      return {
        balance: 0,
        spendOnlyBalance: 0,
        quickVote: false,
        quickVoteAmount: 10
      }; // initialize
    }
    //console.log('userInfo is in window', window.userInfo);
    return window.userInfo; // return from global
  }

  useEffect(() => {
    function handleUpdatedUserInfo() {
      var newUserInfo = getUserInfo();
      setUserInfo(newUserInfo);
      //console.log('handle zapread:updatedUserInfo', newUserInfo);
    }

    //console.log('register zapread:updatedUserInfo');
    document.addEventListener('zapread:updatedUserInfo', handleUpdatedUserInfo);

    return () => {
      document.removeEventListener('zapread:updatedUserInfo', handleUpdatedUserInfo);
    } // Removes listener on unmount
  }, []);

  return userInfo;
}