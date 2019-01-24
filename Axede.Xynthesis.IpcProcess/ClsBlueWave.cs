using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;
using ServicioXynthesis.Utilidades;
using System.Configuration;

namespace Axede.Xynthesis.IpcProcess
{
    public class nodo
    {
        public string lastModified { get; set; }
        public string id { get; set; }
        public string answeringPartyName { get; set; }
        public string cLIName { get; set; }
        public string cLINumber { get; set; }
        public string buttonNumber { get; set; }
        public string callType { get; set; }
        public string callUsage { get; set; }
        public string connectionId { get; set; }
        public string destination { get; set; }
        public string deviceChannel { get; set; }
        public string deviceChannelType { get; set; }
        public string deviceIdId { get; set; }
        public string displayInCallHistory { get; set; }
        public string duration { get; set; }
        public string e164Destination { get; set; }
        public string enterpriseCallId { get; set; }
        public string eventType { get; set; }
        public string parentUserCDIId { get; set; }
        public string personalPointOfContactId { get; set; }
        public string pointOfContactId { get; set; }
        public string priority { get; set; }
        public string pttDuration { get; set; }
        public string reasonForDisconnect { get; set; }
        public string resourceAORId { get; set; }
        public string ringTime { get; set; }
        public string rolloverAppearance { get; set; }
        public string routedDestination { get; set; }
        public string schemaDifference_blob_reserved { get; set; }
        public string schemaDifference_reserved { get; set; }
        public string startTime { get; set; }
        public string trunkBChannel { get; set; }
        public string trunkId { get; set; }
        public string userId { get; set; }
        public string EndTime { get; set; }
        public string EffectiveCallDuration { get; set; }
    }
    public class ClsBlueWave
    {
        CommonProcessLibrary.DataAccess.MySQLConnection oMysqlDB = new CommonProcessLibrary.DataAccess.MySQLConnection(ConfigurationManager.AppSettings["URLDataBaseXynthesis"].ToString());
        List<nodo> nod;
        string blueWaveUrlsession = ConfigurationManager.AppSettings["blueWaveUrlsession"].ToString();
        string blueWaveUrlHistorico = ConfigurationManager.AppSettings["blueWaveUrlHistorico"].ToString();

        LogServicioXynthesis lg = new LogServicioXynthesis();
        public ClsBlueWave()
        {

        }


        public string iniciaSession()
        {
            string AuthenticationToken = "";
            String fullFilePath = @"XMLsession.xml"; //Application.StartupPath + "\\" + @"XMLsession.xml";
            String uri = @blueWaveUrlsession;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.KeepAlive = false;
            request.Headers.Add("Authorization", "Basic " + "YXhlZGUxOmF4ZTExMDY=");
            request.ProtocolVersion = HttpVersion.Version10;
            request.ContentType = "application/xml";
            request.Method = "POST";

            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(fullFilePath))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
                byte[] postBytes = Encoding.UTF8.GetBytes(sb.ToString());
                request.ContentLength = postBytes.Length;

                try
                {
                    Stream requestStream = request.GetRequestStream();

                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();
                    string result = string.Empty;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        Console.WriteLine(response.ToString());

                        using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                        {
                            result = httpWebStreamReader.ReadToEnd();
                        }
                    }
                    XNamespace aw = "http://www.ipc.com/bw";
                    // XElement root = new XElement(aw + "AuthenticationToken", string.Empty);

                    XDocument doc = XDocument.Parse(result);
                    foreach (XElement element in doc.Descendants(aw + "AuthenticationToken"))
                    {
                        AuthenticationToken = element.Value;
                    }
                    //Console.WriteLine(result);

                    //Console.ReadLine();}
                    return AuthenticationToken;
                }
                catch (Exception ex)
                {

                    lg.EscribaLog("ServicioIpc", "Error en metodo iniciaSession (BlueWave):  " + ex.Message, "Administrador");
                    request.Abort();
                    return "";
                }
            }
        }

        public List<nodo> obtenerHistorico(string IPCAuthToken)
        {
                
            //String fullFilePath = @"XMLsession.xml";//Application.StartupPath + "\\" + @"XMLsession.xml";
            String uri = @blueWaveUrlHistorico + "?FilterType=datepage&starttime=3703017600&timezone=EST&timeformat=absolute";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            List<nodo> listaNodos = new List<nodo>();
            try
            {
                request.KeepAlive = false;
                request.Headers.Add("X-IPCAuthToken", IPCAuthToken);
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/xml";
                request.Method = "GET";
                string result = string.Empty;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(response.ToString());

                    using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                    {
                        result = httpWebStreamReader.ReadToEnd();
                    }
                }
                XNamespace aw = "http://www.ipc.com/bw";
                // XElement root = new XElement(aw + "AuthenticationToken", string.Empty);

                XDocument doc = XDocument.Parse(result);

                foreach (XElement element in doc.Descendants(aw + "CommunicationHistory"))
                {
                    nodo n = new nodo();
                    n.id = element.Element(aw + "id").Value;
                    n.lastModified = element.Element(aw + "lastModified").Value;
                    n.cLIName = element.Element(aw + "cLIName").Value;
                    n.cLINumber = element.Element(aw + "cLINumber").Value;
                    n.buttonNumber = element.Element(aw + "buttonNumber").Value;
                    n.callType = element.Element(aw + "callType").Value;
                    n.callUsage = element.Element(aw + "callUsage").Value;
                    n.destination = element.Element(aw + "destination").Value;
                    n.deviceChannel = element.Element(aw + "deviceChannel").Value;
                    n.deviceChannelType = element.Element(aw + "deviceChannelType").Value;
                    n.deviceIdId = element.Element(aw + "deviceIdId").Value;
                    n.displayInCallHistory = element.Element(aw + "displayInCallHistory").Value;
                    n.duration = element.Element(aw + "duration").Value;
                    n.e164Destination = element.Element(aw + "e164Destination").Value;
                    n.eventType = element.Element(aw + "eventType").Value;
                    n.parentUserCDIId = element.Element(aw + "parentUserCDIId").Value;
                    n.personalPointOfContactId = element.Element(aw + "personalPointOfContactId").Value;
                    n.pointOfContactId = element.Element(aw + "pointOfContactId").Value;
                    n.priority = element.Element(aw + "priority").Value;
                    n.reasonForDisconnect = element.Element(aw + "reasonForDisconnect").Value;
                    n.resourceAORId = element.Element(aw + "resourceAORId").Value;
                    n.rolloverAppearance = element.Element(aw + "rolloverAppearance").Value;
                    n.routedDestination = element.Element(aw + "routedDestination").Value;
                    n.startTime = element.Element(aw + "startTime").Value;
                    n.trunkBChannel = element.Element(aw + "trunkBChannel").Value;
                    n.trunkId = element.Element(aw + "trunkId").Value;
                    n.userId = element.Element(aw + "userId").Value;
                    listaNodos.Add(n);
                }
                return listaNodos;

            }
            catch (Exception ex)
            {

                lg.EscribaLog("ServicioIpc", "Error en metodo obtenerHistorico (BlueWave):  " + ex.Message, "Administrador");
                return null;
            }
        }

        public void cargaBlueWave()
        {
            double duracion = 0;
            DateTime EndTime_;
            string StartDateTime;
            string EndDateTime;
            List<nodo> listaData = new List<nodo>();

            oMysqlDB.OpenConnection();
            oMysqlDB.ExecuteQuery("xyp_delCommunicationhistory");
            oMysqlDB.CloseConnection();

            try
            {
                string sesion = iniciaSession();
                if (!sesion.Equals(""))
                {
                    listaData = obtenerHistorico(sesion);
                    lg.EscribaLog("ServicioIpc", "Se obtuvo informacion desde (BlueWave), con exito ", "Administrador");
                    for (int i = 0; i< listaData.Count;i++)
                    {
                        try
                        {
                            
                            double seconds = TimeSpan.Parse(listaData[i].duration).TotalSeconds;
                            duracion =  Convert.ToDouble(listaData[i].duration);
                            EndTime_ = Convert.ToDateTime(listaData[i].startTime.Substring(0, 19).Replace("T", " ")).AddSeconds(duracion);
                        }
                        catch
                        {
                            duracion = 0;
                            EndTime_ = Convert.ToDateTime(listaData[i].startTime.Substring(0, 19).Replace("T", " "));
                        }
                        StartDateTime = Convert.ToDateTime(listaData[i].startTime.Substring(0, 19).Replace("T", " ")).ToString("yyyyMMdd HH:mm:ss");
                        EndDateTime = EndTime_.ToString("yyyyMMdd HH:mm:ss");

                        List<object> param = new List<object>();
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("id_", listaData[i].id));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("answeringPartyName_", ""));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("buttonNumber_", listaData[i].buttonNumber));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("cLIName_", listaData[i].cLIName));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("cLINumber_", listaData[i].cLINumber));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("callType_", listaData[i].callType));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("callUsage_", listaData[i].callUsage));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("connectionId_", ""));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("destination_", listaData[i].destination));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("deviceChannel_", listaData[i].deviceChannel));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("deviceChannelType_", listaData[i].deviceChannelType));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("deviceIdId_", listaData[i].deviceIdId));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("displayInCallHistory_", null));
                        // param.Add(new MySql.Data.MySqlClient.MySqlParameter("duration_", TimeSpan.Parse(listaData[i].duration).TotalSeconds.ToString()));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("duration_", listaData[i].duration));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("e164Destination_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("enterpriseCallId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("eventType_", listaData[i].eventType));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("personalPointOfContactId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("pointOfContactId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("priority_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("pttDuration_", "0"));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("reasonForDisconnect_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("resourceAORId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("ringTime_", "0"));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("rolloverAppearance_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("routedDestination_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("schemaDifference_blob_reserved_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("schemaDifference_reserved_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("startTime_", StartDateTime));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("trunkId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("trunkBChannel_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("userId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("lastModified_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("parentUserCDIId_", null));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("EndTime_", EndDateTime));
                        param.Add(new MySql.Data.MySqlClient.MySqlParameter("EffectiveCallDuration_", listaData[i].duration.ToString()));
                        System.Data.CommandType cmType = System.Data.CommandType.StoredProcedure;

                        oMysqlDB.OpenConnection();
                        oMysqlDB.ExecuteOperationByConnectedMethod("xyp_inscommunicationhistory", cmType, param);
                        oMysqlDB.CloseConnection();


                    }
                    lg.EscribaLog("ServicioIpc", "Se termino el procesode cargue (BlueWave), con exito ", "Administrador");


                }
                else
                {
                    lg.EscribaLog("ServicioIpc", "Error en metodo cargaBlueWave (BlueWave): No se pudo iniciar session BlueWave ", "Administrador");
                }
            }
            catch (Exception ex)
            {
                lg.EscribaLog("ServicioIpc", "Error en metodo cargaBlueWave (BlueWave):  " + ex.Message, "Administrador");
                throw ex;
            }

        }

    }

}
