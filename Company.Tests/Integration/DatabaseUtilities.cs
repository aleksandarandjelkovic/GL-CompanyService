using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Company.Tests.Integration
{
    public static class DatabaseUtilities
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope.ServiceProvider);
            if (dbContext != null)
            {
                // Apply migrations
                await dbContext.Database.MigrateAsync();
            }
        }
        
        public static async Task ResetDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope.ServiceProvider);
            if (dbContext != null)
            {
                // Get all entity types from the context
                var entityTypes = dbContext.Model.GetEntityTypes()
                    .Select(e => e.ClrType)
                    .Where(t => !t.Name.Contains("MigrationHistory"))
                    .ToList();
                
                // Clear all tables in reverse dependency order (to handle foreign key constraints)
                foreach (var entityType in entityTypes)
                {
                    var entity = dbContext.Model.FindEntityType(entityType);
                    if (entity == null) continue;
                    
                    var tableName = entity.GetTableName();
                    var schemaName = entity.GetSchema();
                    
                    if (string.IsNullOrEmpty(tableName)) continue;
                    
                    var sql = string.IsNullOrEmpty(schemaName)
                        ? $"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE"
                        : $"TRUNCATE TABLE \"{schemaName}\".\"{tableName}\" RESTART IDENTITY CASCADE";
                        
                    // Using FormattableString to create a parameterized SQL query
                    await dbContext.Database.ExecuteSqlRawAsync(sql);
                }
                
                await dbContext.SaveChangesAsync();
            }
        }
        
        private static DbContext? GetDbContext(IServiceProvider serviceProvider)
        {
            // Try to get all DbContext types
            var dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t));
                
            if (dbContextType != null)
            {
                return serviceProvider.GetService(dbContextType) as DbContext;
            }
            
            return null;
        }
    }
} 