﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.Automation;
using Fork.Logic.Model.ProxyModels;
using Fork.ViewModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Fork.Logic.Persistence
{
    public class Persistence : IDisposable
    {
        #region Singleton
        private static Persistence instance;
        private static object contextLock;
        public static Persistence Instance => instance ??= new Persistence();

        private Persistence()
        {
            persistenceContext = new PersistenceContext();
            persistenceContext.Database.Migrate();
            ClearOrphans().Wait();
        }
        #endregion
        private PersistenceContext persistenceContext;

        public IEnumerable<Server> RequestServerList()
        {
            return persistenceContext.Servers;
        }
        
        public IEnumerable<Network> RequestNetworkList()
        {
            return persistenceContext.Networks;
        }

        /// <summary>
        /// Add entity to database
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(Entity entity)
        {
            lock (persistenceContext)
            {
                switch (entity)
                {
                    case Server server:
                        persistenceContext.Servers.Add(server);
                        break;
                    case Network network:
                        persistenceContext.Networks.Add(network);
                        break;
                }
                persistenceContext.SaveChanges();
            }
        }

        /// <summary>
        /// Remove entity from database
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEntity(Entity entity)
        {
            lock (persistenceContext)
            {
                switch (entity)
                {
                    case Server server:
                        persistenceContext.Servers.Remove(server);
                        break;
                    case Network network:
                        persistenceContext.Networks.Remove(network);
                        break;
                }
                persistenceContext.SaveChanges();
            }
        }

        /// <summary>
        /// Save changes of all entities to the database
        /// </summary>
        public void SaveChanges()
        {
            lock (persistenceContext)
                persistenceContext.SaveChanges();
        }

        /// <summary>
        /// Updates or Adds a List of entities to the Database
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task SaveEntities(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                switch (entity)
                {
                    case Server server:
                        if (persistenceContext.Servers.Any(server1 => server1.UID == server.UID))
                        {
                            persistenceContext.Servers.Update(server);
                        }
                        else
                        {
                            await persistenceContext.Servers.AddAsync(server);
                        }
                        break;
                    case Network network:
                        if (persistenceContext.Networks.Any(network1 => network1.UID == network.UID))
                        {
                            persistenceContext.Networks.Update(network);
                        }
                        else
                        {
                            await persistenceContext.Networks.AddAsync(network);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Can't save entity, because the type is not implemented.");
                }
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await persistenceContext.SaveChangesAsync();
            stopwatch.Stop();
            Console.WriteLine($"Saved persistence database in {stopwatch.ElapsedMilliseconds}ms");
        }

        public void Dispose()
        {
            persistenceContext.Dispose();
        }

        /// <summary>
        /// Clears orphans from each table
        ///
        /// Orphans are created when a parent entry is deleted, but the children (foreign keys) are not
        /// </summary>
        /// <returns></returns>
        private async Task ClearOrphans()
        {
            await ClearOrphanedVersions(await GetUsedVersionIds());
            await ClearOrphanedJavaSettings(await GetUsedJavaSettingsId());
            await ClearOrphanedRestartTimes(await GetUsedRestartIds());
            await ClearOrphanedStartTimes(await GetUsedAutoStartIds());
            await ClearOrphanedStopTimes(await GetUsedAutoStopIds());
            await ClearOrphanedSimpleTimes(await GetUsedTimeIds());
        }

        private async Task<List<long>> GetUsedVersionIds()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string serverRawQuery = $"SELECT VersionId FROM Servers";
            string networkRawQuery = $"SELECT VersionId FROM Networks";
            var cmd = new SqliteCommand(serverRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["VersionId"]);
                    }
                    catch {}
                }
            }
            cmd = new SqliteCommand(networkRawQuery, sqlConn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["VersionId"]);
                    }
                    catch {}
                }
            }
            return result;
        }

        private async Task<List<long>> GetUsedJavaSettingsId()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string serverRawQuery = $"SELECT JavaSettingsId FROM Servers";
            string networkRawQuery = $"SELECT JavaSettingsId FROM Networks";
            var cmd = new SqliteCommand(serverRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["JavaSettingsId"]);
                    }
                    catch {}
                }
            }
            cmd = new SqliteCommand(networkRawQuery, sqlConn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["JavaSettingsId"]);
                    }
                    catch {}
                }
            }
            return result;
        }

        private async Task<List<long>> GetUsedRestartIds()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string serverRawQuery = $"SELECT Restart1Id, Restart2Id, Restart3Id, Restart4Id FROM Servers";
            var cmd = new SqliteCommand(serverRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    result.Add((long) reader["Restart1Id"]);
                    result.Add((long) reader["Restart2Id"]);
                    result.Add((long) reader["Restart3Id"]);
                    result.Add((long) reader["Restart4Id"]);
                }
                catch {}
            }

            return result;
        }

        private async Task<List<long>> GetUsedAutoStartIds()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string serverRawQuery = $"SELECT AutoStart1Id, AutoStart2Id FROM Servers";
            var cmd = new SqliteCommand(serverRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    result.Add((long) reader["AutoStart1Id"]);
                    result.Add((long) reader["AutoStart2Id"]);
                }
                catch {}
            }

            return result;
        }

        private async Task<List<long>> GetUsedAutoStopIds()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string serverRawQuery = $"SELECT AutoStop1Id, AutoStop2Id FROM Servers";
            var cmd = new SqliteCommand(serverRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    result.Add((long) reader["AutoStop1Id"]);
                    result.Add((long) reader["AutoStop2Id"]);
                }
                catch {}
            }

            return result;
        }

        private async Task<List<long>> GetUsedTimeIds()
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string restartRawQuery = $"SELECT TimeId FROM RestartTime";
            string autoStartRawQuery = $"SELECT TimeId FROM StartTime";
            string autoStopRawQuery = $"SELECT TimeId FROM StopTime";
            var cmd = new SqliteCommand(restartRawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<long> result = new();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long)reader["TimeId"]);
                    }
                    catch {}
                } 
            }
            cmd = new SqliteCommand(autoStartRawQuery, sqlConn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["TimeId"]);
                    }
                    catch
                    {
                    }
                }
            }
            cmd = new SqliteCommand(autoStopRawQuery, sqlConn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    try
                    {
                        result.Add((long) reader["TimeId"]);
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        private async Task ClearOrphanedVersions(List<long> usedVersions)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM ServerVersion";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedVersions.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM ServerVersion WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task ClearOrphanedJavaSettings(List<long> usedJavaSettings)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM JavaSettings";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedJavaSettings.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM JavaSettings WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        
        private async Task ClearOrphanedSimpleTimes(List<long> usedTimeIds)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM SimpleTime";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedTimeIds.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM SimpleTime WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        
        private async Task ClearOrphanedRestartTimes(List<long> usedRestartTimeIds)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM RestartTime";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedRestartTimeIds.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM RestartTime WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        
        private async Task ClearOrphanedStartTimes(List<long> usedStartTimeIds)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM StartTime";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedStartTimeIds.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM StartTime WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        
        private async Task ClearOrphanedStopTimes(List<long> usedStopTimeIds)
        {
            await using var sqlConn = new SqliteConnection("Data Source=" + Path.Combine(App.ApplicationPath, "persistence", "data.db"));
            string rawQuery = $"SELECT Id FROM StopTime";
            var cmd = new SqliteCommand(rawQuery, sqlConn);
            await sqlConn.OpenAsync();
            List<string> deleteQueries = new();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                try
                {
                    if (!usedStopTimeIds.Contains((long)reader["Id"]))
                    {
                        deleteQueries.Add($"DELETE FROM StopTime WHERE Id={reader["Id"]}");
                    }
                }
                catch {}
            }

            foreach (string query in deleteQueries)
            {
                cmd = new SqliteCommand(query, sqlConn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}