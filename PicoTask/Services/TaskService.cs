using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PicoTask.Data;

namespace PicoTask.Services
{
    public class TaskService
    {
        private readonly DatabaseContext _database;
        private readonly CategoryService _categoryService;

        public TaskService(DatabaseContext database, CategoryService categoryService)
        {
            _database = database;
            _categoryService = categoryService;
        }

        public async Task<TaskItem> CreateTaskAsync(string rawTitle, string note)
        {
            var task = new TaskItem();

            try
            {
                await ApplyRawTitleAsync(task, rawTitle);
            }
            catch (Exception e)
            {
                note += $"\n{e.Message}";
            }

            task.Note = note;

            _database.Tasks.Add(task);
            await _database.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem> GetTaskAsync(Guid id)
        {
            return await _database.Tasks
                .Include(t => t.Categories)
                    .ThenInclude(j => j.Category)
                .SingleOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem[]> GetTasksAsync(bool includeDone = false)
        {
            return await _database.Tasks
                .Include(t => t.Categories)
                    .ThenInclude(j => j.Category)
                .Where(t => !t.IsDone || includeDone && !t.IsArchived)
                .OrderByDescending(t => t.CreatedAt)
                .ToArrayAsync();
        }

        public async Task<TaskItem> ToggleDoneAsync(Guid id)
        {
            var task = await _database.Tasks.FindAsync(id);
            task.IsDone = !task.IsDone;

            await _database.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem> ArchiveAsync(Guid id)
        {
            var task = await _database.Tasks.FindAsync(id);
            task.IsArchived = true;

            await _database.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem> UnarchiveAsync(Guid id)
        {
            var task = await _database.Tasks.FindAsync(id);
            task.IsArchived = false;

            await _database.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem> EditAsync(Guid id, string rawTitle, string note)
        {
            var task = await _database.Tasks.FindAsync(id);

            try
            {
                await ApplyRawTitleAsync(task, rawTitle);
            }
            catch(Exception e)
            {
                note += $"\n{e.Message}";
            }

            task.Note = note;

            await _database.SaveChangesAsync();

            return task;
        }

        private async Task ApplyRawTitleAsync(TaskItem task, string rawTitle)
        {
            string title;
            string place;
            DateTimeOffset? deadline = null;
            TaskCategory[] categories;

            const string categoryPattern = @"#(\w+)";
            const string deadlinePattern =
                @"\+\s?(?<year>\d\d)?\.?(?<month>\d\d?)\.(?<day>\d\d?)-?(?<hour>\d\d?)?:?(?<minute>\d\d)?(?<ampm>[PA]M)?";
            const string placePattern = @"@(.+)$";


            categories =
                (await Task.WhenAll(
                    Regex.Matches(rawTitle, categoryPattern)
                        .Select(m => m.Groups[1].Value.Replace("_", " "))
                        .Select(async c => await _categoryService.FindCategoryAsync(c))))
                .Where(c => c != null)
                .ToArray();

            rawTitle = Regex.Replace(rawTitle, categoryPattern, "");
            var deadlineMatch = Regex.Match(rawTitle, deadlinePattern);
            if (deadlineMatch.Success)
            {
                var deadlineYearGroup = deadlineMatch.Groups["year"];
                var deadlineMonthGroup = deadlineMatch.Groups["month"];
                var deadlineDayGroup = deadlineMatch.Groups["day"];
                var deadlineHourGroup = deadlineMatch.Groups["hour"];
                var deadlineMinuteGroup = deadlineMatch.Groups["minute"];
                var deadlineAmpmGroup = deadlineMatch.Groups["ampm"];

                var deadlineTextBuilder = new StringBuilder();
                var formatBuilder = new StringBuilder();

                if (deadlineYearGroup.Success)
                {
                    deadlineTextBuilder.Append(deadlineYearGroup.Value + ".");
                    formatBuilder.Append("yy.");
                }

                deadlineTextBuilder.Append(deadlineMonthGroup.Value + "." + deadlineDayGroup.Value);
                formatBuilder.Append("M.d");

                if (deadlineHourGroup.Success)
                {
                    deadlineTextBuilder.Append("-" + deadlineHourGroup.Value);
                    formatBuilder.Append("-h");
                }

                if (deadlineMinuteGroup.Success)
                {
                    deadlineTextBuilder.Append(":" + deadlineMinuteGroup.Value);
                    formatBuilder.Append(":m");
                }

                if (deadlineAmpmGroup.Success)
                {
                    deadlineTextBuilder.Append(deadlineAmpmGroup.Value);
                    formatBuilder.Append("tt");
                }

                try
                {
                    deadline = DateTimeOffset.ParseExact(deadlineTextBuilder.ToString(), formatBuilder.ToString(),
                        CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    throw new Exception("기한 확인 불가", e);
                }
            }

            rawTitle = Regex.Replace(rawTitle, deadlinePattern, "");
            place = Regex.Match(rawTitle, placePattern).Groups[1].Value;

            title = Regex.Replace(rawTitle, placePattern, "").Trim();

            task.Deadline = deadline;
            task.RawTitle = rawTitle;
            task.Title = title;
            task.Place = place;

            task.Categories.Clear();

            foreach (var category in categories)
            {
                task.Categories.Add(new TaskItemCategory
                {
                    Category = category,
                    Task = task
                });
            }
        }
    }
}
