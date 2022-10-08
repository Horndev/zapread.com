/**
 * Custom plotly module
 *
 * This is a custom module so that the entire plotly.js module doesn't
 * have to be loaded into the client.  Only parts used in the app are
 * included.
 */

import Plotly, { register } from "plotly.js/lib/core";

register([
  require("plotly.js/lib/scatter"),
  require("plotly.js/lib/bar")
]);

export default Plotly;
