using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WSM.SynData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<WorkSpace> lstSpace = new List<WorkSpace>();
        public Timer timer;
        const int iLoopMin = 20;
        public MainWindow()
        {
            InitializeComponent();
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.200", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.201", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.206", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.50", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.51", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.52", 4370, MachineType.BlackNWhite));
            lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.252", 4370, MachineType.TFT));
            lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.253", 4370, MachineType.TFT));
            lstSpace.Add(new WorkSpace(Location.HCM, "172.17.3.245", 4370, MachineType.BlackNWhite));
            timer = new Timer(iLoopMin * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (var item in lstSpace)
                {
                    if (item.GetData())
                        item.SendData();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        txtLogs.Text += DateTime.Now.ToShortTimeString() + " | " + item.attMachineIp + " | "
                        + item.ErrorMess + " | " + item.PushCount.ToString() + " | " + item.begin.ToLongTimeString() +" | " 
                        + item.end.ToLongTimeString() + " | " + (item.end-item.begin).TotalSeconds.ToString() + "\r\n";
                    }));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtLogs.Text += ex.Message + "\r\n";
                }));
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (timer.Enabled == false)
            {
                Timer_Elapsed(null, null);
                timer.Enabled = true;
                btnStart.Content = "Stop";
            }
            else
            {
                timer.Enabled = false;
                btnStart.Content = "Start";
            }
        }
    }
}
