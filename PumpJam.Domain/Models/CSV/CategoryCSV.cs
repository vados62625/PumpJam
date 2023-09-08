using CsvHelper.Configuration.Attributes;

namespace Domain.Models.CSV
{
    public class CategoryCSV
    {
        [Index(0)]        
        public string Category { get; set; }
        [Index(1)]
        public int? R3 { get; set; }
        [Index(2)]
        public int? R4 { get; set; }
        [Index(3)]        
        public int? R5 { get; set; }
    }
}
