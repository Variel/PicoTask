using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PicoTask.Data;

namespace PicoTask.Services
{
    public class CategoryService
    {
        private readonly DatabaseContext _database;

        public CategoryService(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<TaskCategory> FindCategoryAsync(string query)
        {
            query = query.ToLower();

            return await _database.Categories
                .Where(c => c.FullName == query || c.RawAliases.Contains("|" + query + "|"))
                .FirstOrDefaultAsync();
        }

        public async Task<TaskCategory> CreateCategoryAsync(string fullName)
        {
            var category = new TaskCategory
            {
                FullName = fullName
            };

            _database.Categories.Add(category);
            await _database.SaveChangesAsync();

            return category;
        }

        public async Task<bool> AddAliasAsync(TaskCategory category, string alias)
        {
            if (await FindCategoryAsync(alias) != null) return false;

            category.AddAlias(alias);
            await _database.SaveChangesAsync();

            return true;
        }

        public async Task RemoveAliasAsync(TaskCategory category, string alias)
        {
            category.RemoveAlias(alias);
            await _database.SaveChangesAsync();
        }
    }
}
