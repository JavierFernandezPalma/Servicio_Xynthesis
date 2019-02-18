using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ServicioXynthesis.Service1 serv = new ServicioXynthesis.Service1();
            serv.WindowsTest();

            //Axede.Xynthesis.Process.IpcProcess2 prueba = new Axede.Xynthesis.Process.IpcProcess2();

            //prueba.ExtracInfoTaxa();
            //prueba.ExtracInfoCsv();
            //prueba.LlenarTicketinfo();
            //prueba.AgregarUsuarios();
            //prueba.LlenarTickets();
            //prueba.LlenarCalls();

            //Axede.Xynthesis.IpcProcess.IpcProcess i = new Axede.Xynthesis.IpcProcess.IpcProcess();
            //i.Execute("NORMAL");
        }
    }
}
