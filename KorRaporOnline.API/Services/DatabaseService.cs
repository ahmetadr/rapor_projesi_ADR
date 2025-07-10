using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using KorRaporOnline.API.Data;
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Services.Interfaces;

namespace KorRaporOnline.API.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<string>> GetTablesAsync()
        {
            try
            {
                var activeConnection = await GetActiveConnection();
                using var connection = new SqlConnection(BuildConnectionString(activeConnection));
                await connection.OpenAsync();

                var schema = await connection.GetSchemaAsync("Tables");
                return schema.AsEnumerable()
                    .Select(row => row["TABLE_NAME"].ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tables");
                throw;
            }
        }

        public async Task<List<string>> GetTableColumnsAsync(string tableName)
        {
            try
            {
                var activeConnection = await GetActiveConnection();
                using var connection = new SqlConnection(BuildConnectionString(activeConnection));
                await connection.OpenAsync();

                var schema = await connection.GetSchemaAsync("Columns", new[] { null, null, tableName });
                return schema.AsEnumerable()
                    .Select(row => row["COLUMN_NAME"].ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get columns");
                throw;
            }
        }

        public async Task<object> ExecuteQueryAsync(string query)
        {
            try
            {
                var activeConnection = await GetActiveConnection();
                using var connection = new SqlConnection(BuildConnectionString(activeConnection));
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute query");
                throw;
            }
        }

        public async Task<IEnumerable<DatabaseConnection>> GetUserConnections(int userId)
        {
            return await _context.DatabaseConnections
                .Where(c => c.UserID == userId)
                .ToListAsync();
        }

        public async Task<DatabaseConnection> AddConnection(int userId, DatabaseConnectionModel model)
        {
            var connection = new DatabaseConnection
            {
                UserID = userId,
                ConnectionName = model.ConnectionName,
                ServerName = model.ServerName,
                DatabaseName = model.DatabaseName,
                Username = model.Username,
                PasswordHash = model.Password, // Note: Should be hashed in production
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DatabaseConnections.Add(connection);
            await _context.SaveChangesAsync();
            return connection;
        }

        public async Task<bool> TestConnection(DatabaseConnectionModel model)
        {
            try
            {
                using var connection = new SqlConnection(BuildConnectionString(model));
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetTables(int connectionId)
        {
            try
            {
                var connection = await _context.DatabaseConnections.FindAsync(connectionId);
                if (connection == null)
                    throw new Exception("Connection not found");

                using var sqlConnection = new SqlConnection(BuildConnectionString(connection));
                await sqlConnection.OpenAsync();

                var schema = await sqlConnection.GetSchemaAsync("Tables");
                return schema.AsEnumerable()
                    .Select(row => row["TABLE_NAME"].ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tables");
                throw;
            }
        }

        public async Task<List<string>> GetColumns(int connectionId, string tableName)
        {
            try
            {
                var connection = await _context.DatabaseConnections.FindAsync(connectionId);
                if (connection == null)
                    throw new Exception("Connection not found");

                using var sqlConnection = new SqlConnection(BuildConnectionString(connection));
                await sqlConnection.OpenAsync();

                var schema = await sqlConnection.GetSchemaAsync("Columns", new[] { null, null, tableName });
                return schema.AsEnumerable()
                    .Select(row => row["COLUMN_NAME"].ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get columns");
                throw;
            }
        }

        private async Task<DatabaseConnection> GetActiveConnection()
        {
            var connection = await _context.DatabaseConnections
                .FirstOrDefaultAsync(c => c.IsActive);

            if (connection == null)
                throw new Exception("No active database connection found");

            return connection;
        }

        private string BuildConnectionString(DatabaseConnection connection)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connection.ServerName,
                InitialCatalog = connection.DatabaseName,
                UserID = connection.Username,
                Password = connection.PasswordHash, // Note: Should be decrypted in production
                IntegratedSecurity = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }

        private string BuildConnectionString(DatabaseConnectionModel model)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = model.ServerName,
                InitialCatalog = model.DatabaseName,
                UserID = model.Username,
                Password = model.Password,
                IntegratedSecurity = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
    }
}