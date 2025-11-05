using System.Data.SqlClient;
using DiffSpectrumView.Models;

namespace DiffSpectrumView.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly string _connectionString;

        public JobRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<List<Job>> GetAllAsync()
        {
            var jobs = new List<Job>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "SELECT * FROM Jobs ORDER BY StartTime DESC", 
                connection);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                jobs.Add(MapToJob(reader));
            }
            
            return jobs;
        }

        public async Task<Job?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(
                "SELECT * FROM Jobs WHERE Id = @Id", 
                connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToJob(reader);
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Job job)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                INSERT INTO Jobs (Name, StartTime, EndTime, Status, TotalRequestsProcessed, DiffsFound, ErrorMessage)
                VALUES (@Name, @StartTime, @EndTime, @Status, @TotalRequestsProcessed, @DiffsFound, @ErrorMessage);
                SELECT CAST(SCOPE_IDENTITY() as int);", 
                connection);
            
            AddJobParameters(command, job);
            
            var id = await command.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task UpdateAsync(Job job)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                UPDATE Jobs 
                SET Name = @Name, StartTime = @StartTime, EndTime = @EndTime, 
                    Status = @Status, TotalRequestsProcessed = @TotalRequestsProcessed, 
                    DiffsFound = @DiffsFound, ErrorMessage = @ErrorMessage
                WHERE Id = @Id", 
                connection);
            
            command.Parameters.AddWithValue("@Id", job.Id);
            AddJobParameters(command, job);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> GetTotalJobsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Jobs", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetTotalDiffsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Diffs WHERE IsDeleted = 0", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Diffs are just differences found, not "failed" or "succeeded"

        private static Job MapToJob(SqlDataReader reader)
        {
            return new Job
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                TotalRequestsProcessed = reader.GetInt32(reader.GetOrdinal("TotalRequestsProcessed")),
                DiffsFound = reader.GetInt32(reader.GetOrdinal("DiffsFound")),
                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage"))
            };
        }

        private static void AddJobParameters(SqlCommand command, Job job)
        {
            command.Parameters.AddWithValue("@Name", job.Name);
            command.Parameters.AddWithValue("@StartTime", job.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)job.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", job.Status);
            command.Parameters.AddWithValue("@TotalRequestsProcessed", job.TotalRequestsProcessed);
            command.Parameters.AddWithValue("@DiffsFound", job.DiffsFound);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)job.ErrorMessage ?? DBNull.Value);
        }
    }
}
