using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace WFA.SynAll
{
    public partial class frmMain : Form
    {
        static Timer tmLoop;
        public static zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
        const int iMinuteLoop = 5;
        const int iPort = 4370;
        private bool bIsConnected = false;
        private int iMachineNumber = 1;
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

        }
    }
}
