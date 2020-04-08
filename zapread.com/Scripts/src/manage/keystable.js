import React, { useCallback, useEffect, useState } from 'react';
import Button from 'react-bootstrap/Button';
import Datatable from 'react-bs-datatable';
import Swal from 'sweetalert2'
import withReactContent from 'sweetalert2-react-content'

const MySwal = withReactContent(Swal)
//import moment from 'moment';

export default function KeysTable() {
    const [data, setData] = useState([]);
    const [filter, setFilter] = useState('');
    const [sortedProp, setSortedProp] = useState({
        prop: 'realname',
        isAscending: true
    });
    const [currentPage, setCurrentPage] = useState(1);
    const [rowsPerPage, setRowsPerPage] = useState(5);
    //const [maxPage, setMaxPage] = useState(1);

    //const customLabels = {
    //    first: '<<',
    //    last: '>>',
    //    prev: '<',
    //    next: '>',
    //    show: 'Display',
    //    entries: 'rows',
    //    noResults: 'There is no data to be displayed'
    //};

    async function fremoveKey(k) {
        const response = await fetch(`/api/v1/account/apikeys/revoke/${k}/`);
        const json = await response.json();
        if (json.success) {
            // Key was deleted
            const newData = data.filter(d => d.Key !== k);
            setData(newData);
            MySwal.fire({
                icon: "success",
                title: <p>Key successfully revoked.</p>
            });
        } else {
            MySwal.fire({
                icon: "error",
                title: <p>Error revoking key: {json.message}</p>
            });
        }
    }

    const header = [
        {
            title: 'Key',
            prop: 'Key',
            sortable: true,
            filterable: true
        },
        { title: 'Roles', prop: 'Roles', sortable: true, filterable: true },
        //{ title: 'Created', prop: 'Key', sortable: true },
        {
            title: 'Actions',
            prop: 'Key',
            cell: (row) => {
                const key = row.Key;
                return (
                    <>
                        <Button variant="outline-danger" size="sm" onClick={() => fremoveKey(key)}>
                            Revoke
                        </Button>
                    </> 
                );
            }
        }
    ];

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

    useEffect(() => {
        async function getKeys() {
            const response = await fetch('/api/v1/account/apikeys/list/');
            const json = await response.json();
            const newData = json.Keys;
            //const newMaxPage = Math.ceil(json.count / rowsPerPage);
            setData(newData);
            //setMaxPage(newMaxPage);
        }

        getKeys();
    }, [filter, sortedProp, currentPage, rowsPerPage]);

    return (
        <Datatable
            tableHeaders={header}
            tableBody={data}
            tableClass="striped hover responsive"
            //labels={customLabels}
            //rowsPerPageOption={[5, 10, 15, 20]}
            //async={{
            //    currentPage,
            //    filterText: filter,
            //    maxPage,
            //    onSort,
            //    onPaginate: onPageNavigate,
            //    onRowsPerPageChange,
            //    rowsPerPage,
            //    sortedProp: { prop: 'username', isAscending: true }
            //}}
        />
    );
}