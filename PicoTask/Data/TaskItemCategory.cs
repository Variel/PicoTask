using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicoTask.Data
{
    public class TaskItemCategory
    {
        public Guid TaskItemId { get; set; }
        public TaskItem Task { get; set; }

        public Guid TaskCategoryId { get; set; }
        public TaskCategory Category { get; set; }
    }
}
