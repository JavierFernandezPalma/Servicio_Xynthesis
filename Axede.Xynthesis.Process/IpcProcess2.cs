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

namespace Axede.Xynthesis.Process
{
    public class IpcProcess2
    {

        LogServicioXynthesis Log = new LogServicioXynthesis();
        readonly string rutaArcplano = ConfigurationManager.AppSettings["ruta_taxa_data"].ToString();
        readonly string rutaTaxaEjecutado = ConfigurationManager.AppSettings["ruta_taxa_ejecutado"].ToString();
        readonly string fecha_inicial_cargue = ConfigurationManager.AppSettings["fecha_inicial_cargue"].ToString();
        //readonly string nombre_ipc_csv = ConfigurationManager.AppSettings["nombre_ipc_csv"].ToString();
        xynthesisEntities bd_Xynthesis = new xynthesisEntities();

        public IpcProcess2()
        {

        }

        public void ExtracInfoCsv(List<string> FicherosTaxa)
        {
            string registroSinEspacios = null;
            int fila = 0;
            try
            {

                for (int i = 0; i < FicherosTaxa.Count; i++)
                {
                    fila = 0;
                    string filaTaxa = "";
                    string nombreFichero = FicherosTaxa[i];
                    string rutaCompleta = rutaArcplano + "/" + nombreFichero;
                    StreamReader reader;
                    reader = new StreamReader(@rutaCompleta, Encoding.Default);

                    while (filaTaxa != null)
                    {
                        filaTaxa = reader.ReadLine();

                        if (filaTaxa != null)
                        {
                            fila = fila + 1;
                            string[] valuesFila = filaTaxa.Split(',');
                            var arrayRegistro = valuesFila.ToArray();

                            foreach (var item in arrayRegistro)
                            {
                                registroSinEspacios += item.Trim() + ";";
                            }

                            string[] valuesFila2 = registroSinEspacios.Split(';');
                            var arrayRegistro2 = valuesFila2.ToArray();

                            double duracion = Convert.ToDouble(arrayRegistro2[36] + Convert.ToDouble(arrayRegistro2[37]));
                            string fechaFinal = arrayRegistro2[11].Substring(0, 4) + "/" + arrayRegistro2[11].Substring(4, 2) + "/" + arrayRegistro2[11].Substring(6, 2) + " " + arrayRegistro2[11].Substring(9, 8);
                            DateTime EndTime_ = Convert.ToDateTime(fechaFinal).AddSeconds(duracion);
                            //string StartDateTime = arrayRegistro2[39].Substring(0, 4) + "/" + arrayRegistro2[39].Substring(4, 2) + "/" + arrayRegistro2[39].Substring(6, 2) + " " + arrayRegistro2[39].Substring(9, 8);
                            string StartDateTime = Convert.ToDateTime(arrayRegistro2[39].Substring(6, 2) + "/" + arrayRegistro2[39].Substring(4, 2) + "/" + arrayRegistro2[39].Substring(0, 4) + " " + arrayRegistro2[39].Substring(9, 8)).ToString("yyyy/MM/dd HH:mm:ss");



                            bd_Xynthesis.xyp_inscommunicationhistory(

                                /*id*/  arrayRegistro2[2] + "_" + arrayRegistro2[39], //Concatena 
                                                                                      /*answeringPartyName*/  arrayRegistro2[6],
                                /*buttonNumber*/  null,
                                /*OKcLIName*/  arrayRegistro2[3],
                                /*OKcLINumber*/  arrayRegistro2[2],
                                /*OKcallType*/  arrayRegistro2[9],
                                /*callUsage*/  null,
                                /*connectionId*/  arrayRegistro2[0],
                                /*OKdestination*/  arrayRegistro2[1],
                                /*deviceChannel*/  null,
                                /*deviceChannelType*/  null,
                                /*deviceIdId*/  null,
                                /*displayInCallHistory*/  null,
                                /*OKduration*/  arrayRegistro2[14],
                                /*e164Destination*/  null,
                                /*enterpriseCallId*/  null,
                                /*eventType*/  arrayRegistro2[18],
                                /*personalPointOfContactId*/ null,
                                /*pointOfContactId*/  null,
                                /*priority*/  null,
                                /*pttDuration*/  arrayRegistro2[37],
                                /*reasonForDisconnect*/  null,
                                /*resourceAORId*/  null,
                                /*OKringTime*/  arrayRegistro2[36],
                                /*rolloverAppearance*/  null,
                                /*routedDestination*/  null,
                                /*schemaDifference_blob_reserved*/  null,
                                /*schemaDifference_reserved*/  null,
                                /*startTime*/  StartDateTime,
                                /*trunkId*/  arrayRegistro2[15],
                                /*trunkBChannel*/  null,
                                /*userId*/  null,
                                /*lastModified*/  null,
                                /*parentUserCDIId*/  null,
                                /*OKEndTime*/  EndTime_.ToString("yyyyMMdd HH:mm:ss"),
                                /*EffectiveCallDuration*/  duracion.ToString()
                            );

                        }
                        registroSinEspacios = null;

                    }
                    reader.Close();

                    //foreach (string sOutput in arrText) Console.WriteLine(sOutput);
                    //Console.ReadLine();
                }


            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo extracInfoCsv :" + registroSinEspacios + " proceso linea " + fila.ToString() + "\n" + ex.Message, "Administrador");
                throw ex;
            }


        }


        public int ExtracInfoTaxa(List<string> FicherosTaxa)
        {
            string registroSinEspacios = null;
            string nombreFichero = null;
            int fila = 0, maximoCargue = 0;
           
            int MaximaFechaTickets = 0;

            MaximaFechaTickets = (from db in bd_Xynthesis.xy_ticketsoxe select db).Count();

            try
            {
                for (int i = 0; i < FicherosTaxa.Count; i++)
                {
                    fila = 0;
                    string filaTaxa = "";
                    nombreFichero = FicherosTaxa[i];
                    string rutaArchivoTaxa = rutaArcplano + "/" + nombreFichero;
                    StreamReader reader;
                    reader = new StreamReader(rutaArchivoTaxa, Encoding.Default); //Encoding.Default para lectura de caracteres especiales


                    while (filaTaxa != null)
                    {

                        try
                        {

                            decimal ChargedCostCenter = 0;
                            filaTaxa = reader.ReadLine();

                            if (filaTaxa != null)
                            {
                                fila = fila + 1;
                                
                                string[] valuesFila = filaTaxa.Split(',');
                                var arrayRegistro = valuesFila.ToArray();

                                foreach (var item in arrayRegistro)
                                {
                                    registroSinEspacios += item.Trim() + ";";
                                }
                                registroSinEspacios = registroSinEspacios.TrimEnd(';');
                                string[] valuesFila2 = registroSinEspacios.Split(';');
                                var arrayRegistro2 = valuesFila2.ToArray();


                                if (arrayRegistro2[14] != "" && Convert.ToInt32(arrayRegistro2[14]) != 0)
                                {
                                    List<string> UserOxe = ValidarUserOxe(arrayRegistro2); // Valida que el registro tenga asociado un centro de costos, en caso contrario busca el correspondiente


                                    string ChargedNumber = UserOxe[0];
                                    string CostCenter = UserOxe[2];
                                    string CalledNumber = arrayRegistro2[1];
                                    int Coverage = 1;

                                    if (arrayRegistro2[35] != "" && arrayRegistro2[35] != null)
                                    {
                                        Coverage = IdentificaciónCobertura(arrayRegistro2[35]); // Validar la cobertura de la llamada
                                    }

                                    if (arrayRegistro2[1] != "" && arrayRegistro2[1] != null && Coverage == 1)
                                    {
                                        Coverage = IdentificaciónCobertura(arrayRegistro2[1]);
                                    }


                                    if (CalledNumber == "" || CalledNumber == null)
                                    {
                                        CalledNumber = "0";
                                    }


                                    if (CostCenter != "" && CostCenter != null)
                                    {
                                        ChargedCostCenter = Convert.ToDecimal(CostCenter);
                                    }

                                    var horaFinal = Convert.ToDateTime(arrayRegistro2[11].Substring(6, 2) + "/" + arrayRegistro2[11].Substring(4, 2) + "/" + arrayRegistro2[11].Substring(0, 4) + " " + arrayRegistro2[11].Substring(9, 8)).TimeOfDay;

                                    string Ide_TicketsOxe = ChargedNumber + arrayRegistro2[39] + horaFinal + CalledNumber;

                                    bool validarTickets = (from db in bd_Xynthesis.xy_ticketsoxe where db.Ide_TicketsOxe == Ide_TicketsOxe select db).Any();

                                    if (!validarTickets)
                                    {
                                        bd_Xynthesis.xyp_Add_xy_ticketsoxe(

                                        /*Ide_TicketsOxe*/ Ide_TicketsOxe,
                                            /*ChargedUserName*/ UserOxe[1],
                                            /*ChargedNumber*/ ChargedNumber,
                                            /*CalledNumber*/ CalledNumber,
                                            /*CallType*/ Convert.ToSByte(arrayRegistro2[9]),
                                            /*StartDateTime*/ Convert.ToDateTime(arrayRegistro2[39].Substring(6, 2) + "-" + arrayRegistro2[39].Substring(4, 2) + "-" + arrayRegistro2[39].Substring(0, 4) + " " + arrayRegistro2[39].Substring(9, 8)),
                                            /*Duration*/ Convert.ToInt32(arrayRegistro2[14]),
                                            /*PersonalOrBusiness*/ arrayRegistro2[18],
                                            /*AccessCode*/ arrayRegistro2[19],
                                            /*TrunkIdentity*/ arrayRegistro2[15],
                                            /*Ide_Ticket*/ Coverage,
                                            /*EffectiveCallDuration*/ arrayRegistro2[37],
                                            /*WaitingDuration*/ arrayRegistro2[36],
                                            /*CallingNumber*/ arrayRegistro2[8],
                                            /*ChargedCostCenter*/ ChargedCostCenter,
                                            /*Taxa*/ nombreFichero,
                                            /*Linea_Taxa*/ fila
                                    );
                                        maximoCargue = maximoCargue + 1;
                                    }
                                    else
                                    {

                                            xy_ticketsoxe taxa_linea = (from db in bd_Xynthesis.xy_ticketsoxe where db.Ide_TicketsOxe == Ide_TicketsOxe select db).First();
                                            Log.EscribaLog("ExtracInfoTaxa_Repetido", "Fichero: " + nombreFichero + "; proceso linea repetido = " + fila + "; Id = " + Ide_TicketsOxe + "\n Con fichero: " + taxa_linea.Taxa + "; linea: " + taxa_linea.Linea_Taxa);

                                    }

                                }
                            }

                            if(maximoCargue == 500)
                            {
                                if (MaximaFechaTickets != 0)
                                {
                                    EliminarFilasTaxaViejas(MaximaFechaTickets);
                                }

                                AgregarUsuarios();
                                LlenarTickets(Convert.ToDateTime(fecha_inicial_cargue), DateTime.Today);
                                LlenarCalls(Convert.ToDateTime(DateTime.Today.ToString("yyyy/MM/dd HH:mm:ss")));

                                MaximaFechaTickets = (from db in bd_Xynthesis.xy_ticketsoxe select db).Count();

                                maximoCargue = 0;
                            }

                        }
                        catch (Exception ex)
                        {
                            Log.EscribaLog("Error_ExtracInfoTaxa_Fila", "Fichero: " + nombreFichero + "; proceso línea = " + fila + "; Mensaje: " + ex.Message);
                        }

                        registroSinEspacios = null;
                    }



                    reader.Close();

                    File.Move(rutaArchivoTaxa, rutaTaxaEjecutado + "/" + nombreFichero); //Mueve archivo a ruta especificada
                    //File.Move("D:\\SwDeviceIpc\\Archivos_Lectura\\"+ nombreFichero, "D:\\SwDeviceIpc\\Archivos_Lectura\\Ok_"+ nombreFichero); //Cambia de nombre a archivo especificado
                    maximoCargue = 0;
                    //foreach (string sOutput in arrText) Console.WriteLine(sOutput);
                    //Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ExtracInfoTaxa_Error", "El fichero error es: " + nombreFichero + " proceso linea " + fila.ToString() + "\n" + ex.Message, "Administrador");
                throw ex;
            }

            return MaximaFechaTickets;
        }



        private int IdentificaciónCobertura(string InitialDialledNumber)
        {
            //var resultString = string.Empty;
            int cobertura = 0;

            for (int i = 0; i < InitialDialledNumber.Length; i++) // Validar letras dentro de la cadena
            {
                if (!Char.IsDigit(InitialDialledNumber[i]))
                {
                    //resultString += InitialDialledNumber[i]; // Guarda los numeros en una cadena con letras y números
                    cobertura = 1;
                    return cobertura;
                }
            }


            int verificarNacional = Convert.ToInt32(InitialDialledNumber.Trim().Substring(0, 1));
            int verificarInterNacional = Convert.ToInt32(InitialDialledNumber.Trim().Substring(0, 2));
            decimal InitialDialledNumber_ = Convert.ToDecimal(InitialDialledNumber);

            if ((verificarNacional == 3 && InitialDialledNumber.LongCount() == 10) || verificarInterNacional == 66 && InitialDialledNumber.Length == 12)
            {
                cobertura = 5;  //Llamada a Celular
            }
            else if (verificarNacional == 0 && InitialDialledNumber_ >= 10000000 && InitialDialledNumber_ <= 99999999)
            {
                cobertura = 6;
            }
            else if ((verificarInterNacional == 00 && InitialDialledNumber.Length >= 11))
            {
                cobertura = 3;
            }
            else if (InitialDialledNumber_ >= 1000 && InitialDialledNumber_ <= 9999)
            {
                cobertura = 4;
            }
            else if (InitialDialledNumber_ >= 1000000 && InitialDialledNumber_ <= 9999999 || verificarInterNacional == 66 && InitialDialledNumber.Length == 9)
            {
                cobertura = 2;
            }
            else
            {
                cobertura = 1;
            }


            return cobertura;
        }


        private List<string> ValidarUserOxe(IList RegistroTaxa)
        {
            List<string> UserOxe = new List<string>();

            //Comienzo extensión -----------------------------------
            if (RegistroTaxa[2].ToString() != "")
            {
                UserOxe.Add(RegistroTaxa[2].ToString());
            }
            else if (RegistroTaxa[3].ToString() != "")
            {
                string ChargedCostCenter = RegistroTaxa[3].ToString();
                var ValidarChargedNumber = (from db in bd_Xynthesis.xy_costcenters where db.Nom_CostCenter == ChargedCostCenter select db).Any();

                if (ValidarChargedNumber)
                {
                    var ChargedNumber = (from db in bd_Xynthesis.xy_costcenters where db.Nom_CostCenter == ChargedCostCenter select db).First();

                    UserOxe.Add(ChargedNumber.Ide_ParentCostCenter);

                }

                else
                {
                    UserOxe.Add("0");
                }

            }
            else
            {
                UserOxe.Add("0");
            }
            //Fin extensión -----------------------------------

            //Comienzo Nombre Usuario -----------------------------------
            if (RegistroTaxa[3].ToString() != "")
            {
                UserOxe.Add(RegistroTaxa[3].ToString());
            }
            else if (RegistroTaxa[2].ToString() != "")
            {

                string ChargedNumber = RegistroTaxa[2].ToString();
                var ValidarUserName = (from db in bd_Xynthesis.xy_costcenters where db.Ide_ParentCostCenter == ChargedNumber select db).Any();

                if (ValidarUserName)
                {
                    var UserName = (from db in bd_Xynthesis.xy_costcenters where db.Ide_ParentCostCenter == ChargedNumber select db).First();

                    UserOxe.Add(UserName.Nom_CostCenter);
                }
                else
                {
                    UserOxe.Add("SIN NOMBRE");
                }

            }
            else
            {
                UserOxe.Add("SIN NOMBRE");
            }
            //Fin Nombre Usuario -----------------------------------

            //Comienzo Centro de costes -----------------------------------
            if (RegistroTaxa[4].ToString() != "")
            {
                UserOxe.Add(RegistroTaxa[4].ToString());
            }
            else if (RegistroTaxa[2].ToString() != "")
            {

                string ChargedNumber = RegistroTaxa[2].ToString();
                var ValidarCostCenter = (from db in bd_Xynthesis.xy_costcenters where db.Ide_ParentCostCenter == ChargedNumber select db).Any();


                if (ValidarCostCenter)
                {
                    var CostCenter = (from db in bd_Xynthesis.xy_costcenters where db.Ide_ParentCostCenter == ChargedNumber select db).First();
                    UserOxe.Add(CostCenter.Cod_CostCenter);
                }
                else
                {
                    UserOxe.Add("2");
                }


            }
            else if (RegistroTaxa[3].ToString() != "")
            {

                string ChargedCostCenter = RegistroTaxa[3].ToString();
                var ValidarCostCenter = (from db in bd_Xynthesis.xy_costcenters where db.Nom_CostCenter == ChargedCostCenter select db).Any();

                if (ValidarCostCenter)
                {
                    var CostCenter = (from db in bd_Xynthesis.xy_costcenters where db.Nom_CostCenter == ChargedCostCenter select db).First();

                    UserOxe.Add(CostCenter.Cod_CostCenter);
                }
                else
                {
                    UserOxe.Add("2");
                }


            }
            else
            {
                UserOxe.Add("2");
            }
            //Fin Centro de costes -----------------------------------

            return UserOxe;
        }



        public void EliminarFilasTaxaViejas(int MaximaContadorTickets)
        {
            try
            {

                bd_Xynthesis.xyp_DelFilasTicketsOld(MaximaContadorTickets);
            }
            catch (Exception ex)
            {
                Log.EscribaLog("EliminarFilasTaxaViejas_Error", "Error en metodo EliminarFilasTaxaViejas : " + ex.Message, "Administrador");
                throw ex;
            }
        }


        public void LlenarTicketinfo()
        {
            try
            {
                bd_Xynthesis.xyp_ExtractionTicketsIPC();

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo LlenarTicketinfo : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void AgregarUsuarios()
        {
            try
            {
                bd_Xynthesis.xyp_Execute_ETL_OXE();
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo AgregarUsuarios : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void LlenarTickets(DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {

                var a = bd_Xynthesis.xyp_TransTicketsOXE(fechaInicial, fechaFinal);

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo LlenarTickets : " + ex.Message, "Administrador");
                throw ex;
            }
        }

        public void LlenarCalls(DateTime fechaCargue)
        {
            try
            {

                bd_Xynthesis.xyp_LoadTickets(fechaCargue);

            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Error en metodo LlenarCalls : " + ex.Message, "Administrador");
                throw ex;
            }
        }

    }
}
