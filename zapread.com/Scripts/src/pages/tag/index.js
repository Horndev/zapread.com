/**
 * Tags/Index
 *
 * [✓] No javascript
 */

import "../../shared/shared";
import "../../realtime/signalr";

import React, { useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../Components/PageHeading";
import Masonry, { ResponsiveMasonry } from "react-responsive-masonry"
import { useUserInfo } from "../../Components/hooks/useUserInfo";
import { updateUserInfo } from '../../utility/userInfo';
import { getJson } from "../../utility/getData";
import "../../shared/sharedlast";

function Page() {
  const userInfo = useUserInfo(); // Custom hook
  const [tags, setTags] = useState([]);

  async function loadTagsInfo() {
    getJson('/api/v1/tag/list').then((response) => {
      console.log(response);
      if (response.success) {
        setTags(response.Tags);
      }
    });
  };

  useEffect(() => {
    updateUserInfo({
      isAuthenticated: window.IsAuthenticated
    });
    async function initialize() {
      // Do this in parallel
      await Promise.all([loadTagsInfo()]);
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <PageHeading
        title="Tags"
        controller="Tags"
        method="Tags"
        function="List"
        topGroups={false}
      />
      <Row>
        <Col lg={12}>
          <div className="wrapper wrapper-content animated fadeInUp">
            <Row>
              <Col lg={2}></Col>
              <Col lg={8}>
                <div className="ibox">
                  <ResponsiveMasonry
                    gutter="5"
                    columnsCountBreakPoints={{ 350: 1, 750: 2, 900: 3 }}>
                    <Masonry>
                      {tags.map((tag, index) => (
                        <Card key={tag.id}>
                          <Card.Body>
                            <Card.Title>
                              <span className="mention tag-mention">
                                <a href={tag.link}>
                                  <span className="ql-mention-denotation-char">#</span>
                                  <span className="taghint">{tag.value}</span>
                                </a>
                              </span>
                            </Card.Title>
                            <Card.Subtitle className="mb-2 text-muted">
                              {tag.count} post{tag.count > 1 ? "s" : ""}
                            </Card.Subtitle>
                          </Card.Body>
                        </Card>
                      ))}
                    </Masonry>
                  </ResponsiveMasonry>
                </div>
              </Col>
              <Col lg={2}></Col>
            </Row>
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
