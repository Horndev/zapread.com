/*
 * 
 */

import React, { useState, useEffect } from 'react';
import { Container, Row, Col } from "react-bootstrap";
import Swal from 'sweetalert2';

/*React Selectize*/
import { MultiSelect } from "react-selectize";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

import { postJson } from "../../utility/postData";
import UserSetting from "./UserSetting";

function SettingRow(props) {
  return (<>
    <Row>
      <Col className="px-1">
        <UserSetting settingName={props.settingName} isActive={props.isActive} />
      </Col>
      <Col>{ props.title }</Col>
    </Row>
  </>);
}

export default function CustomizationSettings(props) {
  const [settings, setSettings] = useState(null);
  const [knownLanguages, setKnownLanguages] = useState([]);
  const [languages, setLanguages] = useState([]);

  const handleLanguagesChanged = (values) => {
    console.log(values);
      var userlangs = values.map(v => v.value).join(',');
      console.log(userlangs);
      postJson('/Manage/UpdateUserLanguages', {
        languages: userlangs
      }).then(response => {
        if (response.success) {
          console.log('languages updated.');
        } else {
          // Did not work
          Swal.fire("Error updating: " + data.message, "error");
        }
      }).catch((error) => {
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        else {
          error.json().then(data => {
            Swal.fire("Error", `${data.message}`, "error");
          })
        }
      });
    setLanguages(values);
  }

  useEffect(() => {
    async function initialize() {
      await fetch("/api/v1/user/settings/notification").then(response => {
        return response.json();
      }).then(data => {
        setSettings(data.Settings);

        var langOpts = data.KnownLanguages.map((l) => {
          var value = l.split(':')[0];
          var label = l.split(':')[1];
          var selected = data.Languages.find(i => i == value) ? true : false;
          return { value, label, selected };
        });

        const uniqueLanguages = [...new Map(langOpts.map(item =>
          [item['value'], item])).values()];

        var selectedLanguages = uniqueLanguages.filter(l => l.selected)

        setKnownLanguages(uniqueLanguages);
        setLanguages(selectedLanguages);
      });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox-content profile-content">
        <h4>Customization</h4>
        <Container>

          {settings ? (
            <>
              <SettingRow title={"Use dark color theme"} settingName={"colorTheme"} isActive={settings.ColorTheme == "dark"} />
              <SettingRow title={"Hide online status"} settingName={"showOnline"} isActive={settings.ShowOnline} />
              <SettingRow title={"View posts in all languages"} settingName={"ViewAllLanguages"} isActive={settings.ViewAllLanguages} />
            </>
          ) : (<></>)}

          <Row>
            <Col lg={12}>
              <br />
              <h4>User Languages</h4>
              <MultiSelect style={{ width: "100%" }}
                options={knownLanguages}
                values={languages}
                onValuesChange={handleLanguagesChanged}
              />
            </Col>
          </Row>
        </Container>
      </div>
    </>
  );
}