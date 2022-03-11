/*
 * Top Zapread Search
 */

import "../css/components/zrsearch.css";
import React, { useCallback, useEffect, useState, createRef } from "react";
import { InputGroup, FormControl, DropdownButton, Dropdown, Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import ZapreadSearchResultModal from "./ZapreadSearchResultModal";

export default function ZapreadSearch(props) {
  const [query, setQuery] = useState("");
  const [searchType, setSearchType] = useState("all");
  const [showSearch, setShowSearch] = useState(false);
  const [results, setResults] = useState(["Coming soon!"]);

  const handleSearch = () => {
    setShowSearch(true);
  };

  const onCloseModal = () => {
    setShowSearch(false);
  }

  return (
    <>
      <ZapreadSearchResultModal show={showSearch} onClose={onCloseModal} query={query} results={results} />
      <div className="ZRSearch">
        <InputGroup>
          <FormControl
            placeholder="Search"
            aria-label="Search"
            value={query}
            onChange={({ target: { value } }) => setQuery(value)} // Controlled input
          />
          <InputGroup.Append>
            <Button variant="outline-primary" onClick={ handleSearch }>
              <i className="fa-solid fa-note-sticky" style={searchType == "post" ? {} : { display: "none" }}></i>
              <i className="fa-solid fa-user" style={searchType == "user" ? {} : { display: "none" }}></i>
              <i className="fa-solid fa-magnifying-glass" style={searchType == "all" ? {} : { display: "none" }}></i>
            </Button>
          </InputGroup.Append>
          <DropdownButton
            as={InputGroup.Append}
            variant="outline-primary"
            title=""
            id="input-group-dropdown-search"
          >
            <Dropdown.Item href="#" onClick={() => setSearchType("all")}>
              {searchType == "all" ? <><i className="fa-solid fa-check"></i></> : <></>}{" "}Find Anything
            </Dropdown.Item>
            <Dropdown.Item href="#" onClick={() => setSearchType("post")}>
              {searchType == "post" ? <><i className="fa-solid fa-check"></i></> : <></>}{" "}Find Posts
            </Dropdown.Item>
            <Dropdown.Item href="#" onClick={() => setSearchType("user")}>
              {searchType == "user" ? <><i className="fa-solid fa-check"></i></> : <></>}{" "}Find Users
            </Dropdown.Item>
          </DropdownButton>
        </InputGroup>
      </div>
    </>
  )
}