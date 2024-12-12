
using Microsoft.Extensions.Configuration;
using SendMailAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace SendMailAPI.Class
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            try
            {

                //------------------ for .Net SMTP --------------------------------
                using var smtp = new SmtpClient();
                smtp.Host = _mailSettings.Host;

                smtp.EnableSsl = Convert.ToBoolean(true);
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = _mailSettings.UserName;
                NetworkCred.Password = _mailSettings.Password;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = _mailSettings.Port;

                MailMessage message = new MailMessage(mailRequest.FromEmail, mailRequest.ToEmail);
                if (!string.IsNullOrEmpty(mailRequest.CCEmail))
                {
                    message.CC.Add(mailRequest.CCEmail);
                }
                if (!string.IsNullOrEmpty(mailRequest.BCCEmail))
                {
                    message.Bcc.Add(mailRequest.BCCEmail);
                }

                message.Subject = mailRequest.Subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Body = mailRequest.Body;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                //for attach file with base64
                if (mailRequest.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.File_data.Length > 0)
                        {
                            fileBytes = Convert.FromBase64String(file.File_data);

                            Attachment att = new Attachment(new MemoryStream(fileBytes), file.File_name);
                            message.Attachments.Add(att);

                        }
                    }
                }
                smtp.Send(message);
                message.Dispose();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
