using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Cryptography;
using zapread.com.Services;
using NBitcoin;
using System.Numerics;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class LnAuthControllerTest
    {
        [TestMethod]
        public void TestVerify()
        {
            // Sample challenge-response
            //    Send to wallet:
            // req: lnurl1dp68gup69uhnzwfj9ccnvwpwxqhrzdej8gerwdf5xvhkcmnpw46xstmnd9nku6tw8a6xzeead3hkw6twye4nz0f4xqenqvps89zrxd6xxaryx3fc8yc5y3fcxycyxwzyx3qnydz9xveyyd6pg56r2wpjxscnxd2p8qcrzdzpx9prxd29xapnjdjxx4ryys3t74l
            // url:http://192.168.0.172:27543/lnauth/signin?tag=login&k1=5030009D37F7FCE891BE810C8D4A24E32B7AE45824135A8014A1B35E7C96F5FB
            //    Response from wallet:
            // key: 038b02325d76b1e096071a6fdaf6c695800ad79b7245f7e6b0d5ccd505a2d4c10c
            // k1:5030009D37F7FCE891BE810C8D4A24E32B7AE45824135A8014A1B35E7C96F5FB
            // sig:3044022006a071c4997c313e3c0bcb13daed2dddf1ebe5d899405125e674423c12480150022006664645062a1f2b5bca4d35c76ef72b0cdd46e1ba0cab89bd085c18a2922f6d

            // Works
            //var publicKey = CryptoService.HexStringToByteArray("038b02325d76b1e096071a6fdaf6c695800ad79b7245f7e6b0d5ccd505a2d4c10c");
            //var hash = CryptoService.HexStringToByteArray("5030009D37F7FCE891BE810C8D4A24E32B7AE45824135A8014A1B35E7C96F5FB");
            //var signature = CryptoService.HexStringToByteArray("3044022006a071c4997c313e3c0bcb13daed2dddf1ebe5d899405125e674423c12480150022006664645062a1f2b5bca4d35c76ef72b0cdd46e1ba0cab89bd085c18a2922f6d");

            // Fails - should work
            var publicKey = CryptoService.HexStringToByteArray("038b02325d76b1e096071a6fdaf6c695800ad79b7245f7e6b0d5ccd505a2d4c10c");
            var hash = CryptoService.HexStringToByteArray("6A6EAB458D4DE401A78F109911274A1A21A9A512BA861933D6937E0EA95B016A");
            var signature = CryptoService.HexStringToByteArray("3045022100da228f52aaeef71ba92b0241832e5e19f23f74bc663057017c35ce81c51efe5702206a0c1c1f56af604ada61557f827ca0778546c25d6729edbd43a8853f9a96435f");

            var secp256k1 = ECCurve.CreateFromValue("1.3.132.0.10");
            // signature is DER encoded -> convert to 64 byte array



            
            var p1len = signature[3];
            var sigp1 = signature.Skip(4).SkipWhile(b => b == 0).Take(32).ToArray(); // Remove any 0 padded bytes
            var p2len = signature.Skip(4+p1len+1).Take(1).ToArray()[0];
            var sigp2 = signature.Skip(4+p1len+2).SkipWhile(b => b == 0).Take(32).ToArray(); // Remove any 0 padded bytes
            var sig = sigp1.Concat(sigp2).ToArray();
            ;
            // decompress the public key

            //var i = publicKey[0];
            //var x = new BigInteger(publicKey.Skip(1).ToArray());

            //var p = new BigInteger(CryptoService.HexStringToByteArray("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"));
            //var a = new BigInteger(0);
            //var b = new BigInteger(7);

            //var y_sq = (x * x * x + a * x + b) % p;

            //var y = BigInteger.ModPow(y_sq, (p + 1) / 4, p);

            //if ((i == 02 && y % 2 != 0) || (i == 03 && y % 2 == 0))
            //{
            //    y = (p - y % p);
            //}

            //var pkX1 = x.ToByteArray();
            //var pkY1 = y.ToByteArray();

            PubKey pk = new PubKey(publicKey);
            var pkBytes = pk.Decompress().ToBytes();

            var pkX = pkBytes.Skip(1).Take(32).ToArray();
            var pkY = pkBytes.Skip(33).ToArray();

            var dsa = ECDsa.Create(new ECParameters
            {
                Curve = secp256k1,
                //D optional: (private key bytes) we don't have
                Q = new ECPoint
                {
                    // gets the {x,y} from the uncompressed public key
                    X = pkX,
                    Y = pkY,
                }
            }); ;

            var isValid = dsa.VerifyHash(hash, sig);

            Assert.IsTrue(isValid);
            //CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob)
            //ECDsaCng ecsdKey = new ECDsaCng()
        }
    }
}
