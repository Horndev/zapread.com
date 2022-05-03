using System;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class NumberHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToAbbrString(this double number)
        {
            string ret = number.ToString("0.");
            if (Math.Abs(number) > 1000000000)
                ret = (number / 1000000000.0).ToString("0.#") + "G";
            else if (Math.Abs(number) > 1000000)
                ret = (number / 1000000.0).ToString("0.#") + "M";
            else if (Math.Abs(number) > 1000)
                ret = (number / 1000.0).ToString("0.#") + "K";
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
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