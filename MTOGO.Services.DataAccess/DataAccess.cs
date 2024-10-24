using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MTOGO.Services.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await getData(connection);
            }
            catch (Exception ex)
            {
                throw new Exception("Error with connection to database: " + ex.Message, ex);
            }
        }
        public Task<T?> GetById<T>(string sql, object parameters) =>
                    WithConnection(db => db.QueryAsync<T>(sql, parameters).ContinueWith(t => t.Result.FirstOrDefault()));

        public Task<List<T>> GetAll<T>(string sql, object? parameters = null) =>
            WithConnection(db => db.QueryAsync<T>(sql, parameters).ContinueWith(t => t.Result.ToList()));

        public Task<int> Insert(string sql, object parameters) =>
            WithConnection(db => db.ExecuteAsync(sql, parameters));
        public Task<T?> InsertAndGetId<T>(string sql, object parameters) =>
            WithConnection(db => db.ExecuteScalarAsync<T?>(sql, parameters));

        public Task<int> Update(string sql, object parameters) =>
            WithConnection(db => db.ExecuteAsync(sql, parameters));

        public Task<int> Delete(string sql, object parameters) =>
            WithConnection(db => db.ExecuteAsync(sql, parameters));

        public Task<T?> ExecuteStoredProcedure<T>(string procedureName, object parameters) =>
          WithConnection(db => db.QueryFirstOrDefaultAsync<T>(
              procedureName, parameters, commandType: CommandType.StoredProcedure));
    }
}
