using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WSM.SynData
{
    public class MailClient
    {
        #region Property
        public string mailserver;
        public string username;
        public string password;
        public int serverport;
        public bool issshserver;
        public string from;
        public string to;
        public string title;
        public string content;
        #endregion
        #region Methods
        public MailClient(string strMailServer, int iServerPort, bool isSSHServer, string strUserName, string strPassword)
        {
            mailserver = strMailServer;
            serverport = iServerPort;
            issshserver = isSSHServer;
            username = strUserName;
            password = strPassword;
        }
        public bool SendMail(string strFrom, string strTo, string strTitle, string strContent)
        {
            try
            {
                from = strFrom;
                to = strTo;
                title = strTitle;
                content = strContent;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(mailserver);

                mail.From = new MailAddress(from);
                mail.To.Add(to);
                mail.Subject = title;
                mail.Body = content;

                SmtpServer.Port = serverport;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = issshserver;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }
        #endregion
    }
}
