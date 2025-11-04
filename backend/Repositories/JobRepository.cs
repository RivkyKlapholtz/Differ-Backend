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
                INSERT INTO Jobs (Name, StartTime, EndTime, Status, TotalDiffs, FailedDiffs, SucceededDiffs)
                VALUES (@Name, @StartTime, @EndTime, @Status, @TotalDiffs, @FailedDiffs, @SucceededDiffs);
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
                    Status = @Status, TotalDiffs = @TotalDiffs, FailedDiffs = @FailedDiffs, 
                    SucceededDiffs = @SucceededDiffs
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

        public async Task<int> GetFailedDiffsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM Diffs 
                WHERE IsDeleted = 0 AND Category IN ('JSON Response', 'Status Code', 'Headers')", 
                connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetSucceededDiffsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM Diffs 
                WHERE IsDeleted = 0 AND Category NOT IN ('JSON Response', 'Status Code', 'Headers')", 
                connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static Job MapToJob(SqlDataReader reader)
        {
            return new Job
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                StartTime = reader.GetDateTime("StartTime"),
                EndTime = reader.IsDBNull("EndTime") ? null : reader.GetDateTime("EndTime"),
                Status = reader.GetString("Status"),
                TotalDiffs = reader.GetInt32("TotalDiffs"),
                FailedDiffs = reader.GetInt32("FailedDiffs"),
                SucceededDiffs = reader.GetInt32("SucceededDiffs")
            };
        }

        private static void AddJobParameters(SqlCommand command, Job job)
        {
            command.Parameters.AddWithValue("@Name", job.Name);
            command.Parameters.AddWithValue("@StartTime", job.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)job.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", job.Status);
            command.Parameters.AddWithValue("@TotalDiffs", job.TotalDiffs);
            command.Parameters.AddWithValue("@FailedDiffs", job.FailedDiffs);
            command.Parameters.AddWithValue("@SucceededDiffs", job.SucceededDiffs);
        }
    }
}
