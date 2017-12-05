using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace WSM.SynData
{
    public enum Location
    {
        Toong = 1
        , Laboratory = 2
        , HCM = 3
        , Danang = 4
        , Keangnam = 5
    }

    public enum MachineType
    {
        BlackNWhite = 1
        , IFace = 2
        , TFT = 3
    }
    public class WorkSpace
    {
        #region Properties
        public Location local;
        public string attMachineIp;
        public int attMachinePort;
        public MachineType attMachineType;
        public OffTime WorkingTime;
        public List<string> lstEnrollAdv = new List<string>();
        [JsonIgnore]
        public List<Attendance> lstAtt;
        [JsonIgnore]
        public zkemkeeper.CZKEMClass connecter;
        [JsonIgnore]
        public string ErrorMess;
        [JsonIgnore]
        public int PushCount;
        [JsonIgnore]
        public DateTime begin;
        [JsonIgnore]
        public DateTime end;
        [JsonIgnore]
        private string uri = SynData.Properties.Settings.Default.api;
        [JsonIgnore]
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region Methods
        public WorkSpace(Location lcLocal, string strIP, int iPort, MachineType mtype)
        {
            local = lcLocal;
            attMachineIp = strIP;
            attMachinePort = iPort;
            attMachineType = mtype;
            connecter = new zkemkeeper.CZKEMClass();
            lstAtt = new List<Attendance>();
        }
        public bool GetData()
        {
            try
            {
                if (connecter.Connect_Net(attMachineIp, attMachinePort))
                    connecter.RegEvent(1, 65535);
                else
                {
                    int iError = 0;
                    connecter.GetLastError(ref iError);
                    ErrorMess = iError.ToString() + " : Can't connect";
                    log.Error(DateTime.Now.ToShortTimeString() + " | " + attMachineIp + " | " +"Connect error" + " | "
                        + ErrorMess + " | ");
                    return false;
                }
                connecter.EnableDevice(1, false);
                begin = DateTime.Now;
                string sdwEnrollNumber = "";
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
                int idwSecond = 0;
                int idwWorkcode = 0;
                DateTime checkdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day); //(lstAtt.Count == 0 ? DateTime.Now : lstAtt.Max(x => x.date));
                if (lstAtt.Count != 0)
                {
                    if (lstAtt.Max(x => x.date) < checkdate)
                    {
                        lstAtt.Clear();
                    }
                    else
                        checkdate = lstAtt.Max(x => x.date);
                }
                if (connecter.ReadGeneralLogData(1))
                {
                    if (attMachineType == MachineType.BlackNWhite)
                    {
                        while (connecter.GetGeneralLogData(1, ref idwTMachineNumber, ref idwEnrollNumber,
                       ref idwEMachineNumber, ref idwVerifyMode, ref idwInOutMode, ref idwYear, ref idwMonth,
                       ref idwDay, ref idwHour, ref idwMinute))
                        {
                            DateTime itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, 0);
                            if (itemtime >= checkdate)
                            {
                                Attendance item1 = new Attendance(idwEnrollNumber.ToString(), itemtime, true);
                                Attendance item2 = new Attendance(idwEnrollNumber.ToString(), itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                    if (attMachineType == MachineType.TFT)
                    {
                        while (connecter.SSR_GetGeneralLogData(1, out sdwEnrollNumber, out idwVerifyMode,
                           out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))
                        {
                            DateTime itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            if (itemtime >= checkdate)
                            {
                                Attendance item1 = new Attendance(sdwEnrollNumber, itemtime, true);
                                Attendance item2 = new Attendance(sdwEnrollNumber, itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                }
                else
                {
                    ErrorMess = "Can't read data";
                    return false;
                }
                connecter.EnableDevice(1, true);
                end = DateTime.Now;
                connecter.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMess = ex.Message;
                return false;
            }
        }
        public bool ValidateData()
        {
            try
            {
                foreach (var enr in lstEnrollAdv)
                {
                    var enrBegin = lstAtt.Where(x => x.EnrollNumber == enr).Min(x => x.date);
                    var enrEnd = lstAtt.Where(x => x.EnrollNumber == enr).Max(x => x.date);
                    if (enrBegin.TimeOfDay > WorkingTime.tsBegin)
                    {
                        DateTime itemtime = new DateTime(enrBegin.Year, enrBegin.Month, enrBegin.Day, 0, 0, 0) + WorkingTime.tsBegin;
                        Attendance item1 = new Attendance(enr, itemtime, false);
                        var lastAtt = lstAtt.Where(x => x.date.TimeOfDay > WorkingTime.tsBegin).OrderBy(x=>x.date).First();
                        lstAtt.Insert(lstAtt.IndexOf(lastAtt), item1);
                    }
                    if (enrEnd.TimeOfDay < WorkingTime.tsEnd)
                    {
                        if (enrEnd.DayOfYear < DateTime.Now.DayOfYear || (enrEnd.DayOfYear == DateTime.Now.DayOfYear && DateTime.Now.TimeOfDay > WorkingTime.tsEnd))
                        {
                            DateTime itemtime = new DateTime(enrBegin.Year, enrBegin.Month, enrBegin.Day, 0, 0, 0) + WorkingTime.tsEnd;
                            Attendance item1 = new Attendance(enr, itemtime, false);
                            var lastAtt = lstAtt.Where(x => x.date.TimeOfDay > WorkingTime.tsEnd).OrderBy(x => x.date).First();
                            lstAtt.Insert(lstAtt.IndexOf(lastAtt), item1);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }
        public bool SendData()
        {
            try
            {
                PushCount = 0;
                string strJSON = string.Empty;
                foreach (var item in lstAtt.Where(x => x.pushed == false))
                {
                    if (PushCount != 0)
                    {
                        strJSON += ",";
                    }
                    strJSON += "{\"EnrollNumber\": " + item.EnrollNumber
                        + ", \"date\": \"" + item.date.ToString("yyyy-MM-dd HH:mm") + "\"}";
                    PushCount++;
                }
                strJSON = "{\"workspace_id\": " + ((int)local).ToString() + ", \"data\": [ " + strJSON + " ] }";
                strJSON = "{\"attention_data\": [ " + strJSON + " ]}";
                StringContent stringContent = new StringContent(strJSON, UnicodeEncoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                var response = client.PostAsync(uri, stringContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    //log.Info(strJSON);
                    ErrorMess = response.StatusCode.ToString();
                    foreach (var item in lstAtt.Where(x => x.pushed == false))
                        item.pushed = true;
                    return true;
                }
                else
                {
                    ErrorMess = response.StatusCode.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {

                ErrorMess = ex.Message;
                return false;
            }
        }
        public void Run()
        {
            try
            {
                GetData();
                ValidateData();
                SendData();
                log.Info(DateTime.Now.ToShortTimeString() + " | " + attMachineIp + " | "
                        + ErrorMess + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
            }
            catch (Exception ex)
            {

                ErrorMess = ex.Message;
                return;
            }
        }

        public bool SynByDate(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                if (connecter.Connect_Net(attMachineIp, attMachinePort))
                    connecter.RegEvent(1, 65535);
                else
                {
                    int iError = 0;
                    connecter.GetLastError(ref iError);
                    ErrorMess = iError.ToString() + " : Can't connect";
                    log.Error(DateTime.Now.ToShortTimeString() + " | " + attMachineIp + " | "
                        + ErrorMess + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                    return false;
                }
                connecter.EnableDevice(1, false);
                begin = DateTime.Now;
                string sdwEnrollNumber = "";
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
                int idwSecond = 0;
                int idwWorkcode = 0;
                lstAtt.Clear();
                if (connecter.ReadGeneralLogData(1))
                {
                    if (attMachineType == MachineType.BlackNWhite)
                    {
                        while (connecter.GetGeneralLogData(1, ref idwTMachineNumber, ref idwEnrollNumber,
                       ref idwEMachineNumber, ref idwVerifyMode, ref idwInOutMode, ref idwYear, ref idwMonth,
                       ref idwDay, ref idwHour, ref idwMinute))
                        {
                            DateTime itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, 0);
                            if (itemtime.DayOfYear >= dtFrom.DayOfYear && itemtime.DayOfYear <= dtTo.DayOfYear)
                            {
                                Attendance item1 = new Attendance(idwEnrollNumber.ToString(), itemtime, true);
                                Attendance item2 = new Attendance(idwEnrollNumber.ToString(), itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                    if (attMachineType == MachineType.TFT)
                    {
                        while (connecter.SSR_GetGeneralLogData(1, out sdwEnrollNumber, out idwVerifyMode,
                           out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))
                        {
                            DateTime itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            if (itemtime.DayOfYear >= dtFrom.DayOfYear && itemtime.DayOfYear <= dtTo.DayOfYear)
                            {
                                Attendance item1 = new Attendance(sdwEnrollNumber, itemtime, true);
                                Attendance item2 = new Attendance(sdwEnrollNumber, itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                }
                else
                {
                    connecter.EnableDevice(1, true);
                    end = DateTime.Now;
                    connecter.Disconnect();
                    ErrorMess = "Can't read data";
                    return false;
                }
                connecter.EnableDevice(1, true);
                end = DateTime.Now;
                connecter.Disconnect();
                log.Info(DateTime.Now.ToShortTimeString() + " | " + attMachineIp + " | "
                        + ErrorMess + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                return true;
            }
            catch (Exception ex)
            {
                ErrorMess = ex.Message;
                return false;
            }
        }

        public bool SynManual(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                if (SynByDate(dtFrom, dtTo))
                {
                    ValidateData();
                    SendData();
                    lstAtt.Clear();
                }
                return true;
            }
            catch (Exception ex)
            {

                ErrorMess = ex.Message;
                return false;
            }
        }
        #endregion
    }
}
