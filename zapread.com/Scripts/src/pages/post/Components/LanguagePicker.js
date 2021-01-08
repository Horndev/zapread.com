/**
 * 
 */

import React, { useCallback, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import { postJson } from '../../../utility/postData';
import '../../../css/components/autosuggest.css'
import '../../../css/components/input/input.css'

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.Name;

// Use your imagination to render suggestions.
function renderSuggestion(suggestion, { query }) {
    return (
        <div>
            <span style={{
                paddingLeft: "10px"
            }}>
                {suggestion.Name}
            </span>
        </div>
    )
};

function shouldRenderSuggestions() {
    return true;
}

export default class LanguagePicker extends React.Component {
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
        this.props.setValue(newValue);
    };

    loadSuggestions(value) {
        // Cancel the previous request
        if (this.lastRequestId !== null) {
            clearTimeout(this.lastRequestId);
        }

        this.setState({
            isLoading: true
        });

        postJson("/user/languages/", {
            prefix: value,
            max: 10
        }).then((response) => {
            console.log(response);

            this.setState({
                isLoading: false
            });

            this.setState({
                suggestions: response.languages
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
            placeholder: label,
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
                    highlightFirstSuggestion={true}
                    inputProps={inputProps}
                />
                <label htmlFor={1} className={error && "error"}>
                    {error || label}
                </label>
            </div>
        );
    }
}

// placeholder = &#xf1ab