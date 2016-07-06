using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAPI.SignatureTypes.Tests
{
    [TestClass()]
    public class HmacSignatureFactoryTests
    {
        [TestMethod()]
        public void VerifySignatureTest()
        {
            HmacSignatureFactory.Secret = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
            var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkY3RoYW5nQGdtYWlsLmNvbSIsIm5hbWUiOiJUaGFuZyBEdW9uZyIsImFkbWluIjp0cnVlfQ.sOvVeT89cDO00PfB_U5RZHJQ8Lq3SI56JUs8PwmgGz8";

            const int n = 10000; // ten thousand
            for (int i = 0; i < n; i++)
            {
                var actual = HmacSignatureFactory.VerifySignature(jwt, HmacSignatureFactory.ValidationParameters);
                Assert.IsNotNull(actual);
            }
        }
    }
}