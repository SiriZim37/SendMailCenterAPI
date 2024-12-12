using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SendMailAPI.Models;
using SendMailAPI.Class;
using MimeKit;
using System.Net.Http.Headers;
using static SendMailAPI.Class.Entity.ResponseModelClass;
using System.Text.Json;
using System.Text;

namespace SendMailAPI.Controllers
{
    [Route("api/[Controller]/[action]")]
    [ApiController]
    public class MailController : Controller
    {
        private readonly Class.IMailService mailService;
        public MailController(IMailService mailService)
        {
            this.mailService = mailService;
        }
        [HttpPost]
        [ActionName("SendMail")]
        public async Task<IActionResult> SendMail([FromBody]MailRequest request)
        {
            MainClass objMain = new MainClass();
            ApiResponse resp = new ApiResponse();
            bool isError = false;
            string resData = string.Empty;
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //Block for Authen
            try
            {
                bool isAuthen = false;
                string token = string.Empty;
                
                if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    isAuthen = false;
                else
                    //authen PKS
                    isAuthen = await objMain.AuthenPks(authHeader.Parameter);
                   //isAuthen = true;
                    resp.WSMsg.Rslc = authHeader.Parameter;

                //when authen is invalid
                if (!isAuthen)
                    return Unauthorized();


            }
            catch (Exception ex)
            {
                return BadRequest();
                //return "Invalid Request";
            }
            try

            {
                await mailService.SendEmailAsync(request);
                resData = "success";
                isError = false;
            
            }
            catch (Exception ex)
            {
                isError = true;
                resData = "error";
               
        
                
                resp.WSMsg.Msgdesc_en = "Error" + ex.Message.ToString();
                StringBuilder linemsg = new StringBuilder();
                linemsg.AppendLine("Parameter: ");
                linemsg.AppendLine("FromEmail: " + request.FromEmail + ", ");
                linemsg.AppendLine("ToEmail: " + request.ToEmail + ", ");
                linemsg.AppendLine("CCEmail: " + request.CCEmail + ", ");
                linemsg.AppendLine("BCCEmail: " + request.BCCEmail + ", ");
                linemsg.AppendLine("Subject: " + request.Subject + ", ");
                //linemsg.AppendLine("REFID: " + authHeader.Parameter + ", ");
                objMain.Call_Line_Bot("Error_SendMailAPI " + ex.Message.ToString() + linemsg);
                //Keeplog to MobiEx_UAT
                objMain.KeepLogEX("SendMailAPI", "SendMail", ex.Message.ToString() + " " + linemsg, "", "");


            }

           
            resp.WSMsg.Msgstatus = !isError ? "success" : "invalid";
            resp.WSMsg.Result = !isError ? resData : "invalid";
            
            return Ok(resp);

        }
    }
}
