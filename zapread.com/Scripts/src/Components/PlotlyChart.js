import React from "react";

import createPlotlyComponent from "react-plotly.js/factory";
import Plotly from "../plotly/plotly-custom";

const Plot = createPlotlyComponent(Plotly);

const CONFIG = {
  displaylogo: false,
  responsive: true
};
const FRAMES = [];

const ChartOrLoading = props => {
  let { data, layout } = props;
  return (
    <>
      {data && data.length > 0 ? (
        <Plot
          data={data}
          layout={layout}
          frames={FRAMES}
          config={CONFIG}
          useResizeHandler={true}
          style={{ width: "100%", height: "100%"}}
        // These 2 properties have effect only if we make the Plot component state-driven
        // as is described in the react-plotly documentation.
        // onInitialized={(figure) => this.setState(figure)}
        // onUpdate={(figure) => this.setState(figure)}
        />
      ) : (<></>)}
    </>
  );
}

export default function PlotlyChart(props) {
  return (
    <>
      <ChartOrLoading data={props.data} layout={props.layout} />
    </>
  );
}