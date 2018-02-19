using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicoTask.Data
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDone { get; set; }
        public bool IsArchived { get; set; }

        public string RawTitle { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public string Place { get; set; }

        public DateTimeOffset? Deadline { get; set; }

        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public ICollection<TaskItemCategory> Categories { get; set; } = new HashSet<TaskItemCategory>();
    }
}
