/*
 * Autosuggest input for finding users
 */

import React, { useCallback, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import { postJson } from '../utility/postData';
import '../css/components/autosuggest.css';
import '../css/components/input/input.css';

export default function UserAutosuggest(props) {
  const [value, setValue] = useState(props.value || "");
  const [locked, setLocked] = useState(false);
  const [active, setActive] = useState((props.locked && props.active) || false);
  const [error, setError] = useState(props.error || "");
  const [label, setLabel] = useState(props.label || "Label");
  const [isLoading, setIsLoading] = useState(false);
  const [suggestions, setSuggestions] = useState([]);
  const [url, setUrl] = useState("/api/v1/user/search");
  const [userName, setUserName] = useState("");

  // Monitor for changes in props
  useEffect(
    () => {
      setUrl(props.url);
    },
    [props.url]
  );

  function loadSuggestions(value) {
    setIsLoading(true);

    postJson(url, {
      Prefix: value,
      Max: 10
    }).then((response) => {
      setIsLoading(false);
      //console.log("response", response);
      setSuggestions(response.Users);
    });
  }

  const onSuggestionsFetchRequested = ({ value }) => {
    loadSuggestions(value);
  };

  const onSuggestionsClearRequested = () => {
    setSuggestions([]);
  };

  const getSuggestionValue = suggestion => suggestion.UserName;

  function shouldRenderSuggestions() {
    return true;
  }

  function renderSuggestion(suggestion, { query }) {
    return (
      <div>
        <img className="img-circle" loading="lazy" width="30" height="30" 
          src={"/Home/UserImage/?size=30&UserId=" + encodeURIComponent(suggestion.UserAppId) + "&v=" + suggestion.ProfileImageVersion} />
        <span style={{
          paddingLeft: "10px"
        }}>
          {suggestion.UserName}
        </span>
      </div>
    )
  };

  const onChange = (event, { newValue }) => {
    setValue(newValue);

    var AppId = "";
    var suggestionData = suggestions.filter((x) => x.UserName.trim() == newValue.trim());
    if (suggestionData.length) {
      AppId = suggestionData[0].UserAppId;
    }

    // pass to parent
    props.onSelected({
      userName: newValue,
      UserAppId: AppId
    });
  };

  const inputProps = {
    placeholder: label,//'Type a programming language',
    value,
    onChange: onChange
  };

  return (
    <>
      <div className={`field ${(locked ? active : active || value) &&
        "active"} ${locked && !active && "locked"}`}>
        <Autosuggest
          suggestions={suggestions}
          onSuggestionsFetchRequested={onSuggestionsFetchRequested}
          onSuggestionsClearRequested={onSuggestionsClearRequested}
          getSuggestionValue={getSuggestionValue}
          shouldRenderSuggestions={shouldRenderSuggestions}
          renderSuggestion={renderSuggestion}
          inputProps={inputProps}
        />
        <label htmlFor={1} className={error && "error"}>
          {error || label}
        </label>
      </div>
    </>
  )
}
