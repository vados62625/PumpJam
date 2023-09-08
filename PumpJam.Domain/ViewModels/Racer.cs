using Newtonsoft.Json;

namespace Domain.ViewModels
{
    public class Racer
    {
        [JsonProperty("Contest")]
        public string Contest { get; set; }

        [JsonProperty("RankQual")]
        public string RankQual { get; set; } = "-";

        [JsonProperty("Bib")]
        public int Bib { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("H1t")]
        public string H1t { get; set; }

        [JsonProperty("H1r")]
        public string H1r { get; set; } = "-";

        [JsonProperty("H1b")]
        public string H1b { get; set; }

        [JsonProperty("Race1t")]
        public string Race1t { get; set; }

        [JsonProperty("Race1r")]
        public string Race1r { get; set; } = "-";

        [JsonProperty("H2t")]
        public string H2t { get; set; }

        [JsonProperty("H2r")]
        public string H2r { get; set; } = "-";

        [JsonProperty("H2b")]
        public string H2b { get; set; }

        [JsonProperty("Race2t")]
        public string Race2t { get; set; }

        [JsonProperty("Race2r")]
        public string Race2r { get; set; } = "-";
        [JsonProperty("H3t")]
        public string H3t { get; set; }

        [JsonProperty("H3r")]
        public string H3r { get; set; } = "-";

        [JsonProperty("H3b")]
        public string H3b { get; set; }

        [JsonProperty("Race3t")]
        public string Race3t { get; set; }

        [JsonProperty("Race3r")]
        public string Race3r { get; set; } = "-";
        [JsonProperty("H4t")]
        public string H4t { get; set; }

        [JsonProperty("H4r")]
        public string H4r { get; set; } = "-";

        [JsonProperty("H4b")]
        public string H4b { get; set; }

        [JsonProperty("Race4t")]
        public string Race4t { get; set; }

        [JsonProperty("Race4r")]
        public string Race4r { get; set; } = "-";
        [JsonProperty("H5t")]
        public string H5t { get; set; }

        [JsonProperty("H5r")]
        public string H5r { get; set; } = "-";

        [JsonProperty("H5b")]
        public string H5b { get; set; }

        [JsonProperty("Race5t")]
        public string Race5t { get; set; }

        [JsonProperty("Race5r")]
        public string Race5r { get; set; } = "-";

        [JsonProperty("BEST")]
        public double? BEST { get; set; }

        [JsonProperty("LAST")]
        public double? LAST { get; set; }
    }
}
