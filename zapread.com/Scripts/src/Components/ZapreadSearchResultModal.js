/*
 * 
*/

import React, { useCallback, useEffect, useState, createRef } from "react";
import { InputGroup, FormControl, DropdownButton, Dropdown, Modal, Container, Row, Col, Button, Card } from "react-bootstrap";

export default function ZapreadSearchResultModal(props) {
  const [show, setShow] = useState(props.show);
  const [query, setQuery] = useState(props.query);
  const [results, setResults] = useState(props.results);

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
    },
    [props.query, props.show, props.results]
  );

  return (
    <Modal size="lg" id="search-modal" show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Modal.Title id="example-modal-sizes-title-lg">
          Search Results
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {results.map((result, index) => (
          <p key={result}>{result}</p>
        ))}
      </Modal.Body>
    </Modal>
  )
}