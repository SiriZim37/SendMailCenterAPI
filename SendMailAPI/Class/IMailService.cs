using SendMailAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendMailAPI.Class
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
