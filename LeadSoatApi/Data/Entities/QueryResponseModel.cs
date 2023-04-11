using Newtonsoft.Json;

namespace LeadSoatApi.Data.Entities
{
    public class QueryResponseModel : BaseResponseModel
    {
        public List<Lead>? Leads { get; set; }
        public QueryResponseModel()
        {
            Leads = new List<Lead>();
        }
    }

    public class QueryResponseModelContact : BaseResponseModel
    {
        public List<Contact>? Contacts { get; set; }
        public QueryResponseModelContact()
        {
            Contacts = new List<Contact>();
        }
    }

    public class Lead
    {
        public string? DREGDATE { get; set; }
        public string? SMAIL { get; set; }
        //public string? SLEGALNAME { get; set; }
        public string? SCLIENT_NAME { get; set; }
        //public string? SCLIENT_APPPAT { get; set; }
        //public string? SCLIENT_APPMAT { get; set; }
        //public string? NOMBRE { get; set; }
        public string? SDOCUMENT { get; set; }
        public string? SREGIST { get; set; }
        public string? SBRANCH { get; set; }
        public string? MODELO { get; set; }
        public string? ANIO { get; set; }
        public string? SPHONE { get; set; }
        public string? SSTEP { get; set; }
        public string? SFLAGEMISION { get; set; }

        //Email
        public string? SEMAILTO { get; set; }
        public string? SEMAILPW { get; set; }
        public string? SEMAILFM { get; set; }
    }
    /*
        * ------------------------------------------------------------- 
    */
    public class Contact
    {
        public string? contactListId { get; set; }
        public DataContact? data { get; set; }
        public bool? callable { get; set; }
        public PhoneNumberStatusContact? phoneNumberStatus { get; set; }
        public ContactableStatusContact? contactableStatus { get; set; }
    }

    public class DataContact
    {
        public string? NOMBRE_CONTRATANTE { get; set; }
        public string? CLASE { get; set; }
        public string? PLACA { get; set; }
        public string? MARCA { get; set; }
        public string? MODELO { get; set; }
        public string? USO_SOAT { get; set; }
        public string? PRECIO { get; set; }
        public string? FIN_SOAT { get; set; }
        public string? SERIE { get; set; }
        public string? ASIENTOS { get; set; }
        public string? ANIO_FABRICACION { get; set; }
        public string? TIPO_DOCUMENTO { get; set; }
        public string? NRO_DOC_CONTRATANTE { get; set; }
        public string? DEPARTAMENTO { get; set; }
        public string? PROVINCIA { get; set; }
        public string? DISTRITO { get; set; }
        public string? DIRECCION_PROPIETARIO { get; set; }
        public string? CORREO { get; set; }
        public string? TELEFONO_1 { get; set; }
        public string? TELEFONO_2 { get; set; }
        public string? TELEFONO_3 { get; set; }
        public string? TELEFONO_4 { get; set; }
        public string? TELEFONO_5 { get; set; }
        public string? TELEF_NUEVO_1 { get; set; }
        public string? TELEF_NUEVO_2 { get; set; }
        public string? NUEVA_PLACA { get; set; }
        public string? OBSERVACION { get; set; }
    }

    public class PhoneNumberStatusContact
    {
        public CallableContact? TELEFONO_1 { get; set; }
    }

    public class CallableContact
    {
        public bool? callable { get; set; }
    }

    public class ContactableStatusContact
    {
        public ContactableContact? Email { get; set; }
    }
    public class ContactableContact
    {
        public bool? contactable { get; set;}
    }
    /*
        * ------------------------------------------------------------- 
    */
    public partial class CampaignProgress
    {
        public Campaign? Campaign { get; set; }
        public Campaign? ContactList { get; set; }
        public int NumberOfContactsCalled { get; set; }
        public int TotalNumberOfContacts { get; set; }
        public int Percentage { get; set; }
    }

    public partial class Campaign
    {
        public Guid Id { get; set; }
        public string SelfUri { get; set; }
    }
    /*
        * ------------------------------------------------------------- 
    */
    public partial class TokenResp
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }
    /*
        * ------------------------------------------------------------- 
    */

    public class CampaningListDetails
    {
        public int NID { get; set; }
        public string? SCAMPAIGNID { get; set; }
        public string? SCONTACTLISTID { get; set; }
        public int? NSTATUS { get; set; }
        public DateTime? HCAMPAIGN { get; set; }
        public int? ASTATUS { get; set; }
    }
    public class ListCampaing
    {
        public List<string> statusCampain { get; set; }
    }

    public partial class CampaingDetails
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public DateTimeOffset DateModified { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("contactList")]
        public CallAnalysisResponseSet ContactList { get; set; }

        [JsonProperty("queue")]
        public CallAnalysisResponseSet Queue { get; set; }

        [JsonProperty("dialingMode")]
        public string DialingMode { get; set; }

        [JsonProperty("script")]
        public CallAnalysisResponseSet Script { get; set; }

        [JsonProperty("site")]
        public CallAnalysisResponseSet Site { get; set; }

        [JsonProperty("campaignStatus")]
        public string CampaignStatus { get; set; }

        [JsonProperty("phoneColumns")]
        public PhoneColumn[] PhoneColumns { get; set; }

        [JsonProperty("abandonRate")]
        public long AbandonRate { get; set; }

        [JsonProperty("dncLists")]
        public object[] DncLists { get; set; }

        [JsonProperty("callAnalysisResponseSet")]
        public CallAnalysisResponseSet CallAnalysisResponseSet { get; set; }

        [JsonProperty("callerName")]
        public string CallerName { get; set; }

        [JsonProperty("callerAddress")]
        public string CallerAddress { get; set; }

        [JsonProperty("outboundLineCount")]
        public long OutboundLineCount { get; set; }

        [JsonProperty("ruleSets")]
        public object[] RuleSets { get; set; }

        [JsonProperty("skipPreviewDisabled")]
        public bool SkipPreviewDisabled { get; set; }

        [JsonProperty("previewTimeOutSeconds")]
        public long PreviewTimeOutSeconds { get; set; }

        [JsonProperty("singleNumberPreview")]
        public bool SingleNumberPreview { get; set; }

        [JsonProperty("alwaysRunning")]
        public bool AlwaysRunning { get; set; }

        [JsonProperty("noAnswerTimeout")]
        public long NoAnswerTimeout { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }

        [JsonProperty("contactListFilters")]
        public object[] ContactListFilters { get; set; }

        [JsonProperty("division")]
        public CallAnalysisResponseSet Division { get; set; }

        [JsonProperty("dynamicContactQueueingSettings")]
        public DynamicContactQueueingSettings DynamicContactQueueingSettings { get; set; }

        [JsonProperty("selfUri")]
        public string SelfUri { get; set; }
    }

    public partial class CallAnalysisResponseSet
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("selfUri")]
        public string SelfUri { get; set; }
    }

    public partial class DynamicContactQueueingSettings
    {
        [JsonProperty("sort")]
        public bool Sort { get; set; }
    }

    public partial class PhoneColumn
    {
        [JsonProperty("columnName")]
        public string ColumnName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
    public partial class ConfigCampaign
    {
        public string NID { get; set; }
        public string SCAMPAIGNID { get; set; }
        public string SCONTACTLISTID { get; set; }
    }
}