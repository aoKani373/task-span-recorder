using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSpanRecorder.Models
{
    public class TaskSpan
    {
        public int Id { get; set; }

        public int TaskCategoryId { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly? EndTime { get; set; } = null;

        public TaskCategory? TaskCategory { get; set; } = null;
    }
}
