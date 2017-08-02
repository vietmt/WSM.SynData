using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Newtonsoft.Json;

namespace WSM.SynData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<WorkSpace> lstSpace = new List<WorkSpace>();
        public System.Timers.Timer timer;
        public int iLoopMin = 60;
        public List<OffTime> lsTime = new List<OffTime>();
        
        public MainWindow()
        {
            InitializeComponent();
            //var json = SynData.Properties.Settings.Default.workspaces;
            var json2 = SynData.Properties.Settings.Default.timeoff;
            //lstSpace = JsonConvert.DeserializeObject<List<WorkSpace>>(json);
            lsTime = JsonConvert.DeserializeObject<List<OffTime>>(json2);
            //lstSpace.Add(new WorkSpace(Location.Laboratory, "192.168.1.8", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.200", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.201", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.206", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.50", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.51", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.52", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.252", 4370, MachineType.TFT));
            //lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.253", 4370, MachineType.TFT));
            lstSpace.Add(new WorkSpace(Location.HCM, "172.17.3.245", 4370, MachineType.BlackNWhite));
            //lsTime.Add(new OffTime(new TimeSpan(7, 30, 0), new TimeSpan(8, 30, 0)));
            //lsTime.Add(new OffTime(new TimeSpan(16, 30, 0), new TimeSpan(18, 0, 0)));
            //lsTime.Add(new OffTime(new TimeSpan(13, 30, 0), new TimeSpan(15, 0, 0)));
            //var json = JsonConvert.SerializeObject(lstSpace);
            //var json2 = JsonConvert.SerializeObject(lsTime);
            iLoopMin = SynData.Properties.Settings.Default.timeloop;
            timer = new System.Timers.Timer(iLoopMin * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var time in lsTime)
            {
                if (time.CheckTime(DateTime.Now.TimeOfDay))
                    return;
            }
            foreach (var item in lstSpace)
            {
                try
                {
                    Thread run = new Thread(item.Run);
                    run.Start();
                    Thread.Sleep(10000);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
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

        private void btnSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? dtFrom = dpFrom.SelectedDate;
                DateTime? dtTo = dpTo.SelectedDate;

                foreach (var item in lstSpace)
                {
                    item.SynManual((DateTime)dtFrom, (DateTime)dtTo);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
