using CybersecurityChatbotPOE.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CybersecurityChatbotPOE.Services
{
    public class DatabaseService
    {
        private string connectionString = "Server=localhost;Port=3306;Database=cybersecurity_chatbot;Uid=root;Pwd=Themba29@;";
        private bool isConnected = false;
        private string connectionError = "";

        public DatabaseService()
        {
            Task.Run(async () => await InitializeDatabase());
        }

        private async Task InitializeDatabase()
        {
            try
            {
                // Try with password
                using (var conn = new MySqlConnection("Server=localhost;Port=3306;Uid=root;Pwd=Themba29@;"))
                {
                    await conn.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Connected with password!");
                    await new MySqlCommand("CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot", conn).ExecuteNonQueryAsync();
                    isConnected = true;
                }

                if (isConnected)
                {
                    // Create ONLY activity_log table (no tasks table)
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        await conn.OpenAsync();

                        // Only create activity_log table
                        await new MySqlCommand(@"CREATE TABLE IF NOT EXISTS activity_log (
                            id INT PRIMARY KEY AUTO_INCREMENT,
                            action VARCHAR(100) NOT NULL,
                            details TEXT,
                            timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP)", conn).ExecuteNonQueryAsync();
                    }
                    System.Diagnostics.Debug.WriteLine("Database connected and activity_log table created!");
                }
            }
            catch (Exception ex)
            {
                connectionError = ex.Message;
                isConnected = false;
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        public bool IsConnected() => isConnected;
        public string GetConnectionError() => connectionError;

        // Task methods are no longer needed since we use in-memory storage
        // Keep only Activity Log methods

        public async Task AddLog(string action, string details)
        {
            if (!isConnected) return;
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("INSERT INTO activity_log (action, details) VALUES (@action, @details)", conn);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@details", details ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLog Error: {ex.Message}");
            }
        }

        public async Task<List<ActivityLogEntry>> GetLogs(int limit = 10)
        {
            var list = new List<ActivityLogEntry>();
            if (!isConnected) return list;
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("SELECT * FROM activity_log ORDER BY timestamp DESC LIMIT @limit", conn);
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new ActivityLogEntry
                            {
                                Id = reader.GetInt32("id"),
                                Action = reader.GetString("action"),
                                Details = reader.GetString("details"),
                                Timestamp = reader.GetDateTime("timestamp")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLogs Error: {ex.Message}");
            }
            return list;
        }
    }
}