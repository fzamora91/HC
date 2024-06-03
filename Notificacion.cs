using System.Net;
using System.Net.Mail;
using System.Text;

namespace HCMonitoreoAPis
{
    public class Notificacion
    {

        public string nombre { get; set; }
        public string correo { get; set; }
        public string contenido { get; set; }



        public void enviarCorreo()
        {
            string smtpServer = "smtp.gmail.com";
            string port = "587";
            const String CONFIGSET = "ConfigSet";
            string from = "financia-ec@financiaenlinea.com";
            string address = "flxzamora91@gmail.com";
            string pass = "PTEafZchY3YANg*/";

            using (SmtpClient SMTP = new SmtpClient(smtpServer, int.Parse(port)))
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                    MailAddress to = new MailAddress(address);
                    mail.To.Add(to);

                    mail.Priority = MailPriority.Normal;
                    mail.From = new MailAddress(from, "", Encoding.UTF8);



                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = true;

                    SMTP.EnableSsl = true;

                    if (smtpServer.Contains("gmail.com") && !smtpServer.Contains("relay"))
                        SMTP.TargetName = "STARTTLS/smtp.gmail.com";


                    SMTP.Credentials = new NetworkCredential(from, pass);

                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = true;


                    

                    mail.Subject = "Error Monitoreo";
                    mail.Body = "Estimado Usuario, Ha ocurrido un error en el endpoint de monitoreo";



                    try
                    {
                        SMTP.Send(mail);
                    }
                    catch (Exception ex)
                    { 


                    
                    }







                    }
            }
        }
    }
}
 