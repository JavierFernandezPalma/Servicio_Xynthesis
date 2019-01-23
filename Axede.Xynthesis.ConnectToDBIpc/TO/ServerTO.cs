using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axede.Xynthesis.ConnectToDBIpc.TO
{
    public class ServerTO
    {
        private string sServer;

        public string Server
        {
            get { return sServer; }
            set { sServer = value; }
        }
        private string sNombServer;

        public string NombServer
        {
            get { return sNombServer; }
            set { sNombServer = value; }
        }

        private string sUser;

        public string User
        {
            get { return sUser; }
            set { sUser = value; }
        }

        private string sPassword;

        public string Password
        {
            get { return sPassword; }
            set { sPassword = value; }
        }
        private string sFormato;

        public string Formato
        {
            get { return sFormato; }
            set { sFormato = value; }
        }
        private string sPlugin;

        public string Plugin
        {
            get { return sPlugin; }
            set { sPlugin = value; }
        }


    }
}
