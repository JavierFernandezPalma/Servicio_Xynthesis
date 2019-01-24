using Modelo;
using ServicioXynthesis.Utilidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axede.Xynthesis.IpcProcess
{
    public class IpcProcess2
    {

        LogServicioXynthesis Log = new LogServicioXynthesis();
        readonly string rutaArcplano = ConfigurationManager.AppSettings["ruta_ipc_csv"].ToString();
        readonly string nombre_ipc_csv = ConfigurationManager.AppSettings["nombre_ipc_csv"].ToString();
        xynthesisEntities bd_Xynthesis = new xynthesisEntities();


        public void ExtracInfoCsv()
        {
            string cadSql = "";
            string registroSinEspacios = null;
            int linea = 0;
            string rutaCompleta = rutaArcplano + "/" + nombre_ipc_csv;
            ArrayList arrText = new ArrayList();

            try
            {
                StreamReader reader;
                reader = new StreamReader(@rutaCompleta);

                while (cadSql != null)
                {

                    cadSql = reader.ReadLine();


                    if (cadSql != null)
                    {
                        
                        string[] values = cadSql.Split(',');
                        var arrayRegistro = values.ToArray();

                        foreach (var item in arrayRegistro)
                        {
                            registroSinEspacios += item.Trim() + ";";
                        }

                        string[] values2 = registroSinEspacios.Split(';');
                        var arrayRegistro2 = values2.ToArray();

                        xy_ipc_communicationhistory t_history = new xy_ipc_communicationhistory()
                        {
                            //id = arrayRegistro2[0],
                            //answeringPartyName = arrayRegistro2[6],
                            //buttonNumber = arrayRegistro2[47],
                            //cLIName = arrayRegistro2[3],
                            //cLINumber = arrayRegistro2[47],
                            //callType = arrayRegistro2[9],
                            //callUsage = arrayRegistro2[47],
                            //connectionId = arrayRegistro2[47],
                            //destination = arrayRegistro2[47],
                            //deviceChannel = arrayRegistro2[47],
                            //deviceChannelType = arrayRegistro2[47],
                            //deviceIdId = arrayRegistro2[47],
                            //displayInCallHistory = arrayRegistro2[47],
                            //duration = arrayRegistro2[14],
                            //e164Destination = arrayRegistro2[47],
                            //enterpriseCallId = arrayRegistro2[47],
                            //eventType = arrayRegistro2[47],
                            //personalPointOfContactId = arrayRegistro2[18],
                            //pointOfContactId = arrayRegistro2[47],
                            //priority = arrayRegistro2[47],
                            //pttDuration = arrayRegistro2[36],
                            //reasonForDisconnect = arrayRegistro2[47],
                            //resourceAORId = arrayRegistro2[47],
                            //ringTime = arrayRegistro2[47],
                            //rolloverAppearance = arrayRegistro2[47],
                            //routedDestination = arrayRegistro2[47],
                            //schemaDifference_blob_reserved = arrayRegistro2[47],
                            //schemaDifference_reserved = arrayRegistro2[47],
                            //startTime = arrayRegistro2[39],
                            //trunkId = arrayRegistro2[15],
                            //trunkBChannel = arrayRegistro2[47],
                            //userId = arrayRegistro2[47],
                            //lastModified = arrayRegistro2[47],
                            //parentUserCDIId = arrayRegistro2[47],
                            //EndTime = arrayRegistro2[11],
                            //EffectiveCallDuration = arrayRegistro2[37]

                            id = Convert.ToString(2),
                            answeringPartyName = "",
                            buttonNumber = "",
                            cLIName = "",
                            cLINumber = "",
                            callType = "",
                            callUsage = "",
                            connectionId = "",
                            destination = "",
                            deviceChannel = "",
                            deviceChannelType = "",
                            deviceIdId = "",
                            displayInCallHistory = "",
                            duration = "",
                            e164Destination = "",
                            enterpriseCallId = "",
                            eventType = "",
                            personalPointOfContactId = "",
                            pointOfContactId = "",
                            priority = "",
                            pttDuration = "",
                            reasonForDisconnect = "",
                            resourceAORId = "",
                            ringTime = "",
                            rolloverAppearance = "",
                            routedDestination = "",
                            schemaDifference_blob_reserved = "",
                            schemaDifference_reserved = "",
                            startTime = "",
                            trunkId = "",
                            trunkBChannel = "",
                            userId = "",
                            lastModified = "",
                            parentUserCDIId = "",
                            EndTime = "",
                            EffectiveCallDuration = ""
                        };

                        //bd_Xynthesis.Configuration.ValidateOnSaveEnabled = false;
                        bd_Xynthesis.xy_ipc_communicationhistory.Add(t_history);
                        bd_Xynthesis.SaveChanges();

                        //arrText.Add(registroSinEspacios);
                    }
                        
                }
                reader.Close();

                //foreach (string sOutput in arrText) Console.WriteLine(sOutput);
                //Console.ReadLine();

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo extracInfoCsv :" + registroSinEspacios + " proceso linea " + linea.ToString() + "\n" + ex.Message, "Administrador");
                throw ex;
            }


        }


    }
}
