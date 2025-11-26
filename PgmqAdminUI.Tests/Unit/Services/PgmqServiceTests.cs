using Microsoft.Extensions.Logging;
using PgmqAdminUI.Services;

namespace PgmqAdminUI.Tests.Unit.Services;

public class PgmqServiceTests
{
    [Test]
    public void Constructor_WithValidConnectionString_CreatesInstance()
    {
        var logger = A.Fake<ILogger<PgmqService>>();
        var connectionString = "Host=localhost;Port=5432;Database=test;";

        var service = new PgmqService(connectionString, logger);

        service.ShouldNotBeNull();
    }
}
