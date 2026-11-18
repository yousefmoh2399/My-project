using KingWorldWar.Core.Logging;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class LogRedactorTests
    {
        [Test]
        public void Redact_NullOrEmpty_ReturnsSame()
        {
            Assert.IsNull(LogRedactor.Redact(null));
            Assert.AreEqual(string.Empty, LogRedactor.Redact(string.Empty));
        }

        [Test]
        public void Redact_CleanMessage_Unchanged()
        {
            const string msg = "User loaded territory 12 successfully.";
            Assert.AreEqual(msg, LogRedactor.Redact(msg));
        }

        [Test]
        public void Redact_PasswordInKeyValue_RedactsValue()
        {
            string input = "User login attempt with password=SecretPassword123! from 192.168.1.1";
            string result = LogRedactor.Redact(input);

            Assert.IsFalse(result.Contains("SecretPassword123!"));
            Assert.IsTrue(result.Contains("password=[REDACTED]"));
        }

        [Test]
        public void Redact_BearerToken_RedactsToken()
        {
            string input = "Headers: Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.payload.sig";
            string result = LogRedactor.Redact(input);

            Assert.IsFalse(result.Contains("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"));
            Assert.IsTrue(result.Contains("Bearer [REDACTED]"));
        }

        [Test]
        public void Redact_PostgresConnectionString_MasksPasswordOnly()
        {
            string input = "Connecting to postgres://nakama_admin:SuperSecretPass123@db.prod.internal:5432/nakama";
            string result = LogRedactor.Redact(input);

            Assert.IsFalse(result.Contains("SuperSecretPass123"));
            Assert.IsTrue(result.Contains("postgres://nakama_admin:[REDACTED]@db.prod.internal:5432/nakama"));
        }

        [Test]
        public void Redact_ApiKey_RedactsKey()
        {
            string input = "Request with api_key: AIzaSyD3ExampleKey12345 sent.";
            string result = LogRedactor.Redact(input);

            Assert.IsFalse(result.Contains("AIzaSyD3ExampleKey12345"));
            Assert.IsTrue(result.Contains("api_key=[REDACTED]"));
        }
    }
}
