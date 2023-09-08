namespace Domain.Models
{
    public class RacerDB
    {
        public int Id { get; set; }
        public double Bib { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}
