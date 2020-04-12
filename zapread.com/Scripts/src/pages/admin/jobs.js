/*
 * 
 */

import '../../shared/shared';
import '../../realtime/signalr';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';

import { getAntiForgeryToken } from '../../utility/antiforgery';

// Setup a job
export function setupJob(j) {
    var msg = JSON.stringify({
        jobid: j
    });

    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/Jobs/Install/",
        data: msg,
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                alert("Successfully started " + j);
            }
        },
        failure: function (response) {
            alert("Failure.");
        },
        error: function (response) {
            alert("Error.");
        }
    });
}
window.setupJob = setupJob;

export function removeJob(j) {
    var msg = JSON.stringify({
        jobid: j
    });
    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/Jobs/Remove/",
        data: msg,
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                alert("Successfully removed " + j);
            }
        },
        failure: function (response) {
            alert("Failure.");
        },
        error: function (response) {
            alert("Error.");
        }
    });
}
window.removeJob = removeJob;

export function runJob(j) {
    var msg = JSON.stringify({
        jobid: j
    });
    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/Jobs/Run/",
        data: msg,
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                alert("Successfully triggered " + j);
            }
        },
        failure: function (response) {
            alert("Failure.");
        },
        error: function (response) {
            alert("Error.");
        }
    });
}
window.runJob = runJob;