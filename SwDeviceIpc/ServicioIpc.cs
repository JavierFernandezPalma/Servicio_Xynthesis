using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Axede.Xynthesis.Log;
using CommonProcessLibrary;
using Axede.Xynthesis.ConnectToDBIpc;
using System.Configuration;
using Axede.Xynthesis.IpcProcess;


namespace SwDeviceIpc
{
    public partial class ServicioIpc : ServiceBase
    {
        private System.Timers.Timer m_mainTimer; 
        private bool m_timerTaskSuccess;
        private LogError lg = new LogError();
        public DateTime fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00");
        string periodicidad = ConfigurationManager.AppSettings["Periodicidad"].ToString();
        string horaTrabajo = DateTime.Now.ToString("yyyy/MM/dd ") + ConfigurationManager.AppSettings["horaTrabajo"].ToString();
        string ruta_ipc_csv = ConfigurationManager.AppSettings["ruta_ipc_csv"].ToString();
        string horas = ConfigurationManager.AppSettings["horaTrabajo"].ToString();
        string OpcionCargue = ConfigurationManager.AppSettings["OpcionCargue"].ToString();
        string origenCargue = ConfigurationManager.AppSettings["origenCargue"].ToString();
        public ServicioIpc()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas);
                m_mainTimer = new System.Timers.Timer();
                if (periodicidad == "D")   //intervalo dias
                    m_mainTimer.Interval = 5000;  //5 Segundos
                else
                    m_mainTimer.Interval = ConvertiraMiliSegundos(Convert.ToInt32(horas.Substring(0, 2)), Convert.ToInt32(horas.Substring(3, 2)), Convert.ToInt32(horas.Substring(6, 2)));    // intervalo HORAS

                m_mainTimer.Elapsed += MainTimer_Elapsed;
                m_mainTimer.AutoReset = false;  // makes it fire only once
                m_mainTimer.Start(); // Start

                m_timerTaskSuccess = false;
            }
            catch (Exception ex)
            {
                lg.EscribaLog("ServicioIpc", "OnStart " + ex.Message, "Administrador");
                // omitted

            }
        }

        void MainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (origenCargue.ToUpper().Equals("A"))// A= archivo plano; BW=desde Blue Wave
                    ProcesoIpc();
                else
                   // procesoIpcBueWave();

                m_timerTaskSuccess = true;
            }
            catch (Exception ex)
            {
                if (periodicidad == "D")
                    fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddDays(1);
                else
                    fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddMilliseconds(m_mainTimer.Interval);
                m_timerTaskSuccess = false;
                lg.EscribaLog("ServicioIpc", "Error en metodo m_mainTimer_Elapsed " + ex.Message, "Administrador");
            }
            finally
            {
                if (m_timerTaskSuccess)
                {
                    m_mainTimer.Start();
                }
            }
        }

        private void ProcesoIpcBlueWave()
        {
            ClsBlueWave Bw = new ClsBlueWave();
            Bw.iniciaSession();
        }


        private void ProcesoIpc()
        {

            IpcProcess moMultiple = new IpcProcess();
            
            try
            {
                DateTime dt = DateTime.Now;
                string HrActual = dt.ToString("yyyy/MM/dd  HH:mm:ss"); 
                
                if (Convert.ToDateTime(HrActual) >= Convert.ToDateTime(horaTrabajo) || periodicidad == "H")
                {
                    if (fechaEje <= DateTime.Now )
                    {
                        if (periodicidad == "D")
                            fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddDays(1);
                        else
                            fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddMilliseconds(m_mainTimer.Interval);
                        m_mainTimer.Stop();
                        lg.EscribaLog("ServicioIpc", "ServicioIpc; iniciando el proceso", "Administrador");
                        moMultiple.ExecuteETL(OpcionCargue);
                        //lg.EscribaLog("ServicioIpc", "ServicioIpc; termino el proceso con exito", "Administrador");
                        m_mainTimer.Enabled = true;
                        m_mainTimer.Start();
                    }
                }


            }
            catch (Exception ex)
            {
                //fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00");
                if (periodicidad == "D")
                    fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddDays(1);
                else
                    fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas).AddMilliseconds(m_mainTimer.Interval);
                m_mainTimer.Start();
                lg.EscribaLog("ServicioIpc", "Ocurrrio un error en el proceso metodo procesoIpc " + ex.Message, "Administrador");
                throw ex;
            }

        }

        protected override void OnStop()
        {
            try
            {
                fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00");
                // Service stopped. Also stop the timer.
                m_mainTimer.Stop();
                m_mainTimer.Dispose();
                m_mainTimer = null;
            }
            catch (Exception ex)
            {
                lg.EscribaLog("ServicioIpc", "OnStop " + ex.Message, "Administrador");
                // omitted
            }


        }


        protected int ConvertiraMiliSegundos(int h, int m, int s)
        {
            return (h * 3600 + m * 60 + s) * 1000;
        }


        public void WindowsTest()
        {
            //try
            //{
            //    fechaEje = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " " + horas);
            //    m_mainTimer = new System.Timers.Timer();
            //    if (periodicidad == "D")   //intervalo dias
            //        m_mainTimer.Interval = 5000;  //5 Segundos
            //    else
            //        m_mainTimer.Interval = ConvertiraMiliSegundos(Convert.ToInt32(horas.Substring(0, 2)), Convert.ToInt32(horas.Substring(3, 2)), Convert.ToInt32(horas.Substring(6, 2)));    // intervalo HORAS

            //    m_mainTimer.Elapsed += MainTimer_Elapsed;
            //    m_mainTimer.AutoReset = false;  // makes it fire only once
            //    m_mainTimer.Start(); // Start

            //    m_timerTaskSuccess = false;
            //}
            //catch (Exception ex)
            //{
            //    lg.EscribaLog("ServicioIpc", "OnStart " + ex.Message, "Administrador");
            //    // omitted

            //}
        }

    }
}
