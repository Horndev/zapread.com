using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using NBitcoin;

/*  Bech32 code https://github.com/guillaumebonnot/bech32/blob/master/Bech32/Bech32Engine.cs
 *  
/* Copyright (c) 2017 Guillaume Bonnot and Palekhov Ilia
 * Based on the work of Pieter Wuille
 * Special Thanks to adiabat
 *                  
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

namespace zapread.com.Services
{
    /// <summary>
    /// Class for helpers to do encryption/decryption
    /// 
    /// Based on code from https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
    /// </summary>
    public static class CryptoService
    {
        private static readonly char[] alphabetEnc = new char[] {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_', '.', '~',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's','t','u','v','w','x','y','z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S','T','U','V','W','X','Y','Z',
        };

        /// <summary>
        /// Generate a new referral Code
        /// </summary>
        /// <returns></returns>
        public static string GetNewRefCode()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("/", "-").Replace("+", "_");
        }

        /// <summary>
        /// Encode a number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string IntIdToString(int number)
        {
            var numChars = alphabetEnc.Count();

            int dividend = number;
            int remainder;
            var digits = new List<int>();

            while (dividend > 0)
            {
                remainder = dividend % numChars;
                dividend /= numChars;
                digits.Add(remainder);
            }

            digits.Reverse();

            string encString = "";
            int i = 0;
            while(digits.Count() > i)
            {
                encString += alphabetEnc[digits[i]];
                i++;
            }

            return encString;
        }

        /// <summary>
        /// Decode a number
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static int StringToIntId(string encoded)
        {
            if (encoded == null) return 0;

            int val = 0;
            var numChars = alphabetEnc.Count();
            var numDigits = encoded.Length;
            int i = 0;
            int placeval = 1;
            while (numDigits > i)
            {
                var c = encoded[numDigits - i - 1]; //LSB first
                var n = Array.IndexOf(alphabetEnc, c);
                val += placeval * n;
                placeval *= numChars;
                i++;
            }

            return val;
        }

        /// <summary>
        /// Verify a
        /// </summary>
        /// <param name="pubKey"></param>
        /// <param name="hash"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool VerifyHashSignatureSecp256k1(string pubKey, string hash, string signature)
        {
            var secp256k1 = ECCurve.CreateFromValue("1.3.132.0.10");

            // signature is DER encoded -> convert to 64 byte array
            var sigBytes = HexStringToByteArray(signature);
            
            var p1len = sigBytes[3];
            var sigp1 = sigBytes.Skip(4).SkipWhile(b => b == 0).Take(32).ToArray(); // Remove any 0 padded bytes
            var p2len = sigBytes.Skip(4 + p1len + 1).Take(1).ToArray()[0];
            var sigp2 = sigBytes.Skip(4 + p1len + 2).SkipWhile(b => b == 0).Take(32).ToArray(); // Remove any 0 padded bytes
            var sig = sigp1.Concat(sigp2).ToArray();

            PubKey pk = new PubKey(HexStringToByteArray(pubKey));
            var pkBytes = pk.Decompress().ToBytes();

            using (var dsa = ECDsa.Create(new ECParameters
            {
                Curve = secp256k1,
                Q = new ECPoint
                {
                    // gets the {x,y} from the uncompressed public key
                    X = pkBytes.Skip(1).Take(32).ToArray(),
                    Y = pkBytes.Skip(33).ToArray(),
                }
            }))
            {
                var isValid = dsa.VerifyHash(HexStringToByteArray(hash), sig);

                return isValid;
            }
        }

        /// <summary>
        /// Convert a hexadecimal string to an array of bytes
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16]; // careful - for security should use different initialization vectors
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Base32.Base32Encoder.Encode(array); // Convert.ToBase64String(array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Base32.Base32Encoder.Decode(cipherText); // Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static class Bech32
        {
            // used for polymod
            private static readonly uint[] generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };

            // charset is the sequence of ascii characters that make up the bech32
            // alphabet.  Each character represents a 5-bit squashed byte.
            // q = 0b00000, p = 0b00001, z = 0b00010, and so on.
            private const string charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

            // icharset is a mapping of 8-bit ascii characters to the charset
            // positions.  Both uppercase and lowercase ascii are mapped to the 5-bit
            // position values.
            private static readonly short[] icharset =
            {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            15, -1, 10, 17, 21, 20, 26, 30, 7, 5, -1, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1
        };

            /// <summary>
            /// PolyMod takes a byte slice and returns the 32-bit BCH checksum.
            /// Note that the input bytes to PolyMod need to be squashed to 5-bits tall
            /// before being used in this function.  And this function will not error,
            /// but instead return an unsuable checksum, if you give it full-height bytes.
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public static uint PolyMod(byte[] values)
            {
                uint chk = 1;
                foreach (byte value in values)
                {
                    var top = chk >> 25;
                    chk = (chk & 0x1ffffff) << 5 ^ value;
                    for (var i = 0; i < 5; ++i)
                    {
                        if (((top >> i) & 1) == 1)
                        {
                            chk ^= generator[i];
                        }
                    }
                }
                return chk;
            }

            /// <summary>
            /// on error, data == null
            /// </summary>
            /// <param name="encoded"></param>
            /// <param name="hrp"></param>
            /// <param name="data"></param>
            public static void Decode(string encoded, out string hrp, out byte[] data)
            {
                byte[] squashed;
                DecodeSquashed(encoded, out hrp, out squashed);
                if (squashed == null)
                {
                    data = null;
                    return;
                }
                data = Bytes5to8(squashed);
            }

            // on error, data == null
            private static void DecodeSquashed(string adr, out string hrp, out byte[] data)
            {
                adr = CheckAndFormat(adr);
                if (adr == null)
                {
                    data = null; hrp = null; return;
                }

                // find the last "1" and split there
                var splitLoc = adr.LastIndexOf("1");
                if (splitLoc == -1)
                {
                    //Debug.WriteLine("1 separator not present in address");
                    data = null; hrp = null; return;
                }

                // hrp comes before the split
                hrp = adr.Substring(0, splitLoc);

                // get squashed data
                var squashed = StringToSquashedBytes(adr.Substring(splitLoc + 1));
                if (squashed == null)
                {
                    data = null; return;
                }

                // make sure checksum works
                if (!VerifyChecksum(hrp, squashed))
                {
                    //Debug.WriteLine("Checksum invalid");
                    data = null; return;
                }

                // chop off checksum to return only payload
                var length = squashed.Length - 6;
                data = new byte[length];
                Array.Copy(squashed, 0, data, 0, length);
            }

            // on error, return null
            private static string CheckAndFormat(string adr)
            {
                // make an all lowercase and all uppercase version of the input string
                var lowAdr = adr.ToLower();
                var highAdr = adr.ToUpper();

                // if there's mixed case, that's not OK
                if (adr != lowAdr && adr != highAdr)
                {
                    //Debug.WriteLine("mixed case address");
                    return null;
                }

                // default to lowercase
                return lowAdr;
            }

            private static bool VerifyChecksum(string hrp, byte[] data)
            {
                var values = HRPExpand(hrp).Concat(data).ToArray();
                var checksum = PolyMod(values);
                // make sure it's 1 (from the LSB flip in CreateChecksum
                return checksum == 1;
            }

            // on error, return null
            private static byte[] StringToSquashedBytes(string input)
            {
                byte[] squashed = new byte[input.Length];

                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    var buffer = icharset[c];
                    if (buffer == -1)
                    {
                        //Debug.WriteLine("contains invalid character " + c);
                        return null;
                    }
                    squashed[i] = (byte)buffer;
                }

                return squashed;
            }

            /// <summary>
            /// // we encode the data and the human readable prefix
            /// </summary>
            /// <param name="hrp"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public static string Encode(string hrp, byte[] data)
            {
                var base5 = Bytes8to5(data);
                if (base5 == null)
                    return string.Empty;
                return EncodeSquashed(hrp, base5);
            }

            // on error, return null
            private static string EncodeSquashed(string hrp, byte[] data)
            {
                var checksum = CreateChecksum(hrp, data);
                var combined = data.Concat(checksum).ToArray();

                // Should be squashed, return empty string if it's not.
                var encoded = SquashedBytesToString(combined);
                if (encoded == null)
                    return null;
                return hrp + "1" + encoded;
            }

            private static byte[] CreateChecksum(string hrp, byte[] data)
            {
                var values = HRPExpand(hrp).Concat(data).ToArray();
                // put 6 zero bytes on at the end
                values = values.Concat(new byte[6]).ToArray();
                //get checksum for whole slice

                // flip the LSB of the checksum data after creating it
                var checksum = PolyMod(values) ^ 1;

                byte[] ret = new byte[6];
                for (var i = 0; i < 6; i++)
                {
                    // note that this is NOT the same as converting 8 to 5
                    // this is it's own expansion to 6 bytes from 4, chopping
                    // off the MSBs.
                    ret[i] = (byte)(checksum >> (5 * (5 - i)) & 0x1f);
                }

                return ret;
            }

            // HRPExpand turns the human redable part into 5bit-bytes for later processing
            private static byte[] HRPExpand(string input)
            {
                var output = new byte[(input.Length * 2) + 1];

                // first half is the input string shifted down 5 bits.
                // not much is going on there in terms of data / entropy
                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    output[i] = (byte)(c >> 5);
                }

                // then there's a 0 byte separator
                // don't need to set 0 byte in the middle, as it starts out that way

                // second half is the input string, with the top 3 bits zeroed.
                // most of the data / entropy will live here.
                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    output[i + input.Length + 1] = (byte)(c & 0x1f);
                }
                return output;
            }

            private static string SquashedBytesToString(byte[] input)
            {
                string s = string.Empty;
                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    if ((c & 0xe0) != 0)
                    {
                        //Debug.WriteLine("high bits set at position {0}: {1}", i, c);
                        return null;
                    }
                    s += charset[c];
                }

                return s;
            }

            private static byte[] Bytes8to5(byte[] data)
            {
                return ByteSquasher(data, 8, 5);
            }

            private static byte[] Bytes5to8(byte[] data)
            {
                return ByteSquasher(data, 5, 8);
            }

            // ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes,
            // and vice versa.  It can operate on other widths but in this package only
            // goes 5 to 8 and back again.  It can return null if the squashed input
            // you give it isn't actually squashed, or if there is padding (trailing q characters)
            // when going from 5 to 8
            private static byte[] ByteSquasher(byte[] input, int inputWidth, int outputWidth)
            {
                int bitstash = 0;
                int accumulator = 0;
                List<byte> output = new List<byte>();
                var maxOutputValue = (1 << outputWidth) - 1;

                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    if (c >> inputWidth != 0)
                    {
                        //Debug.WriteLine("byte {0} ({1}) high bits set", i, c);
                        return null;
                    }
                    accumulator = (accumulator << inputWidth) | c;
                    bitstash += inputWidth;
                    while (bitstash >= outputWidth)
                    {
                        bitstash -= outputWidth;
                        output.Add((byte)((accumulator >> bitstash) & maxOutputValue));
                    }
                }

                // pad if going from 8 to 5
                if (inputWidth == 8 && outputWidth == 5)
                {
                    if (bitstash != 0)
                    {
                        output.Add((byte)(accumulator << (outputWidth - bitstash) & maxOutputValue));
                    }
                }
                else if (bitstash >= inputWidth || ((accumulator << (outputWidth - bitstash)) & maxOutputValue) != 0)
                {
                    // no pad from 5 to 8 allowed
                    //Debug.WriteLine("invalid padding from {0} to {1} bits", inputWidth, outputWidth);
                    return null;
                }
                return output.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="hrp"></param>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string EncodeString(string hrp, string s)
            {
                //var data = ByteSquasher(Encoding.Default.GetBytes(s), 8, 5);
                return Bech32.Encode(hrp, Encoding.UTF8.GetBytes(s));
            }
        }
    }
}