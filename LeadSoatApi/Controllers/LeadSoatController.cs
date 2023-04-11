using LeadSoatApi.Data;
using LeadSoatApi.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;
using LeadSoatApi.Data.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics.Contracts;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.X86;
using OracleInternal.Secure.Network;
using MimeKit;
using MailKit.Security;
using System;
using System.Threading;

namespace LeadSoatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadSoatController : ControllerBase
    {
        private readonly LeadSoatDbContext _myLeadSoatContext;
        private readonly ConfigLeadAuto configLeadAuto;
        private string? Token = null;
        private readonly string? ApiUrl = null;
        private readonly string? LoginUrl = null;
        private readonly string? TokenUrl = null;
        private readonly string connString = "User Id=INSUDB;Password=devtime55;Data Source=(DESCRIPTION=(ADDRESS= (PROTOCOL=tcp)(HOST=172.23.2.145)(PORT=1528))(CONNECT_DATA=(SERVICE_NAME=VISUALTIME7)))";
        public LeadSoatController(LeadSoatDbContext myLeadSoatContext, IConfiguration configuration)
        {
            _myLeadSoatContext = myLeadSoatContext;
            ApiUrl = configuration.GetValue<string>("Api:ApiUrl");
            LoginUrl = configuration.GetValue<string>("Api:LoginUrl");
            TokenUrl = configuration.GetValue<string>("Api:TokenUrl");
            configLeadAuto = new()
            {
                GenesysUsernameCode = configuration.GetValue<int>("ConfigLeadsAuto:GenesysUsernameCode"),
                GenesysPwdCode = configuration.GetValue<int>("ConfigLeadsAuto:GenesysPwdCode"),
                FromEmail = configuration.GetValue<int>("ConfigLeadsAuto:FromEmail"),
                FromPwdCode = configuration.GetValue<int>("ConfigLeadsAuto:FromPwdCode"),
                ToEmail = configuration.GetValue<int>("ConfigLeadsAuto:ToEmail"),
                Ncondition = configuration.GetValue<int>("ConfigLeadsAuto:Ncondition")
            };
        }
        private string? IdCampaignSelected;
        private string? IdContactListSelected;
        private int? ActionResult;
        private int? SdStatusSelected;

        /*ENCRIPTAR Y DESENCRIPTAR*/

        public static string Encrypt(string text)
        {
            string key = "A!9HHhi%XjjYY4YP2@Nob009X";
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }
        public static string Decrypt(string cipher)
        {
            string key = "A!9HHhi%XjjYY4YP2@Nob009X";
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.None;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return System.Text.UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }

        [HttpPost("Data/addContact")]
        public async Task<ActionResult> AddContactToList()
        {
            try
            {
                //String hourMinute = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                String hourMinute = DateTime.Now.ToString("dd/MM/yyyy 11:00");

                await GetTokenApi();

                QueryResponseModelContact response = new();

                /*
                    * Procedimiento que obtiene los leads para la creación de contactos 
                */

                string sql = "BEGIN INSUDB.PRC_REA_LEADS_SOAT(:HOURWDATE,:P_TABLE,:P_NCODE,:P_SMESSAGE); END;";

                var p1 = new OracleParameter { ParameterName = "HOURWDATE", Value = hourMinute, OracleDbType = OracleDbType.Varchar2 };
                var p3 = new OracleParameter { ParameterName = "P_TABLE", OracleDbType = OracleDbType.RefCursor, Direction = ParameterDirection.Output };
                var p4 = new OracleParameter { ParameterName = "P_NCODE", OracleDbType = OracleDbType.Int32, Direction = ParameterDirection.Output };
                var p5 = new OracleParameter { ParameterName = "P_SMESSAGE", OracleDbType = OracleDbType.Varchar2, Direction = ParameterDirection.Output, Size = 4000 };

                var leads = await _myLeadSoatContext.Set<Lead>().FromSqlRaw(sql, p1, p3, p4, p5).AsNoTracking().ToListAsync();

                if (leads.Count < 1 || leads[0].SCLIENT_NAME == null)
                {
                    List<Contact> contactos = new();

                    response.CodigoRespuesta = "400";
                    response.DescripcionResp = "No se encontraron leads para agregar, se realizó el reciclaje";

                    await GetCampaingDetails(contactos);
                    return BadRequest(response);
                }

                /*CREAR CONTACTOS*/

                List<Contact> contacts = new();

                foreach (Lead lead in leads)
                {
                    /*
                        * Creación del objeto de Contacto 
                    */
                    Contact contact = new()
                    {
                        contactListId = IdContactListSelected,

                        data = new()
                        {
                            NOMBRE_CONTRATANTE = lead.SCLIENT_NAME,
                            CLASE = "",
                            PLACA = lead.SREGIST,
                            MARCA = lead.SBRANCH,
                            MODELO = lead.MODELO,
                            USO_SOAT = "",
                            PRECIO = "",
                            FIN_SOAT = lead.DREGDATE,
                            SERIE = "",
                            ASIENTOS = "",
                            ANIO_FABRICACION = "",
                            TIPO_DOCUMENTO = lead.SSTEP,
                            NRO_DOC_CONTRATANTE = lead.SDOCUMENT,
                            DEPARTAMENTO = "",
                            PROVINCIA = "",
                            DISTRITO = "",
                            DIRECCION_PROPIETARIO = "",
                            CORREO = lead.SMAIL,
                            TELEFONO_1 = lead.SPHONE,
                            TELEFONO_2 = "",
                            TELEFONO_3 = "",
                            TELEFONO_4 = "",
                            TELEFONO_5 = "",
                            TELEF_NUEVO_1 = "",
                            TELEF_NUEVO_2 = "",
                            NUEVA_PLACA = "",
                            OBSERVACION = ""
                        },
                        callable = true,
                        phoneNumberStatus = new()
                        {
                            TELEFONO_1 = new()
                            {
                                callable = true
                            }
                        },
                        contactableStatus = new()
                        {
                            Email = new()
                            {
                                contactable = true
                            }
                        }
                    };
                    contacts.Add(contact);
                }


                response.CodigoRespuesta = "200";
                response.DescripcionResp = "Cargado con exito";
                response.Contacts?.AddRange(contacts);

                await GetCampaingDetails(contacts);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /*TAREAS PRINCIPALES*/

        private async Task<string> AddContactsToListApi(List<Contact> contacts)
        {

            /*
                * Api para guardar los contactos 
            */

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ApiUrl}/contactlists/{IdContactListSelected}/contacts"),
                Method = HttpMethod.Post
            };

            request.Headers.Add("Authorization", $"Bearer {Token}");

            var bodyString = JsonConvert.SerializeObject(contacts);

            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            request.Content = content;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
        private async Task<CampaignProgress> DeleteCampaignProgress()
        {
            /*
                * Api para reciclar el progreso de la campaña
            */
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ApiUrl}/campaigns/{IdCampaignSelected}/progress"),
                Method = HttpMethod.Delete
            };

            request.Headers.Add("Authorization", $"Bearer {Token}");
            var response = client.Send(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var CampaignProgress = JsonConvert.DeserializeObject<CampaignProgress>(await response.Content.ReadAsStringAsync());

            return CampaignProgress;
        }
        private async Task<CampaingDetails> GetCampaingDetails(List<Contact> contacts)
        {
            CampaingDetails? detail = null;
            var id = IdCampaignSelected;
            var sd = SdStatusSelected;
            var cd = IdContactListSelected;
            var action = ActionResult;
            var campaign = await GetCampaignsIds(); //aqui armo la tabla con las 4 campañas

            foreach ( DataRow row in campaign.Rows)
            {
                id = row["SCAMPAIGNID"].ToString();
                sd = Convert.ToInt32(row["NSTATUS"]);
                action = Convert.ToInt32(row["ASTATUS"]);
                cd = row["SCONTACTLISTID"].ToString();

                string urlDetailCampaigns = $"{ApiUrl}/campaigns/{id}";

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(urlDetailCampaigns),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("Authorization", $"Bearer {Token}");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                var campaingDetails = JsonConvert.DeserializeObject<CampaingDetails>(await response.Content.ReadAsStringAsync());

                detail = campaingDetails;

                /* Api para guardar los contactos */
                async Task<string> addContactAsync(List<Contact> contacts)
                {
                    /* Api para guardar los contactos */

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{ApiUrl}/contactlists/{cd}/contacts"),
                        Method = HttpMethod.Post
                    };

                    request.Headers.Add("Authorization", $"Bearer {Token}");

                    var bodyString = JsonConvert.SerializeObject(contacts);

                    var content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                    request.Content = content;

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(await response.Content.ReadAsStringAsync());
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    return result;
                }

                async Task<string> PutOffStatusAsync(CampaingDetails details)
                {
                    //Actualizar el estado de la campaña (on: Encendido, off: Apagado) 
                    details.CampaignStatus = "off";

                    //Api para actualizar campaña
                    var client = new HttpClient();

                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{ApiUrl}/campaigns/{id}"),
                        Method = HttpMethod.Put
                    };

                    request.Headers.Add("Authorization", $"Bearer {Token}");

                    var bodyString = JsonConvert.SerializeObject(details);

                    var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

                    request.Content = content;

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(await response.Content.ReadAsStringAsync());
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    return result;
                }

                async Task<string> PutOnStatusAsync(CampaingDetails details)
                {
                    //Actualizar el estado de la campaña (on: Encendido, off: Apagado) 
                    details.CampaignStatus = "on";

                    //Api para actualizar campaña
                    var client = new HttpClient();

                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{ApiUrl}/campaigns/{id}"),
                        Method = HttpMethod.Put
                    };

                    request.Headers.Add("Authorization", $"Bearer {Token}");

                    var bodyString = JsonConvert.SerializeObject(details);

                    var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

                    request.Content = content;

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(await response.Content.ReadAsStringAsync());
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    return result;
                }

                async Task<string> PostContactCleanAsync()
                {
                    /* Api para Limpiar La lista de Contactos */

                    var clientPost = new HttpClient();
                    var requestPost = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{ApiUrl}/contactlists/{cd}/clear"),
                        Method = HttpMethod.Post
                    };

                    requestPost.Headers.Add("Authorization", $"Bearer {Token}");

                    var responseHttpCLean = await clientPost.SendAsync(requestPost);

                    if (!responseHttpCLean.IsSuccessStatusCode)
                    {
                        throw new Exception(await responseHttpCLean.Content.ReadAsStringAsync());
                    }
                    var result = await responseHttpCLean.Content.ReadAsStringAsync();
                    return result;
                }

                async Task<CampaignProgress> DeleteProgressAsync()
                {
                    /* Api para reciclar el progreso de la campaña */
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{ApiUrl}/campaigns/{id}/progress"),
                        Method = HttpMethod.Delete
                    };

                    request.Headers.Add("Authorization", $"Bearer {Token}");
                    var response = client.Send(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(await response.Content.ReadAsStringAsync());
                    }

                    var CampaignProgress = JsonConvert.DeserializeObject<CampaignProgress>(await response.Content.ReadAsStringAsync());

                    return CampaignProgress;
                }

                if(contacts.Count() > 1000)
                {

                }
                if (action == 0)
                {
                    if (detail.CampaignStatus == "on")
                    {
                        await PutOffStatusAsync(detail);
                        Thread.Sleep(5000);
                        await PostContactCleanAsync();
                        Thread.Sleep(3000);
                        await addContactAsync(contacts);
                        //await AddContactsToListApi(contacts);
                        Thread.Sleep(5000);
                        if(detail.CampaignStatus == "off")
                        {
                            detail.Version += 2;
                            Thread.Sleep(5000);
                            await PutOnStatusAsync(detail); //Enciendo campaña
                        }                        
                    }
                    else
                    {
                        await addContactAsync(contacts);
                        Thread.Sleep(5000);
                        await PutOnStatusAsync(detail); //Enciendo campaña
                    }
                }
                else
                {
                    if (detail.CampaignStatus == "on")
                    {
                        await DeleteProgressAsync();
                    }
                    else if (detail.CampaignStatus == "off")
                    {
                        await PutOnStatusAsync(detail); //Enciendo campaña
                    }
                    else if (detail.CampaignStatus == "complete")
                    {
                        await DeleteProgressAsync();
                        Thread.Sleep(5000);
                        await PutOnStatusAsync(detail); //Enciendo campaña
                    }
                }
            }
            return detail;
        }
        private async Task<string> PutCampaingStatus(CampaingDetails details)
        {

            //Actualizar el estado de la campaña (on: Encendido, off: Apagado) 
            details.CampaignStatus = "off";

            //Api para actualizar campaña
            var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ApiUrl}/campaigns/{IdCampaignSelected}"),
                Method = HttpMethod.Put
            };

            request.Headers.Add("Authorization", $"Bearer {Token}");

            var bodyString = JsonConvert.SerializeObject(details);

            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            request.Content = content;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
        private async Task<string> PutCampaingStatusOn(CampaingDetails details)
        {

            //Actualizar el estado de la campaña (on: Encendido, off: Apagado) 
            details.CampaignStatus = "on";

            //Api para actualizar campaña
            var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ApiUrl}/campaigns/{IdCampaignSelected}"),
                Method = HttpMethod.Put
            };

            request.Headers.Add("Authorization", $"Bearer {Token}");

            var bodyString = JsonConvert.SerializeObject(details);

            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            request.Content = content;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
        private async Task<string> PostContactListClean()
        {

            /*
                * Api para Limpiar La lista de Contactos 
            */

            var clientPost = new HttpClient();
            var requestPost = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ApiUrl}/contactlists/{IdContactListSelected}/clear"),
                Method = HttpMethod.Post
            };

            requestPost.Headers.Add("Authorization", $"Bearer {Token}");

            var responseHttpCLean = await clientPost.SendAsync(requestPost);

            if (!responseHttpCLean.IsSuccessStatusCode)
            {
                throw new Exception(await responseHttpCLean.Content.ReadAsStringAsync());
            }
            var result = await responseHttpCLean.Content.ReadAsStringAsync();
            return result;
        }
        private async Task<string> GetTokenApi()
        {
            /*
                * Validación del Token 
            */

            bool validToken = await ValidateToken();

            if (validToken && Token != null)
            {
                return Token;
            }
            /*
                * Obtencion de Credenciales 
            */

            string? UserName = "";
            string? Password = "";

            OracleConnection conn = new(connString);

            var configUser = new System.Data.DataTable();

            configUser.Columns.Add("NCONDITION");
            configUser.Columns.Add("NFUNCTIONALITY");
            configUser.Columns.Add("SVALUE");

            if (conn.State == ConnectionState.Closed)
            {
                await conn.OpenAsync();
            }

            OracleDataAdapter da = new();

            // Obtener credenciales para acceder a la api de GENESYS

            OracleCommand cmd = new("INSUDB.PKG_AUTO_LEADS.PRC_AUTO_GET_USERS_CREDENTIALS", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("P_NCONDITION", OracleDbType.Int64).Value = configLeadAuto.Ncondition;
            cmd.Parameters.Add("C_TABLE", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            da.SelectCommand = cmd;

            cmd.ExecuteReader();

            da.Fill(configUser);
            conn.Close();

            foreach (DataRow row in configUser.Rows)
            {
                if (DBNull.Value.Equals(row["NFUNCTIONALITY"]) && DBNull.Value.Equals(row["SVALUE"]))
                {
                    continue;
                }

                _ = int.TryParse(row["NFUNCTIONALITY"].ToString(), out int functionality);

                if (functionality == configLeadAuto.GenesysUsernameCode)
                {
                    UserName = Regex.Replace(Decrypt(row["SVALUE"].ToString()), @"[^\t\r\n -~]", "").Trim();
                }
                else if (functionality == configLeadAuto.GenesysPwdCode)
                {
                    Password = Regex.Replace(Decrypt(row["SVALUE"].ToString()), @"[^\t\r\n -~]", "").Trim();
                }

            }

            /*
                * Generación del Token
            */

            var plainTextBytes = Encoding.UTF8.GetBytes(UserName + ":" + Password);

            var base64 = Convert.ToBase64String(plainTextBytes);

            string urlApiToken = LoginUrl ?? "";

            var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(urlApiToken),
                Method = HttpMethod.Post
            };

            request.Headers.Add("Authorization", $"Basic {base64}");

            var formList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            request.Content = new FormUrlEncodedContent(formList);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            string result = await response.Content.ReadAsStringAsync();

            var tokenResp = JsonConvert.DeserializeObject<TokenResp>(result);

            Token = tokenResp.AccessToken;

            return tokenResp.AccessToken;
        }
        private async Task<Boolean> ValidateToken()
        {

            if (Token == null)
            {
                return false;
            }

            /*
                * Api para verificar si el token guardado es valido 
            */

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(TokenUrl ?? ""),
                Method = HttpMethod.Get
            };

            request.Headers.Add("Authorization", $"Bearer {Token}");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private async Task<DataTable> GetCampaignsIds()
        {
            try
            {

                string prcName = "INSUDB.PKG_AUTO_LEADS.PRC_AUTO_GET_CAMPAIGN_IDS";

                OracleConnection conn = new(connString);

                var configCampaign = new System.Data.DataTable();

                configCampaign.Columns.Add("NID");
                configCampaign.Columns.Add("SCAMPAIGNID");
                configCampaign.Columns.Add("SCONTACTLISTID");
                configCampaign.Columns.Add("NSTATUS");
                configCampaign.Columns.Add("HCAMPAIGN");
                configCampaign.Columns.Add("ASTATUS");

                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }

                OracleDataAdapter da = new();

                // Obtener lista de las campañas disponibles junto a la lista de contactos correspondiente

                OracleCommand cmd = new(prcName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("C_TABLE", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                da.SelectCommand = cmd;

                cmd.ExecuteReader();

                da.Fill(configCampaign);

                return configCampaign;

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}