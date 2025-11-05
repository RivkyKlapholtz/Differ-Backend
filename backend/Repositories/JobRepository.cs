using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
                INSERT INTO Jobs (StartTime, EndTime, Status, FoundDiff, DiffId, ErrorMessage)
                VALUES (@StartTime, @EndTime, @Status, @FoundDiff, @DiffId, @ErrorMessage);
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
                SET StartTime = @StartTime, EndTime = @EndTime, 
                    Status = @Status, FoundDiff = @FoundDiff, 
                    DiffId = @DiffId, ErrorMessage = @ErrorMessage
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

        public async Task<int> GetSuccessfulJobsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Jobs WHERE Status = 'Success'", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetFailedJobsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Jobs WHERE Status = 'Failed'", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetJobsWithDiffsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT COUNT(*) FROM Jobs WHERE FoundDiff = 1", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static Job MapToJob(SqlDataReader reader)
        {
            return new Job
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                FoundDiff = reader.GetBoolean(reader.GetOrdinal("FoundDiff")),
                DiffId = reader.IsDBNull(reader.GetOrdinal("DiffId")) ? null : reader.GetInt32(reader.GetOrdinal("DiffId")),
                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage"))
            };
        }

        private static void AddJobParameters(SqlCommand command, Job job)
        {
            command.Parameters.AddWithValue("@StartTime", job.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)job.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", job.Status);
            command.Parameters.AddWithValue("@FoundDiff", job.FoundDiff);
            command.Parameters.AddWithValue("@DiffId", (object?)job.DiffId ?? DBNull.Value);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)job.ErrorMessage ?? DBNull.Value);
        }
    }
}
