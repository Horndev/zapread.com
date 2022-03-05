/*
 * Displays the User Balance in the top navbar
*/

import React, { useCallback, useEffect, useState, createRef } from "react";
import { useUserInfo } from "./hooks/useUserInfo";

export default function UserBalance(props) {
  const userInfo = useUserInfo(); // Custom hook

  return (
    <>
      {userInfo.balance}
    </>
  )
}