using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Helpers
{
    public static class NumberHelpers
    {
        public static String ToAbbrString(this double number)
        {
            String ret = Convert.ToString(number);
            if (number > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (number > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (number > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";

            return ret;
        }

        public static String ToAbbrString(this int number)
        {
            String ret = Convert.ToString(number);
            if (number > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (number > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (number > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";

            return ret;
        }
    }
}