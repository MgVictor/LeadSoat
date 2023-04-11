using System.ComponentModel.DataAnnotations;

namespace LeadSoatApi.Data.Entities
{
    public class LeadSoatTest
    {
        [Key]
        public string? DREGDATE { get; set; }
        public string? SMAIL { get; set; }
        public string? SLEGALNAME { get; set; }
        public string? SCLIENT_NAME { get; set; }
        public string? SCLIENT_APPPAT { get; set; }
        public string? SCLIENT_APPMAT { get; set; }
        public string? NOMBRE { get; set; }
        public string? SDOCUMENT { get; set; }
        public string? SREGIST { get; set; }
        public string? SBRANCH { get; set; }
        public string? MODELO { get; set; }
        public string? ANIO { get; set; }
        public string? SPHONE { get; set; }
        
    }

}
