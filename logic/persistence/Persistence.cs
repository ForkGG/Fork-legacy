using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fork.Logic.Model;
using Fork.Logic.Model.Automation;
using Fork.Logic.Model.ProxyModels;
using Fork.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

        public Entity RequestEntity(string entityId)
        {
            lock (persistenceContext)
            {
                if (persistenceContext.Servers.Any(server => server.UID == entityId))
                {
                    return persistenceContext.Servers.Find(entityId);
                } 
                else if (persistenceContext.Networks.Any(network => network.UID == entityId))
                {
                    return persistenceContext.Networks.Find(entityId);
                }
                else
                {
                    throw new DataException("Database did not contain requested entity");
                }
            }
        }

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

        public void SaveChanges()
        {
            lock (persistenceContext)
                persistenceContext.SaveChanges();
        }

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
    }
}