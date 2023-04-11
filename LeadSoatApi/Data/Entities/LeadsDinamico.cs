namespace LeadSoatApi.Data.Entities
{
    public class LeadsDinamico
    {
        public string? id_campana { get;set;}
        public string? user { get;set;}
        public string? estado {get;set;}
        public IList<Fields>? fields { get;set;}
    }
}
