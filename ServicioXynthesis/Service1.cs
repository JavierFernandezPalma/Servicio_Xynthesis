using ServicioXynthesis.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceProcess;
using Axede.Xynthesis.Process;
using System.Threading;

namespace ServicioXynthesis
{
    public partial class Service1 : ServiceBase
    {


        private LogServicioXynthesis Log = new LogServicioXynthesis();

        //string ruta_ipc_csv = ConfigurationManager.AppSettings["ruta_ipc_csv"].ToString();
        readonly string fecha_inicial_cargue = ConfigurationManager.AppSettings["fecha_inicial_cargue"].ToString();
        readonly string rutaArchplano = ConfigurationManager.AppSettings["ruta_taxa_data"].ToString();
        readonly string nombre_archivo = ConfigurationManager.AppSettings["nombre_archivo"].ToString();
        readonly int numero_archivos = Convert.ToInt32(ConfigurationManager.AppSettings["numero_archivos"]);
        Timer Schedular;
        IpcProcess2 moMultiple = new IpcProcess2();

        public Service1()
        {
            InitializeComponent();
        }

        private void SchedularCallback(object e)
        {
            this.VerificarFicheros();
            Log.EscribaLog("SchedularCallback()", "Ejecutando Servicio Xynthesis Oxe");
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //Log.EscribaLog("OnStart()", "Inicia el metodo OnStart()");
                //Schedular = new Timer();
                //Schedular.Interval = 60000 * Convert.ToDouble(ConfigurationManager.AppSettings["intervalo_tiempo"]);
                //Schedular.Elapsed += new ElapsedEventHandler(metodo_elapsed);
                //Schedular.Enabled = true;
                //Schedular.Start();
                //Log.EscribaLog("OnStart()", "Termino de ejecutarse el metodo OnStart()");

                Log.EscribaLog("OnStart()", "Inicia el metodo OnStart()");
                this.VerificarFicheros();
                EventLog.WriteEntry("Inicia el Servicio Xynthesis Oxe.");
            }
            catch (Exception ex)
            {
                Log.EscribaLog("OnStart()_Error", "El error es : " + ex.ToString());
            }

        }

        protected override void OnStop()
        {
            try
            {
                this.Schedular.Dispose();
                Log.EscribaLog("OnStop()", "Finalizo el Servicio Xynthesis Oxe.");
                EventLog.WriteEntry("Finalizo el Servicio Xynthesis Oxe.");
            }
            catch (Exception ex)
            {
                Log.EscribaLog("OnStop()_Error", "El error es : " + ex.ToString());
            }
        }



        //private void metodo_elapsed(object sender, ElapsedEventArgs e)
        //{

        //    try
        //    {
        //        Schedular.Enabled = false;
        //        Schedular.Stop();
        //        VerificarFicheros();
        //        Schedular.Enabled = true;
        //        Schedular.Start();
        //        EventLog.WriteEntry("Servicio AgendaCSJ completo el ciclo correctamente.");
        //        Log.EscribaLog("ServicioAgendaCSJ", "Servicio AgendaCSJ completo el ciclo correctamente.");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.EscribaLog("metodo_elapsed()_Error", "El error es : " + ex.ToString());
        //    }
        //}

        private void VerificarFicheros()
        {
            try
            {
                string directorio = rutaArchplano;
                string[] ficheros = Directory.GetFiles(directorio).Take(numero_archivos).ToArray();
                List<string> listFicherosTaxa = new List<string>();

                for (int i = 0; i < ficheros.Count(); i++)
                {
                    int longitudFichero = ficheros[i].Length;
                    int posicionTaxa = ficheros[i].IndexOf(nombre_archivo);
                    if (posicionTaxa != -1)
                    {
                        int diferenciaFicheroTaxa = longitudFichero - posicionTaxa;
                        string taxaValidar = ficheros[i].Substring(posicionTaxa, diferenciaFicheroTaxa);
                        listFicherosTaxa.Add(taxaValidar);
                    }

                }

                if (listFicherosTaxa.Count != 0)
                {
                    ProcesarFicheros(listFicherosTaxa);
                }
                else
                {
                    Schedular = new Timer(new TimerCallback(SchedularCallback));
                    Schedular.Change(Convert.ToInt64(1000 * Convert.ToDouble(ConfigurationManager.AppSettings["intervalo_tiempo"])), Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ValidarTaxa()_Error", ex.Message);
                //throw ex;
            }
        }

        private void ProcesarFicheros(List<string> Ficheros)
        {
            try
            {
                int MaximaContadorTickets = moMultiple.ExtracInfoTaxa(Ficheros);

                if (MaximaContadorTickets != 0)
                {
                    moMultiple.EliminarFilasTaxaViejas(MaximaContadorTickets);
                }

                moMultiple.AgregarUsuarios();
                moMultiple.LlenarTickets(Convert.ToDateTime(fecha_inicial_cargue), DateTime.Today);
                moMultiple.LlenarCalls(Convert.ToDateTime(DateTime.Today));

                Schedular = new Timer(new TimerCallback(SchedularCallback));
                Schedular.Change(Convert.ToInt64(1000 * Convert.ToDouble(ConfigurationManager.AppSettings["intervalo_tiempo"])), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.EscribaLog("ServicioIpc", "Ocurrrio un error en ProcesarFicheros " + ex.Message, "Administrador");
                throw ex;
            }

        }


        public void WindowsTest()
        {
            try
            {
                Log.EscribaLog("OnStart()", "Inicia el metodo OnStart()");
                this.VerificarFicheros();
                //EventLog.WriteEntry("Inicia el servicio Windows Reportes programado.");
            }
            catch (Exception ex)
            {
                Log.EscribaLog("OnStart()_Error", "El error es : " + ex.ToString());
            }
        }
    }
}
