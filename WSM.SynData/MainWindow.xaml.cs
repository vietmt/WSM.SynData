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
        public System.Timers.Timer timerstart;
        public int iLoopHour = 1;
        public List<OffTime> lsTime = new List<OffTime>();

        public MainWindow()
        {
            InitializeComponent();
            var json2 = SynData.Properties.Settings.Default.timeoff;
            lsTime = JsonConvert.DeserializeObject<List<OffTime>>(json2);
            var json = SynData.Properties.Settings.Default.workspaces;
            lstSpace = JsonConvert.DeserializeObject<List<WorkSpace>>(json);
            //lstSpace.Add(new WorkSpace(Location.Laboratory, "192.168.1.8", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.200", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.201", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.1.206", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.50", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.51", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Keangnam, "192.168.0.52", 4370, MachineType.BlackNWhite));
            //lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.252", 4370, MachineType.TFT));
            //lstSpace.Add(new WorkSpace(Location.Danang, "172.16.7.253", 4370, MachineType.TFT));
            //lstSpace.Add(new WorkSpace(Location.HCM, "172.17.3.245", 4370, MachineType.BlackNWhite));
            //lsTime.Add(new OffTime(new TimeSpan(7, 30, 0), new TimeSpan(8, 30, 0)));
            //lsTime.Add(new OffTime(new TimeSpan(16, 30, 0), new TimeSpan(18, 0, 0)));
            //lsTime.Add(new OffTime(new TimeSpan(13, 30, 0), new TimeSpan(15, 0, 0)));
            //var json = JsonConvert.SerializeObject(lstSpace);
            //var json2 = JsonConvert.SerializeObject(lsTime);
            iLoopHour = SynData.Properties.Settings.Default.timeloop;
            timer = new System.Timers.Timer(iLoopHour * 60 * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            List<string> lstCombo = new List<string>();
            foreach (var item in lstSpace)
            {
                lstCombo.Add(item.attMachineIp);
            }

            cbWorkSpace.ItemsSource = lstCombo;
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
                var currenttime = DateTime.Now;
                var addhour = iLoopHour- ((currenttime.Hour) % iLoopHour);
                TimeSpan nexttime = new TimeSpan(currenttime.Hour + addhour, 0, 0);
                timerstart = new System.Timers.Timer((int)(nexttime - currenttime.TimeOfDay).TotalMilliseconds - 10 * 60 * 1000);
                timerstart.Elapsed += Timerstart_Elapsed;
                timerstart.Enabled = true;
                btnStart.Content = "Stop";
                //Thread.Sleep((int)(nexttime - currenttime.TimeOfDay).TotalMilliseconds-10*60*1000);
                //timer.Enabled = true;
                //Timer_Elapsed(null, null);
                //btnStart.Content = "Stop";
            }
            else
            {
                timer.Enabled = false;
                btnStart.Content = "Start";
            }
        }

        private void Timerstart_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(timer.Enabled==false)
            {
                timer.Enabled = true;
                timerstart.Enabled = false;
                Timer_Elapsed(null, null);
            }
        }

        private void btnSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? dtFrom = dpFrom.SelectedDate;
                DateTime? dtTo = dpTo.SelectedDate;
                string ip = (string)cbWorkSpace.SelectedItem;
                var space = lstSpace.Where(x => x.attMachineIp == ip).First();
                space.SynManual((DateTime)dtFrom, (DateTime)dtTo);
                //foreach (var item in lstSpace)
                //{
                //    item.SynManual((DateTime)dtFrom, (DateTime)dtTo);
                //}
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
