
namespace AvaloniaTestTables.Models
{
    public class Step
    {
        public int ID { get; set; }
        public int ModeId { get; set; }
        public Mode Mode { get; set; }
        public int Timer { get; set; }
        public string Destination { get; set; }
        public int Speed { get; set; }
        public string Type { get; set; }
        public decimal Volume { get; set; }
    }
}
