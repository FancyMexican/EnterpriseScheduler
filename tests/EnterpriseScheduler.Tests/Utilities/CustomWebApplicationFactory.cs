using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Tests.Utilities;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static int _databaseCounter;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database with unique name
            var databaseName = $"TestDb_{Interlocked.Increment(ref _databaseCounter)}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created and clean
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed test data
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                TimeZone = "UTC"
            };
            db.Users.Add(testUser);
            db.SaveChanges();

            // Dispose the scope after seeding
            scope.Dispose();
        });
    }
}
