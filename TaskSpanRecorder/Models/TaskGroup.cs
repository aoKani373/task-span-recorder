using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSpanRecorder.Models
{
    public class TaskGroup
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string ColorHex { get; set; } = "#FF808080";
    }
}
