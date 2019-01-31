using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Helpers
{
    public static class NumberHelpers
    {
        public static string ToAbbrString(this double number)
        {
            string ret = Convert.ToString(number);
            if (Math.Abs(number) > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (Math.Abs(number) > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (Math.Abs(number) > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";
            return ret;
        }

        public static string ToAbbrString(this int number)
        {
            string ret = Convert.ToString(number);
            if (Math.Abs(number) > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (Math.Abs(number) > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (Math.Abs(number) > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";
            return ret;
        }

        public static string ToAbbrString(this long number)
        {
            string ret = Convert.ToString(number);
            if (Math.Abs(number) > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (Math.Abs(number) > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (Math.Abs(number) > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";
            return ret;
        }
    }
}