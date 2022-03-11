/*
 * 
*/

import "../css/components/zrsearch.css";

import React, { useCallback, useEffect, useState, createRef } from "react";
import { InputGroup, FormControl, DropdownButton, Dropdown, Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import { ISOtoRelative } from "../utility/datetime/posttime"

export default function ZapreadSearchResultModal(props) {
  const [show, setShow] = useState(props.show);
  const [query, setQuery] = useState(props.query);
  const [results, setResults] = useState(props.results);
  const [noResults, setNoResults] = useState(props.noResults);

  const handleClose = () => {
    // Cleanup & reset
    props.onClose();
  }

  // Monitor for changes in props
  useEffect(
    () => {
      setQuery(props.query);
      setShow(props.show);
      setResults(props.results);
      setNoResults(props.noResults);
    },
    [props.query, props.show, props.results, props.noResults]
  );

  return (
    <Modal size="lg" id="search-modal" show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Modal.Title id="example-modal-sizes-title-lg">
          Search Results
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <div style={results.length == 0 && !noResults ? {} : { display: "none" }}>
          <div className="sk-loading" style={{ BorderStyle: "none" }}>
            <div className="sk-spinner sk-spinner-three-bounce">
              <div className="sk-bounce1"></div>
              <div className="sk-bounce2"></div>
              <div className="sk-bounce3"></div>
            </div>
          </div>
        </div>
        {noResults ? <>No Results</> : <></>}
        {results.map((result, index) => (
          <>
            <div key={result.PostId + ":" + result.Id} className="search-result zr-search-hover" 
              onClick={() => {
                //window.location = '/p/' + result.EncPostId;
                window.open("/p/" + result.EncPostId, '_blank').focus();
              }}>
              {console.log(result)}
              <h3>{result.Type == "post" ? result.PostScore : result.CommentScore} <a href="#">{result.Title}</a></h3>
              <div className="search-link">{result.AuthorName}{" "}{result.Type == "post" ? <>posted</> : <>commented</>}{" "}in{" "}{result.GroupName}</div>
              <small className="text-muted">{ISOtoRelative(result.TimeStamp)}</small>
              <div
                dangerouslySetInnerHTML={{ __html: result.Content }}
                className="zr-search-preview"></div>
            </div>
            <div class="zr-search-hr"></div>
          </>
        ))}
      </Modal.Body>
    </Modal>
  )
}