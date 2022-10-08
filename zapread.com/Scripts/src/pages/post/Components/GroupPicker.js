/**
 * 
 */

import React, { useCallback, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import { postJson } from '../../../utility/postData';
import '../../../css/components/autosuggest.css'
import '../../../css/components/input/input.css'

// Teach Autosuggest how to calculate suggestions for any given input value.
//async function getSuggestions(value) {
//    const inputValue = value.trim().toLowerCase();
//    const inputLength = inputValue.length;

//    if (inputLength === 0) {
//        return [];
//    }

//    postJson("/Group/GetGroups/", {
//        prefix: inputValue,
//        max: 10
//    }).then((response) => {
//        console.log(response);
//        return response;
//    });
//};

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.GroupName;

// Use your imagination to render suggestions.
function renderSuggestion(suggestion, { query }) {
  return (
    <div>
      {suggestion.Icon != null ? (<>
        <div className="forum-icon" style={{ marginRight: "0px", display: "inline" }}>
          <i className={"fa " + suggestion.Icon} style={{ marginTop: "0px" }} />
        </div>
      </>) : (<img src={"/Img/Group/Icon/" + suggestion.GroupId} width="30" height="30"></img>)}
      <span style={{
        paddingLeft: "10px"
      }}>
        {suggestion.GroupName}
      </span>
    </div>
  )
};

function shouldRenderSuggestions() {
  return true;
}

export default class Picker extends React.Component {
  constructor(props) {
    super(props);

    // Autosuggest is a controlled component.
    // This means that you need to provide an input value
    // and an onChange handler that updates this value (see below).
    // Suggestions also need to be provided to the Autosuggest,
    // and they are initially empty because the Autosuggest is closed.
    this.state = {
      value: props.value || "",
      suggestions: [],
      active: (props.locked && props.active) || false,
      error: props.error || "",
      label: props.label || "Label",
      isLoading: false
    };

    this.lastRequestId = null;
  }

  componentDidUpdate(prevProps) {
    this.updateValueIfNeeded(prevProps.value);
  }

  updateValueIfNeeded(prevValue) {
    if (this.props.value !== prevValue) {
      this.setState({ value: this.props.value });
    }
    //console.log('picker updated value: ' + this.props.value)
  }

  onChange = (event, { newValue }) => {
    this.setState({
      value: newValue
    });

    // get the group ID
    var groupId = 1; //fallback
    var suggestionData = this.state.suggestions.filter((x) => x.GroupName.trim() == newValue.trim());
    if (suggestionData.length) {
      groupId = suggestionData[0].GroupId;
    }

    this.props.setValue({
      groupName: newValue,
      groupId: groupId
    });
  };

  loadSuggestions(value) {
    // Cancel the previous request
    if (this.lastRequestId !== null) {
      clearTimeout(this.lastRequestId);
    }

    this.setState({
      isLoading: true
    });

    postJson("/Group/GetGroups/", {
      prefix: value,
      max: 10
    }).then((response) => {
      //console.log(response);

      this.setState({
        isLoading: false
      });

      this.setState({
        suggestions: response
      });
    });
  }

  // Autosuggest will call this function every time you need to update suggestions.
  onSuggestionsFetchRequested = ({ value }) => {
    this.loadSuggestions(value);
  };

  // Autosuggest will call this function every time you need to clear suggestions.
  onSuggestionsClearRequested = () => {
    this.setState({
      suggestions: []
    });
  };

  render() {
    const { value, suggestions, active, error, label } = this.state;
    const { predicted, locked } = this.props;

    // Autosuggest will pass through all these props to the input.
    const inputProps = {
      placeholder: label,//'Type a programming language',
      value,
      onChange: this.onChange
    };

    const fieldClassName = `field ${(locked ? active : active || value) &&
      "active"} ${locked && !active && "locked"}`;

    return (
      <div className={fieldClassName}>
        <Autosuggest
          suggestions={suggestions}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
          getSuggestionValue={getSuggestionValue}
          shouldRenderSuggestions={shouldRenderSuggestions}
          renderSuggestion={renderSuggestion}
          inputProps={inputProps}
        />
        <label htmlFor={1} className={error && "error"}>
          {error || label}
        </label>
      </div>
    );
  }
}