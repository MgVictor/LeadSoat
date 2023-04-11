using System.ComponentModel.DataAnnotations;

namespace LeadSoatApi.Data.Entities
{
    public class configCampaignTest
    {
        [Key]
        public string? NID { get; set; }
        public string? SCAMPAIGNID { get; set; }
        public string? SCONTACTLISTID { get; set; }
    }
}
