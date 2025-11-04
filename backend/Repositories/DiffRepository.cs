using System.Data;
using System.Data.SqlClient;
using DiffSpectrumView.Models;

namespace DiffSpectrumView.Repositories
{
    public class DiffRepository : IDiffRepository
    {
        private readonly string _connectionString;

        public DiffRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<List<Diff>> GetAllAsync()
        {
            var diffs = new List<Diff>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "SELECT * FROM Diffs WHERE IsDeleted = 0 ORDER BY Timestamp DESC", 
                connection);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                diffs.Add(MapToDiff(reader));
            }
            
            return diffs;
        }

        public async Task<Diff?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "SELECT * FROM Diffs WHERE Id = @Id", 
                connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToDiff(reader);
            }
            
            return null;
        }

        public async Task<List<Diff>> GetByJobIdAsync(int jobId)
        {
            var diffs = new List<Diff>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "SELECT * FROM Diffs WHERE JobId = @JobId AND IsDeleted = 0 ORDER BY Timestamp DESC", 
                connection);
            command.Parameters.AddWithValue("@JobId", jobId);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                diffs.Add(MapToDiff(reader));
            }
            
            return diffs;
        }

        public async Task<int> CreateAsync(Diff diff)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO Diffs (JobId, Category, Endpoint, Method, ProductionResponse, 
                    IntegrationResponse, ProductionCurl, IntegrationCurl, Timestamp, IsDeleted, IsChecked)
                VALUES (@JobId, @Category, @Endpoint, @Method, @ProductionResponse, 
                    @IntegrationResponse, @ProductionCurl, @IntegrationCurl, @Timestamp, @IsDeleted, @IsChecked);
                SELECT CAST(SCOPE_IDENTITY() as int);", 
                connection);
            
            AddDiffParameters(command, diff);
            
            var id = await command.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task UpdateAsync(Diff diff)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE Diffs 
                SET JobId = @JobId, Category = @Category, Endpoint = @Endpoint, 
                    Method = @Method, ProductionResponse = @ProductionResponse, 
                    IntegrationResponse = @IntegrationResponse, ProductionCurl = @ProductionCurl, 
                    IntegrationCurl = @IntegrationCurl, Timestamp = @Timestamp, 
                    IsDeleted = @IsDeleted, IsChecked = @IsChecked
                WHERE Id = @Id", 
                connection);
            
            command.Parameters.AddWithValue("@Id", diff.Id);
            AddDiffParameters(command, diff);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "UPDATE Diffs SET IsDeleted = 1 WHERE Id = @Id", 
                connection);
            command.Parameters.AddWithValue("@Id", id);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task RestoreAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "UPDATE Diffs SET IsDeleted = 0 WHERE Id = @Id", 
                connection);
            command.Parameters.AddWithValue("@Id", id);
            
            await command.ExecuteNonQueryAsync();
        }

        private static Diff MapToDiff(SqlDataReader reader)
        {
            return new Diff
            {
                Id = reader.GetInt32("Id"),
                JobId = reader.GetInt32("JobId"),
                Category = reader.GetString("Category"),
                Endpoint = reader.GetString("Endpoint"),
                Method = reader.GetString("Method"),
                ProductionResponse = reader.GetString("ProductionResponse"),
                IntegrationResponse = reader.GetString("IntegrationResponse"),
                ProductionCurl = reader.GetString("ProductionCurl"),
                IntegrationCurl = reader.GetString("IntegrationCurl"),
                Timestamp = reader.GetDateTime("Timestamp"),
                IsDeleted = reader.GetBoolean("IsDeleted"),
                IsChecked = reader.GetBoolean("IsChecked")
            };
        }

        private static void AddDiffParameters(SqlCommand command, Diff diff)
        {
            command.Parameters.AddWithValue("@JobId", diff.JobId);
            command.Parameters.AddWithValue("@Category", diff.Category);
            command.Parameters.AddWithValue("@Endpoint", diff.Endpoint);
            command.Parameters.AddWithValue("@Method", diff.Method);
            command.Parameters.AddWithValue("@ProductionResponse", diff.ProductionResponse);
            command.Parameters.AddWithValue("@IntegrationResponse", diff.IntegrationResponse);
            command.Parameters.AddWithValue("@ProductionCurl", diff.ProductionCurl);
            command.Parameters.AddWithValue("@IntegrationCurl", diff.IntegrationCurl);
            command.Parameters.AddWithValue("@Timestamp", diff.Timestamp);
            command.Parameters.AddWithValue("@IsDeleted", diff.IsDeleted);
            command.Parameters.AddWithValue("@IsChecked", diff.IsChecked);
        }
    }
}
