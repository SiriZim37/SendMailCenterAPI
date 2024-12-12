using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendMailAPI.Class.Entity
{
    public class ResponseModelClass
    {
        public class ApiResponse
        {
            public ApiResponse()
            {
                WSMsg = new WSmsgDetail();
                WSStatus = "Y"; 
            }
            public string WSStatus { get; set; }
            public WSmsgDetail WSMsg { get; set; }
        }



        public class WSmsgDetail
        {
            public WSmsgDetail()
            {
                Rslc = "";
                Step = "";
                Msgdesc_th = "";
                Msgdesc_en = "";
            }
            public string Msgstatus { get; set; }
            public object Result { get; set; }
            public string Rslc { get; set; }
            public string Step { get; set; }
            public string Msgdesc_th { get; set; }
            public string Msgdesc_en { get; set; }
        }

        public class AuthenData
        {
            public string Status { get; set; }
        }
     
    }
}

