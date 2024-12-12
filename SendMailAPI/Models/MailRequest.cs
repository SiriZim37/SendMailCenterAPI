using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendMailAPI.Models
{
    public class MailRequest
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }

        public string CCEmail { get; set; }

        public string BCCEmail { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public List<Attachment> Attachments { get; set; }
        

        public class Attachment
        {
            public string File_name { get; set; }
            public string File_content_type { get; set; }
            public string File_data { get; set; }
        }


    }

    public class TokenJWT
    {
        public string Token { get; set; }
    }
}
