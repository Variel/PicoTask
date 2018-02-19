using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicoTask.Data
{
    public class TaskCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Note { get; set; }

        public string RawAliases { get; set; } = "|";
        public string[] Aliases => RawAliases.Split("|");

        public ICollection<TaskItemCategory> Tasks { get; set; } = new HashSet<TaskItemCategory>();


        public void AddAlias(string alias)
        {
            alias = alias.ToLower();

            if (alias.Contains("|"))
                throw new Exception("| 문자는 포함할 수 없습니다");

            RawAliases += alias + "|";
        }

        public void RemoveAlias(string alias)
        {
            if (String.IsNullOrWhiteSpace(alias))
                RawAliases = "|";

            RawAliases = RawAliases.Replace("|" + alias + "|", "|");
        }
    }
}
