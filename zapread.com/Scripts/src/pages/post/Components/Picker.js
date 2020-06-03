/**
 * 
 */

import React, { useCallback, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import '../../../css/components/autosuggest.css'
import '../../../css/components/input.css'

// Imagine you have a list of languages that you'd like to autosuggest.
const groups = [
    {
        name: 'Community',
        year: 1972,
        img: '/Img/Group/Icon/1'
    },
    {
        name: 'Cats',
        year: 1972,
        img: '/Img/Group/Icon/4'
    },
    {
        name: 'Lightning',
        year: 1972,
        img: '/Img/Group/Icon/2'
    },
    {
        name: 'Bitcoin',
        year: 2012,
        img: '/Img/Group/Icon/3'
    }
];

// Teach Autosuggest how to calculate suggestions for any given input value.
const getSuggestions = value => {
    const inputValue = value.trim().toLowerCase();
    const inputLength = inputValue.length;

    return inputLength === 0 ? [] : groups.filter(lang =>
        lang.name.toLowerCase().slice(0, inputLength) === inputValue
    );
};

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.name;

// Use your imagination to render suggestions.
function renderSuggestion(suggestion, { query }) {

    return (
        <div>
            <img src={suggestion.img} width="30" height="30"></img>
            <span style={{
                paddingLeft: "10px"
            }}>
                {suggestion.name}
            </span>
        </div>
    )
};

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
        console.log('picker updated value: ' + this.props.value)
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

        // Fake request
        this.lastRequestId = setTimeout(() => {
            this.setState({
                suggestions: getSuggestions(value)
            });
        }, 1000);
    }

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {
        this.loadSuggestions(value);
        //this.setState({
        //    suggestions: getSuggestions(value)
        //});
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