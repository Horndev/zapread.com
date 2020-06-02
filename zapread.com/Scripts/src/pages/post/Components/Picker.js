/**
 * 
 */

import React, { useCallback, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import '../../../css/components/autosuggest.css'
import '../../../css/components/input.css'

// Imagine you have a list of languages that you'd like to autosuggest.
const languages = [
    {
        name: 'C',
        year: 1972
    },
    {
        name: 'Elm',
        year: 2012
    }
];

// Teach Autosuggest how to calculate suggestions for any given input value.
const getSuggestions = value => {
    const inputValue = value.trim().toLowerCase();
    const inputLength = inputValue.length;

    return inputLength === 0 ? [] : languages.filter(lang =>
        lang.name.toLowerCase().slice(0, inputLength) === inputValue
    );
};

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.name;

// Use your imagination to render suggestions.
const renderSuggestion = suggestion => (
    <div>
        {suggestion.name}
    </div>
);

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
            label: props.label || "Label"
        };
    }

    componentDidUpdate(prevProps) {
        this.updateValueIfNeeded(prevProps.value);
    }

    updateValueIfNeeded(prevValue) {
        if (this.props.value !== prevValue) {
            this.setState({ value: this.props.value });
        }
    }

    onChange = (event, { newValue }) => {
        this.setState({
            value: newValue
        });
        this.props.setValue(newValue);
    };

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {
        this.setState({
            suggestions: getSuggestions(value)
        });
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