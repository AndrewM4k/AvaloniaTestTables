using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaTestTables.Models
{
    public class Mode
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; } = "New Mode";
        public int MaxBottleNumber { get; set; }
        public int MaxUsedTips { get; set; }

        public List<Step> Steps { get; set; } = new();
    }
}
