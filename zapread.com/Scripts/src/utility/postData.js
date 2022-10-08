/**
 * 
 **/
import { getAntiForgeryTokenValue } from './antiforgery';

export async function postData(url = '', data = {}, abortSignal = null) {
  // Default options are marked with *

  // https://javascript.info/fetch-abort
  let signal = abortSignal ? abortSignal : new AbortController().signal;
  try {
    const response = await fetch(url, {
      signal: signal,
      method: 'POST', // *GET, POST, PUT, DELETE, etc.
      mode: 'same-origin', // no-cors, *cors, same-origin
      cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
      credentials: 'same-origin', // include, *same-origin, omit
      headers: {
        'Content-Type': 'application/json',
        '__RequestVerificationToken': getAntiForgeryTokenValue()
      },
      redirect: 'follow', // manual, *follow, error
      referrerPolicy: 'no-referrer', // no-referrer, *client
      body: JSON.stringify(data) // body data type must match "Content-Type" header
    });
    return response.json(); // parses JSON response into native JavaScript objects
  } catch (err) {
    if (err.name == 'AbortError') {
      // Handle aborted fetch
      return { success: false, abort: true };
    } else {
      // Rethrow
      throw err;
    }
  }
}

export async function postJson(url = '', data = {}, abortSignal = null) {
  try {
    let signal = abortSignal ? abortSignal : new AbortController().signal;
    return fetch(url, {
      signal: signal,
      method: 'POST', // *GET, POST, PUT, DELETE, etc.
      mode: 'same-origin', // no-cors, *cors, same-origin
      cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
      credentials: 'same-origin', // include, *same-origin, omit
      headers: {
        'Content-Type': 'application/json',
        '__RequestVerificationToken': getAntiForgeryTokenValue()
      },
      redirect: 'follow', // manual, *follow, error
      referrerPolicy: 'no-referrer', // no-referrer, *client
      body: JSON.stringify(data) // body data type must match "Content-Type" header
    })
      .then(response => {
        if (response.status >= 400 && response.status < 600) {
          throw response;
        }
        return response.json()
      });
  } catch (err) {
    console.log(err);
    if (err.name == 'AbortError') {
      // Handle aborted fetch
      return { success: false, abort: true };
    } else {
      // Rethrow
      throw err;
    }
  }
}