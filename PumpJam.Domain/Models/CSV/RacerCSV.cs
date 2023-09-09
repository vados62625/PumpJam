using CsvHelper.Configuration.Attributes;

namespace Domain.Models.CSV
{
    public class RacerCSV
    {
        [Index(0)]        
        public string Category { get; set; }
        [Index(1)]
        public string LastName { get; set; }
        [Index(2)]
        public string FirstName { get; set; }
        [Index(3)]        
        public string Surname { get; set; }
        [Index(4)]        
        public int Number { get; set; }
    }
}
