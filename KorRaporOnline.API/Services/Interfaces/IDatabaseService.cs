using System.Collections.Generic;
using System.Threading.Tasks;
using KorRaporOnline.API.Models;
using System.Data;

namespace KorRaporOnline.API.Services.Interfaces
{
    public interface IDatabaseService
    {
        Task<List<string>> GetTablesAsync();
        Task<List<string>> GetTableColumnsAsync(string tableName);
        Task<object> ExecuteQueryAsync(string query);
        Task<IEnumerable<DatabaseConnection>> GetUserConnections(int userId);
        Task<DatabaseConnection> AddConnection(int userId, DatabaseConnectionModel model);
        Task<bool> TestConnection(DatabaseConnectionModel model);
        Task<List<string>> GetTables(int connectionId);
        Task<List<string>> GetColumns(int connectionId, string tableName);
    }
}