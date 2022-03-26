/*
 * Theme context provider
 * 
 * Based on: https://www.section.io/engineering-education/building-a-switchable-multi-color-theme-with-react/
 * 
 */
import React, { createContext, useState } from "react";

const ThemeColors = {
  primary: "brown",
  blue: "blue",
  red: "red",
  purple: "purple",
  orange: "orange",
  green: "green",
  white: "white"
};

export const ThemeColorContext = createContext({
  bgColor: ThemeColors.white,
  changeBgColor: (color) => { },
});

export default function ThemeColorWrapper(props) {
  const [color, setColor] = useState(ThemeColors.white);

  function changeColor(color) {
    setColor(color);
  }

  return (
    <ThemeColorContext.Provider
      value={{ color: color, changeColor: changeColor }}
    >
      {props.children}
    </ThemeColorContext.Provider>
  );
}