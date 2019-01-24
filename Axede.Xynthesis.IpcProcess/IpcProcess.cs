using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using CommonProcessLibrary.TO;
using CommonProcessLibrary;
using CommonProcessLibrary.Utils;
using CommonProcessLibrary.DataAccess;
using System.Globalization;
using System.Configuration;
using CommonProcessLibrary.FTPProcess;
using System.IO;
using System.Text.RegularExpressions;
using ServicioXynthesis.Utilidades;

namespace Axede.Xynthesis.IpcProcess
{
    public class IpcProcess
    {
        private LogServicioXynthesis lg = new LogServicioXynthesis();
        //Axede.Xynthesis.IpcProcess.IpcProcess pro = new IpcProcess.IpcProcess();
        MySQLConnection oMysqlDB = new MySQLConnection(ConfigurationManager.AppSettings["URLDataBaseXynthesis"].ToString());
        LogServicioXynthesis Log = new LogServicioXynthesis();
        readonly string nroCampos = ConfigurationManager.AppSettings["nroCampos"].ToString();
        readonly string rutaArcplano = ConfigurationManager.AppSettings["ruta_ipc_csv"].ToString();
        string nombreCampos = ConfigurationManager.AppSettings["nombreCampos"].ToString();
        readonly string nombre_ipc_csv = ConfigurationManager.AppSettings["nombre_ipc_csv"].ToString();
        public string OpcionCargue;
        public IConnection connXynthesis;
        public Logger logger = Logger.GetInstance();
        //CommonProcessLibrary.DataAccess.MySQLConnection oMysqlDB;

        public IpcProcess()
        { }

        public string GetStartDateTickets()
        {
            string sStartDatetickets = null;
            try
            {
                connXynthesis = new MySQLConnection(ConfigurationManager.AppSettings["URLDataBaseXynthesis"]);
                connXynthesis.OpenConnection();
                MySqlDataReader msdrQueryResult = connXynthesis.ExecuteQuery("xyp_SelMaxDateTickets", new object[] {
                    new MySqlParameter("OpcionCargue", "NORMAL")
                });
                if (msdrQueryResult.Read())
                {
                    if (!msdrQueryResult.IsDBNull(msdrQueryResult.GetOrdinal("StartDateTime")))
                        sStartDatetickets = Convert.ToString(msdrQueryResult.GetString("StartDateTime"));
                }
                msdrQueryResult.Dispose();
                msdrQueryResult = null;
                connXynthesis.CloseConnection();

                return sStartDatetickets;
            }
            catch (Exception eException)
            {

                logger.Error("Error en carga de tickets Ipc: " + eException.Message, eException);
                throw new Exception("Error capturando la última fecha de carga de tickets:\n" + "xyp_SelMaxDateTickets"
                    + "\n\nDetalle del error:\n" + eException.Message);

            }
        }

        public void Execute(string v)
        {
            try
            {

                ExecuteETL("NORMAL");
                oMysqlDB.OpenConnection();
                oMysqlDB.ExecuteOperation("call xyp_Execute_ETL_Ipc()");
                oMysqlDB.CloseConnection();

            }
            catch (Exception eException)
            {
                logger.Error("Error en carga de tickets Ipc: " + eException.Message, eException);
            }
        }

        public void ExecuteETL(String OpcionCargue_)
        {
            OpcionCargue = OpcionCargue_;
            try
            {
                oMysqlDB.OpenConnection();
                oMysqlDB.ExecuteQuery("xyp_delCommunicationhistory");
                oMysqlDB.CloseConnection();
                ExtracInfoCsv();
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo ExecuteETL : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void TransforCalls()
        {
            try
            {
                oMysqlDB.OpenConnection();
                oMysqlDB.ExecuteQuery("xyp_ExtractionTicketsIPC"); //transfroacion hacia xy_ticketinfo
                oMysqlDB.CloseConnection();

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo transforCalls : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void ActualiUsuarios()
        {
            try
            {
                oMysqlDB.OpenConnection();
                oMysqlDB.ExecuteQuery("xyp_Execute_ETL_Ipc");  //validando usuarios en el sistema, 
                oMysqlDB.CloseConnection();

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo actualiUsuarios : " + ex.Message, "Administrador");
                throw ex;
            }
        }


        public StreamReader ExtracInfoCsv_(string ftp_, string usuario, string clave)
        {
            try
            {
                FtpIPC ftp = new FtpIPC();
                return ftp.descargaArchivoFtp(ftp_, usuario, clave);
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo extracInfoCsv : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void ExtracInfoCsv()
        {
            Boolean esEncabezado = false;
            string cadSql = "";
            int linea = 0, lineEncabezado = 0;
            string idLlamada = "";
            string rutaCompleta = rutaArcplano + "/" + nombre_ipc_csv;
            double duracion = 0;
            DateTime EndTime_;
            string StartDateTime;
            string EndDateTime;
            List<nodo> listaIpc = new List<nodo>();
            try
            {
                StreamReader reader;
                string[] ncampo = nombreCampos.Split(';');
                reader = new StreamReader(@rutaCompleta);

                using (reader)
                {

                    while (!reader.EndOfStream)
                    {

                        string[] values = reader.ReadLine().Split(';');
                        if (!esEncabezado)
                        {
                            // Validar nombres de os campos
                            int cuenta = 0;
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (!values[i].ToUpper().Equals(ncampo[i].ToUpper()))
                                {
                                    esEncabezado = false;
                                }
                                else
                                {
                                    cuenta = cuenta + 1;
                                }
                            }
                            if (cuenta == values.Length)
                            {
                                esEncabezado = true;
                                lineEncabezado = linea;
                            }

                        }

                        if (esEncabezado)
                        {

                            var a = values.Length;
                            var b = int.Parse(nroCampos);
                            var c = values[0].IndexOf(IpcConstants.sIdenllamada);
                            var e = IpcConstants.sOmitirRegistro;
                            var d = values[0];


                            if (values[0].IndexOf(IpcConstants.sIdenllamada) >= 0)
                            {
                                //linea que identifica la llamada
                                idLlamada = values[0];
                            }
                            if (values.Length == int.Parse(nroCampos) && values[0].IndexOf(IpcConstants.sIdenllamada) == -1 && linea > lineEncabezado && values[0].IndexOf(IpcConstants.sOmitirRegistro) == -1)
                            {
                                // Validacion de la data


                                try
                                {

                                    double seconds = TimeSpan.Parse(values[6]).TotalSeconds;
                                    //duracion = seconds + Convert.ToDouble(values[7]) + Convert.ToDouble(values[8]);
                                    duracion = Convert.ToDouble(values[7]) + Convert.ToDouble(values[8]);
                                    EndTime_ = Convert.ToDateTime(values[0]).AddSeconds(duracion);
                                }
                                catch
                                {
                                    duracion = 0;
                                    EndTime_ = Convert.ToDateTime(values[0]);
                                }
                                StartDateTime = Convert.ToDateTime(values[0]).ToString("yyyyMMdd HH:mm:ss");
                                EndDateTime = EndTime_.ToString("yyyyMMdd HH:mm:ss");
                                nodo n = new nodo
                                {
                                    id = idLlamada,
                                    answeringPartyName = values[10],
                                    buttonNumber = null,
                                    cLIName = values[1],
                                    cLINumber = values[4],
                                    callType = values[2],
                                    callUsage = null,
                                    connectionId = values[9],
                                    destination = values[5],
                                    deviceChannel = null,
                                    deviceChannelType = null,
                                    deviceIdId = null,
                                    displayInCallHistory = null,
                                    duration = TimeSpan.Parse(values[6]).TotalSeconds.ToString(),
                                    e164Destination = null,
                                    enterpriseCallId = null,
                                    eventType = values[3],
                                    personalPointOfContactId = null,
                                    pointOfContactId = null,
                                    priority = null,
                                    pttDuration = values[8],
                                    reasonForDisconnect = null,
                                    resourceAORId = null,
                                    rolloverAppearance = null,
                                    ringTime = values[7]
                                };
                                n.rolloverAppearance = null;
                                n.routedDestination = null;
                                n.schemaDifference_blob_reserved = null;
                                n.schemaDifference_reserved = null;
                                n.startTime = StartDateTime;
                                n.trunkId = null;
                                n.trunkBChannel = null;
                                n.userId = null;
                                n.lastModified = null;
                                n.parentUserCDIId = null;
                                n.EndTime = EndTime_.ToString("yyyyMMdd HH:mm:ss");
                                n.EffectiveCallDuration = duracion.ToString();
                                listaIpc.Add(n);

                            }
                            else
                            {
                                // el numero de campos no coincide
                                //Log.EscribaLog("ConnectToDBIpc", "Error en metodo extracInfoCsv, el numero de campos exigido es : " +nroCampos+", y se reportan "+ values.Length .ToString()+ "  \n   " , "Administrador");
                                // break;
                            }

                        }
                        linea = linea + 1;
                    }  ///while

                }
                reader.Close();
                string ErrorMensage = "";
                // session de validaacion data del archivo.
                for (int j = 0; j < listaIpc.Count; j++)
                {
                    if (!DateTime.TryParse(listaIpc[j].startTime, out DateTime fec))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos fecha llamada inicial " + listaIpc[j].startTime + " en fila " + j.ToString() + "\n";

                    if (!DateTime.TryParse(listaIpc[j].EndTime, out fec))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos fecha llamada final" + listaIpc[j].EndTime + " en fila " + j.ToString() + "\n";

                    if (!double.TryParse(listaIpc[j].cLINumber, out double num))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos telefono origen " + listaIpc[j].cLINumber + " en fila " + j.ToString() + "\n";

                    if (!double.TryParse(listaIpc[j].destination, out num))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos telefono destino" + listaIpc[j].destination + " en fila " + j.ToString() + "\n";

                    if (EsHoraValida(listaIpc[j].ringTime))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos telefono ringtime" + listaIpc[j].ringTime + " en fila " + j.ToString() + "\n";

                    if (!double.TryParse(listaIpc[j].connectionId, out num))
                        ErrorMensage = ErrorMensage + " Error en el tipo de datos connectionId " + listaIpc[j].connectionId + " en fila " + j.ToString() + "\n";

                }
                if (ErrorMensage.Equals(""))
                {
                    //cargar desde la lista a la base tabla temporal
                    for (int v = 0; v < listaIpc.Count; v++)
                    {
                        oMysqlDB.OpenConnection();
                        List<object> param = new List<object>
                        {
                            new MySql.Data.MySqlClient.MySqlParameter("id_", listaIpc[v].id),
                            new MySql.Data.MySqlClient.MySqlParameter("answeringPartyName_", listaIpc[v].answeringPartyName),
                            new MySql.Data.MySqlClient.MySqlParameter("buttonNumber_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("cLIName_", listaIpc[v].cLIName),
                            new MySql.Data.MySqlClient.MySqlParameter("cLINumber_", listaIpc[v].cLINumber),
                            new MySql.Data.MySqlClient.MySqlParameter("callType_", listaIpc[v].callType),
                            new MySql.Data.MySqlClient.MySqlParameter("callUsage_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("connectionId_", listaIpc[v].connectionId),
                            new MySql.Data.MySqlClient.MySqlParameter("destination_", listaIpc[v].destination),
                            new MySql.Data.MySqlClient.MySqlParameter("deviceChannel_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("deviceChannelType_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("deviceIdId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("displayInCallHistory_", null),
                            //param.Add(new MySql.Data.MySqlClient.MySqlParameter("duration_", TimeSpan.Parse(values[6]).TotalSeconds.ToString()));
                            new MySql.Data.MySqlClient.MySqlParameter("duration_", listaIpc[v].duration),
                            new MySql.Data.MySqlClient.MySqlParameter("e164Destination_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("enterpriseCallId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("eventType_", listaIpc[v].eventType),
                            new MySql.Data.MySqlClient.MySqlParameter("personalPointOfContactId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("pointOfContactId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("priority_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("pttDuration_", listaIpc[v].pttDuration),
                            new MySql.Data.MySqlClient.MySqlParameter("reasonForDisconnect_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("resourceAORId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("ringTime_", listaIpc[v].ringTime),
                            new MySql.Data.MySqlClient.MySqlParameter("rolloverAppearance_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("routedDestination_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("schemaDifference_blob_reserved_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("schemaDifference_reserved_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("startTime_", listaIpc[v].startTime),
                            new MySql.Data.MySqlClient.MySqlParameter("trunkId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("trunkBChannel_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("userId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("lastModified_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("parentUserCDIId_", null),
                            new MySql.Data.MySqlClient.MySqlParameter("EndTime_", listaIpc[v].EndTime),
                            new MySql.Data.MySqlClient.MySqlParameter("EffectiveCallDuration_", listaIpc[v].duration)
                        };

                        System.Data.CommandType cmType = System.Data.CommandType.StoredProcedure;
                        oMysqlDB.ExecuteOperationByConnectedMethod("xyp_inscommunicationhistory", cmType, param);
                        oMysqlDB.CloseConnection();
                    }
                    TransforCalls();
                    lg.EscribaLog("ServicioIpc", "ServicioIpc; termino el proceso con exito", "Administrador");
                }
                else
                {
                    lg.EscribaLog("ServicioIpc", "ServicioIpc; fallido el proceso de cargue con errores :: " + ErrorMensage, "Administrador");
                }

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo extracInfoCsv :" + cadSql + " proceso linea " + linea.ToString() + "\n" + ex.Message, "Administrador");
                throw ex;
            }

        }

        public Boolean EsHoraValida(string hora)
        {
            string pattern = "\\d{1,2}:\\d{2}:\\d{2}";
            if (hora != null)
            {
                if (!Regex.IsMatch(hora, pattern, RegexOptions.CultureInvariant))
                {
                    return false;
                }
                else
                {
                    if (double.Parse(hora.Substring(0, 2)) < 0 || double.Parse(hora.Substring(0, 2)) > 24)
                    {
                        return false;
                    }
                    else
                    {
                        if (double.Parse(hora.Substring(3, 2)) < 0 || double.Parse(hora.Substring(3, 2)) > 59)
                        {
                            return false;
                        }
                        else
                        {
                            if (double.Parse(hora.Substring(6, 2)) < 0 || double.Parse(hora.Substring(6, 2)) > 59)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }


    }
}
