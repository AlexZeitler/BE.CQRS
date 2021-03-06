using System.Net;
using BE.CQRS.Domain.Events;
using Xunit;

namespace AspCore.Tests
{
    public class GivenStatusCodeConverter
    {
        [Fact]
        public void AcceptedWhenNoError()
        {
            var result = new AppendResult(false, 14);

            HttpStatusCode code = StatusCodeConverter.From(result);

            Assert.Equal(HttpStatusCode.Accepted, code);
        }

        [Fact]
        public void ConflictWhenVersionError()
        {
            var result = new AppendResult(true, 14);

            HttpStatusCode code = StatusCodeConverter.From(result);

            Assert.Equal(HttpStatusCode.Conflict, code);
        }
    }
}