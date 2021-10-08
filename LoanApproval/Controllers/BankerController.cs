using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanApproval.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace LoanApproval.Controllers
{
    [Route("LoanApproval/[controller]")]
    [ApiController]
    public class BankerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        IAmazonSQS sqsClient { get; set; }

        public BankerController(IAmazonSQS amazonSQS, IConfiguration configuration)
        {
            _configuration = configuration;
            this.sqsClient = amazonSQS;
        }

        // GET: LoanApproval/Banker
        [HttpGet]
        public JsonResult Get()
        {
            this.Response.ContentType = "text/json";
            this.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            string msg;
            List<Banker> lstloandetail = new List<Banker>();
            Banker loandetail = new Banker();

            try
            {
                lstloandetail = loandetail.Get_All_Loan();

                if (lstloandetail.Count <= 0)
                {
                    msg = "No loan application found.";
                    loandetail.LogMessage("Get Loan Data ----" + " " + msg.ToString());
                    return new JsonResult(msg, new JsonSerializerSettings { Formatting = Formatting.Indented });
                }
                else
                {
                    var content = JsonConvert.SerializeObject(lstloandetail);
                    loandetail.LogMessage("Get Loan Data ----" + " " + content.ToString());
                    return new JsonResult(lstloandetail, new JsonSerializerSettings { Formatting = Formatting.Indented });
                }
            }
            catch (Exception ex)
            {
                this.Response.StatusCode = 400;
                loandetail.LogMessage(ex.Message.ToString() + " " + ex.Message.ToString());
                return new JsonResult(ex.Message);
            }

        }

        // PUT: LoanApproval/Banker/5
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] Banker value)
        {
            this.Response.ContentType = "text/json";
            this.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            string msg = ""; int Loan_Status = 0; string qUrl; bool result = false; bool IsClosed = false;
            Banker loandetail = new Banker();

            try
            {
                //Check the if the loan application has closed.
                IsClosed = loandetail.CheckIsClosed(id);

                if (IsClosed == false)
                {
                    Loan_Status = value.LoanApplication_Status;
                    result = loandetail.Update_LoanInfo(value, id);

                    if (Loan_Status == 5 && result == true) //Loan_Status 5 i.e. Approved For Credit
                    {
                        value.LoanApplication_ID = id;

                        qUrl = _configuration.GetSection("QueueURL").Value.ToString();
                        var JsonMessage = JsonConvert.SerializeObject(value);

                        var res = sqsClient.SendMessageAsync(qUrl, JsonMessage);
                    }
                }

                if (result == true)
                {
                    if (Loan_Status == 5)
                    {
                        msg = "Loan has been approved and queued for the creditor.";
                        var content = JsonConvert.SerializeObject(value);
                        loandetail.LogMessage(msg + " " + content.ToString());
                    }
                    else
                    {
                        msg = "Loan data updated successfully.";
                        var content = JsonConvert.SerializeObject(value);
                        loandetail.LogMessage(msg + " " + content.ToString());
                    }
                }
                else if (IsClosed == true)
                {
                    msg = "This loan application is closed, You can not process this.";
                    var content = JsonConvert.SerializeObject(value);
                    loandetail.LogMessage(msg + " " + content.ToString());
                }
                else
                {
                    msg = "Loan application not updated.";
                    var content = JsonConvert.SerializeObject(value);
                    loandetail.LogMessage(msg + " " + content.ToString());
                }

                return new JsonResult(msg, new JsonSerializerSettings { Formatting = Formatting.Indented });
            }
            catch (Exception ex)
            {
                this.Response.StatusCode = 400;
                loandetail.LogMessage(ex.Message.ToString() + " " + ex.Message.ToString());
                return new JsonResult(ex.Message);
            }

        }

        // Options: LoanApproval/ApiWithActions/5
        [HttpDelete("{id}")]
        public JsonResult Options(int id)
        {
            try
            {
                HttpResponseMessage res = new HttpResponseMessage();

                this.Response.ContentType = "application/json";
                this.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return new JsonResult(res, new JsonSerializerSettings { Formatting = Formatting.Indented });
            }
            catch (Exception ex)
            {
                this.Response.StatusCode = 400;
                return new JsonResult(ex.Message);
            }

        }
    }
}
