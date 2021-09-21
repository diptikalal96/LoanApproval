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

namespace LoanApproval.Controllers
{
    [Route("LoanApproval/[controller]")]
    [ApiController]
    public class BankerController : ControllerBase
    {
        IAmazonSQS sqsClient { get; set; }

        public BankerController(IAmazonSQS amazonSQS)
        {
            this.sqsClient = amazonSQS;
        }

        // GET: LoanApproval/Banker
        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                string msg;
                List<Banker> lstloandetail = new List<Banker>();
                Banker loandetail = new Banker();
                lstloandetail = loandetail.Get_All_Loan();

                this.Response.ContentType = "text/json";
                this.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                if (lstloandetail.Count <= 0)
                {
                    msg = "No loan application found.";
                    return new JsonResult(msg, new JsonSerializerSettings { Formatting = Formatting.Indented });
                }
                else
                {
                    return new JsonResult(lstloandetail, new JsonSerializerSettings { Formatting = Formatting.Indented });
                }
            }
            catch (Exception ex)
            {
                this.Response.StatusCode = 400;
                return new JsonResult(ex.Message);
            }
            
        }

        // PUT: LoanApproval/Banker/5
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] Banker value)
        {
            try
            {
                string msg = ""; int Loan_Status = 0;
                Banker loandetail = new Banker();
                Loan_Status = value.LoanApplication_Status;
                bool result = loandetail.Update_LoanInfo(value, id);


                if (Loan_Status == 4 && result == true)
                {
                    var JsonMessage = JsonConvert.SerializeObject(value);
                    string qUrl = "https://sqs.ap-south-1.amazonaws.com/052987743965/CreditDecision";

                    var res = sqsClient.SendMessageAsync(qUrl, JsonMessage);
                }

                if (result == true)
                {
                    if (Loan_Status == 4)
                    {
                        msg = "Loan has been approved and queued for the creditor.";
                    }
                    else
                    {
                        msg = "Loan data updated successfully.";
                    }

                }
                else
                {
                    msg = "Loan application not updated.";
                }

                this.Response.ContentType = "text/json";
                this.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                return new JsonResult(msg, new JsonSerializerSettings { Formatting = Formatting.Indented });
            }
            catch (Exception ex)
            {
                this.Response.StatusCode = 400;
                return new JsonResult(ex.Message);
            }
            
        }

    }
}
