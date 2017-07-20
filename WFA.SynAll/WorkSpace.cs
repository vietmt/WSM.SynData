using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WFA.SynAll
{
    public enum Location
    {
        Toong = 1
        , Laboratory = 2
        , HCM = 3
        , Danang = 4
        , Keangnam = 5
    }
    public class WorkSpace
    {
        #region Properties
        public Location local;
        public string attMachineIp;
        public int attMachinePort;
        public int attMachineType;
        public List<Attendance> lstAtt;
        public zkemkeeper.CZKEMClass connecter;
        public string ErrorMess;
        public int PushCount;
        private string uri = @"http://edev.framgia.vn/api/enroll_checks";
        #endregion
        #region Methods
        public WorkSpace(Location lcLocal, string strIP, int iPort)
        {
            local = lcLocal;
            attMachineIp = strIP;
            attMachinePort = iPort;
            connecter = new zkemkeeper.CZKEMClass();
            lstAtt = new List<Attendance>();
        }
        public bool GetData()
        {
            try
            {
                if (connecter.Connect_Net(attMachineIp, attMachinePort))
                    //iMachineNumber = 1;
                    connecter.RegEvent(1, 65535);
                else
                {
                    ErrorMess = "Can't connect";
                    return false;
                }
                connecter.EnableDevice(1, false);
                int idwTMachineNumber = 0;
                int idwEnrollNumber = 0;
                int idwEMachineNumber = 0;
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                DateTime checkdate = (lstAtt.Count == 0 ? DateTime.Now : lstAtt.Max(x => x.date));
                if (connecter.ReadGeneralLogData(1))
                {
                    while (connecter.GetGeneralLogData(1, ref idwTMachineNumber, ref idwEnrollNumber,
                       ref idwEMachineNumber, ref idwVerifyMode, ref idwInOutMode, ref idwYear, ref idwMonth,
                       ref idwDay, ref idwHour, ref idwMinute))
                    {
                        DateTime itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, 0);
                        if (itemtime >= checkdate)
                        {
                            Attendance item = new Attendance(idwEnrollNumber, itemtime);
                            if (!lstAtt.Contains(item))
                                lstAtt.Add(item);
                        }
                    }
                }
                else
                {
                    ErrorMess = "Can't read data";
                    return false;
                }
                connecter.EnableDevice(1, true);
                connecter.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMess = ex.Message;
                return false;
            }
        }
        public bool SendDate()
        {
            try
            {
                PushCount = 0;
                string strJSON = string.Empty;
                foreach (var item in lstAtt.Where(x => x.pushed == false))
                {
                    PushCount++;
                    strJSON += "{\"EnrollNumber\": " + item.EnrollNumber.ToString()
                        + ", \"date\": \"" + item.date.ToString("yyyy-MM-dd HH:mm") + "\"}";
                }
                strJSON = "{\"workspace_id\": " + ((int)local).ToString() + ", \"data\": [ " + strJSON + " ] }";
                strJSON = "{\"attention_data\": [ " + strJSON + " ]}";
                StringContent stringContent = new StringContent(strJSON, UnicodeEncoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                ErrorMess = client.PostAsync(uri, stringContent).Result.StatusCode.ToString();
                return true;
            }
            catch (Exception ex)
            {

                ErrorMess = "Can't send data to host";
                return false;
            }
        }
        #endregion
    }
}
