using Newtonsoft.Json;

namespace Domain.ViewModels
{
    public class RacerDto
    {
        [JsonProperty("Contest")]
        public string Contest { get; set; }
        
        [JsonProperty("Qual")]
        public bool Qual { get; set; }

        [JsonProperty("Rank")]
        public string Rank { get; set; }

        [JsonProperty("Bib")]
        public int Bib { get; set; }
        public int? Id { get; set; }
        
        [JsonProperty("raceNum")]
        public int RaceNum { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Ht")]
        public string Ht { get; set; }

        [JsonProperty("Hr")]
        public string Hr { get; set; }

        [JsonProperty("Hb")]
        public string Hb { get; set; }

        [JsonProperty("HbClass")]
        public string HbClass { get; set; }

        [JsonProperty("RaceT")]
        public string RaceT { get; set; }
        
        [JsonProperty("H2t")]
        public string H2t { get; set; }

        [JsonProperty("H2r")]
        public string H2r { get; set; }

        [JsonProperty("H2b")]
        public string H2b { get; set; }

        [JsonProperty("H2bClass")]
        public string H2bClass { get; set; }

        [JsonProperty("Race2T")]
        public string Race2T { get; set; }

        //[JsonProperty("RaceR")]
        //public string RaceR { get; set; }        

        [JsonProperty("BEST")]
        public double? BEST { get; set; }

        [JsonProperty("LAST")]
        public double? LAST { get; set; }
    }
}
