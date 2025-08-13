using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoRemoveBad.Core.HealthChecks
{
    /// <summary>
    /// Health check for Azure Table Storage connectivity.
    /// </summary>
    public class TableStorageHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageHealthCheck"/> class.
        /// </summary>
        /// <param name="connectionString">The Azure Storage connection string.</param>
        /// <param name="tableName">The name of the table to test connectivity with.</param>
        public TableStorageHealthCheck(string connectionString, string tableName)
        {
            _connectionString = connectionString ?? string.Empty;
            _tableName = tableName ?? string.Empty;
        }

        /// <summary>
        /// Checks the health of the Azure Table Storage connection.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the health check result.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create table service client
                var serviceClient = new TableServiceClient(_connectionString);
                
                // Get table client
                var tableClient = serviceClient.GetTableClient(_tableName);
                
                // Try to create the table (it will succeed if it already exists)
                await tableClient.CreateIfNotExistsAsync(cancellationToken);
                
                // Try to query the table metadata to verify connectivity
                await tableClient.GetAccessPoliciesAsync(cancellationToken);
                
                return HealthCheckResult.Healthy($"Table Storage is accessible. Table '{_tableName}' exists or was created successfully.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Table Storage connectivity failed: {ex.Message}", ex);
            }
        }
    }
}
