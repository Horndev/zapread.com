import React, { useState } from "react";
import { postJson } from "../../utility/postData";

export default function UserSetting(props) {
  const [settingName, setSettingName] = useState(props.settingName);
  const [isActive, setIsActive] = useState(props.isActive);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaved, setIsSaved] = useState(false);

  const handleToggle = (value) => {
    //console.log("handleToggle", value);

    setIsLoading(true);
    postJson('/Manage/UpdateUserSetting', {
      setting: settingName,
      value: value
    }).then((response) => {
      if (response.success) {
        setIsLoading(false);
        setIsActive(value);
        setIsSaved(true);
      }
    });
  }

  return (
    <>
      <div className="switch">
        <div className="onoffswitch" >
          <input type="checkbox"
            id={settingName}
            checked={isActive}
            onChange={({ target: { value } }) => { }}
            className="onoffswitch-checkbox"
            onClick={() => handleToggle(!isActive)}
          />
          <label className="onoffswitch-label" htmlFor={settingName}>
            {isSaved ? (<>
              <i className="fa fa-check switch-spinner" style={{display:"initial"}}></i>
            </>) : (<>
                {isLoading ? (<>
                  <i className="fa fa-refresh fa-spin switch-spinner" style={{ display: "initial" }}></i>
              </>) : (<></>)}
            </>)}
            <span className="onoffswitch-inner"></span>
            <span className="onoffswitch-switch"></span>
          </label>
        </div>
      </div>
    </>);
}