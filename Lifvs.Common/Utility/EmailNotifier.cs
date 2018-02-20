using Lifvs.Common.Utility.Interfaces;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;

namespace Lifvs.Common.Utility
{
    public class EmailNotifier : IEmailNotifier
    {
        public void SendEmail(string to, string body, string subject)
        {
            var smtpPort = ConfigurationManager.AppSettings["SmtpPort"];
            var smtpClient = ConfigurationManager.AppSettings["SmtpClient"];
            var userName = ConfigurationManager.AppSettings["UserName"];
            var passWord = ConfigurationManager.AppSettings["Password"];
            MailMessage mail = new MailMessage();

            SmtpClient smtpServer = new SmtpClient(Convert.ToString(smtpClient));
            smtpServer.Credentials = new System.Net.NetworkCredential(Convert.ToString(userName), Convert.ToString(passWord));
            smtpServer.Port = Convert.ToInt32(smtpPort); // Gmail works on this port
            smtpServer.EnableSsl = true;

            mail.From = new MailAddress(Convert.ToString(userName), "Lifvs");
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            var mimeType = new ContentType("text/html");
            var alternate = AlternateView.CreateAlternateViewFromString(body, mimeType);
            mail.AlternateViews.Add(alternate);
            smtpServer.Send(mail);
        }
    }
}
