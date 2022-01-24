/*
 *
 * Based on https://imballinst.github.io/react-bs-datatable/?path=/story/advanced-guides--asynchronous-table
 *
 * See doc for Datatable: https://github.com/Imballinst/react-bs-datatable
 **/

import React, { useCallback, useEffect, useState } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";
import Datatable from "react-bs-datatable";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
import { postJson } from "../../../utility/postData";
import JoinLeaveButton from "./JoinLeaveButton";

import "../../../css/pages/groups/index.css";

const MySwal = withReactContent(Swal);

export default function GroupsTable(props) {
  const [data, setData] = useState([]);
  const [numRecords, setNumRecords] = useState(0);

  const [filter, setFilter] = useState("");
  const [sortedProp, setSortedProp] = useState({
    prop: "tier",
    isAscending: false
  });
  const [currentPage, setCurrentPage] = useState(1);
  const [currentPageFiltered, setCurrentPageFiltered] = useState(1);
  const [rowsPerPage, setRowsPerPage] = useState(props.pageSize);
  const [maxPage, setMaxPage] = useState(1);

  const onFilter = useCallback(text => {
    setFilter(text);
    setCurrentPage(1);
  }, []);

  const onSort = useCallback(nextProp => {
    setSortedProp(oldState => {
      const nextSort = { ...oldState };

      if (nextProp !== oldState.prop) {
        nextSort.prop = nextProp;
        nextSort.isAscending = true;
      } else {
        nextSort.isAscending = !oldState.isAscending;
      }

      return nextSort;
    });
  }, []);

  const onPageNavigate = useCallback(nextPage => {
    setCurrentPage(nextPage);
  }, []);

  const onRowsPerPageChange = useCallback(rowsPerPage => {
    setRowsPerPage(rowsPerPage);
  }, []);

  useEffect(
    () => {
      async function getData() {
        // pass along filter
        var start = (currentPage - 1) * rowsPerPage;
        var query = {
          Start: start,
          Length: rowsPerPage
        };
        if (filter != "") {
          query.Search = { Value: filter };
        }
        //console.log(query);
        await postJson("/api/v1/groups/list/", query).then(response => {
          var newData = response.data;
          //console.log(newData);
          setData(newData);
          setNumRecords(response.recordsTotal);
          setMaxPage(Math.ceil(response.recordsTotal / props.pageSize));
        });
      }
      //console.log("[debug] call getData");
      getData();
    },
    [sortedProp, filter, currentPage, rowsPerPage]
  );

  const classes = {
    table: "table table-hover",
    theadCol: "group-table-head",
    //tbodyRow: "",
    paginationOptsFormText: ""
  };

  const header = [
    {
      title: "Id",
      prop: "Id",
      sortable: false,
      filterable: false,
      headerStyle: (column, colIndex) => {
        return { width: "20px", textAlign: "center" };
      },
      cellProps: { className: "project-status" },
      cell: row => {
        if (row.Icon != null) {
          return (
            <>
              <div className="forum-icon">
                <i className={"fa " + row.Icon} />
              </div>
            </>
          );
        }
        return (
          <>
            <img src={`/Img/Group/Icon/${row.Id}/`} />
          </>
        );
      }
    },
    {
      title: "Name",
      prop: "Name",
      sortable: true,
      filterable: true,
      //cellProps: { className: "project-title" }, // [TODO] Not sure why this is not being applied to td
      cell: data => {
        return (
          <>
            <div className="project-title">
              <a href={`/Group/GroupDetail/${data.Id}`}> {data.Name} </a>
              {data.IsAdmin && (
                <i
                  className="fa fa-gavel text-primary"
                  data-toggle="tooltip"
                  data-placement="right"
                  title="Administrator"
                />
              )}
              {data.IsMod && (
                <i
                  className="fa fa-gavel text-success"
                  data-toggle="tooltip"
                  data-placement="right"
                  title="Moderator"
                />
              )}
              <br />
              <small>Created {data.CreatedddMMMYYYY} </small>
            </div>
          </>
        );
      }
    },
    {
      title: "Tags",
      prop: "Tags",
      sortable: true,
      filterable: false,
      cellProps: { className: "project-title" },
      cell: data => {
        return (
          <>
            {data.Tags.length ? (
              data.Tags.map(tag => (
                <span
                  className="badge badge-light"
                  style={{ marginLeft: "3px" }}
                >
                  {tag}
                </span>
              ))
            ) : (
              <> </>
            )}
            <br />
            <small>Tags</small>
          </>
        );
      }
    },
    {
      title: "Tier",
      prop: "Progress",
      sortable: false,
      filterable: false,
      cellProps: { className: "project-completion" },
      cell: data => {
        return (
          <>
            <small className="d-none d-md-block">Progress to next tier</small>
            <div className="progress progress-mini d-none d-md-block">
              <div
                className="progress-bar"
                style={{ width: data.Progress + "%" }}
              />
            </div>
          </>
        );
      }
    },
    {
      title: "Level",
      prop: "Level",
      sortable: true,
      filterable: false,
      cellProps: { className: "project-people" },
      cell: data => {
        return (
          <>
            <span className="views-number d-none d-sm-block">{data.Level}</span>
            <div className="d-none d-sm-block">
              <small>Tier</small>
            </div>
          </>
        );
      }
    },
    {
      title: "Members",
      prop: "NumMembers",
      sortable: true,
      filterable: false,
      cellProps: { className: "project-people" },
      cell: data => {
        return (
          <>
            <a href={"/Group/Members/" + data.Id} className="btn btn-link">
              <span
                className="views-number d-none d-sm-block"
                id={"group_membercount_" + data.Id}
              >
                {data.NumMembers}
              </span>
              <div className="d-none d-sm-block">
                <small>Members</small>
              </div>
            </a>
          </>
        );
      }
    },
    {
      title: "Posts",
      prop: "NumPosts",
      sortable: true,
      filterable: false,
      cellProps: { className: "project-people" },
      cell: data => {
        return (
          <>
            <span className="views-number d-none d-sm-block">
              {data.NumPosts}
            </span>
            <div className="d-none d-sm-block">
              <small>Posts</small>
            </div>
          </>
        );
      }
    },
    {
      title: "Actions",
      prop: "NumComments",
      sortable: false,
      filterable: false,
      cellProps: { className: "project-actions" },
      cell: data => {
        if (!data.IsLoggedIn) {
          return (
            <>
              <button className="btn btn-primary btn-sm" disabled>
                <i className="fa fa-user-plus" /> Join
              </button>
            </>
          );
        } else {
          return (
            <>
              <JoinLeaveButton isMember={data.IsMember} id={data.Id} />
            </>
          );
        }
      }
    }
  ];

  return (
    <div className="ibox float-e-margins">
      <div className="ibox-content">
        <Container fluid="md">
          <Row>
            <Col lg={12}>
              <div className="project-list">
                <Datatable
                  classes={classes}
                  tableHeaders={header}
                  tableBody={data}
                  labels={{ filterPlaceholder: "Search..." }}
                  async={{
                    currentPage,
                    filterText: filter,
                    maxPage,
                    onFilter,
                    onSort,
                    onPaginate: onPageNavigate,
                    onRowsPerPageChange,
                    rowsPerPage,
                    sortedProp: { prop: "Name", isAscending: true }
                  }}
                />
              </div>
            </Col>
          </Row>
        </Container>
      </div>
    </div>
  );
}
