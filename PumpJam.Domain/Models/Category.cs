namespace Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public bool Next { get; set; } = false;
        public DateTime NextDateTime { get; set; }
        public string Name { get; set; }
        public int Race3 { get; set; }
        public int Race4 { get; set; }
        public int Race5 { get; set; }
        public virtual List<RacerDB> Racers { get; set; }
    }
}
