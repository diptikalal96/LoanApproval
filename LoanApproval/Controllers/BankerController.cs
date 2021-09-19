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

        // GET: api/Banker
        [HttpGet]
        public List<Banker> Get()
        {
            List<Banker> lstloandetail = new List<Banker>();
            Banker loandetail = new Banker();
            lstloandetail = loandetail.Get_All_Loan();
            return lstloandetail;
        }

        //// GET: api/Banker/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Banker
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{

        //}

        // PUT: api/Banker/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] Banker value)
        {
            string msg = "";
            Banker loandetail = new Banker();
            bool result = loandetail.Update_LoanInfo(value, id);

            //string qUrl = "https://sqs.ap-south-1.amazonaws.com/052987743965/CreditDecision";
            //string messageBody = "This is a test message executed";
            //SendMessageResponse responseSendMsg = await sqsClient.SendMessageAsync(qUrl, messageBody);

            var client = new AmazonSQSClient();

            var request = new SendMessageRequest
            {
                DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
  {
    {
      "MyNameAttribute",new MessageAttributeValue
        { DataType ="String", StringValue ="John Doe" }
    },
    {
      "MyAddressAttribute",new MessageAttributeValue
        { DataType ="String", StringValue ="123 Main St." }
    },
    {
      "MyRegionAttribute",new MessageAttributeValue
        { DataType ="String", StringValue ="Any Town, United States" }
    }
  },
                MessageBody = "John Doe customer information.",
                QueueUrl = "https://sqs.ap-south-1.amazonaws.com/052987743965/CreditDecision"
            };

            var response = await sqsClient.SendMessageAsync(request);
            
            //Console.WriteLine("For message ID '" + response.MessageId + "':");
            //Console.WriteLine("  MD5 of message attributes: " +
            //  response.MD5OfMessageAttributes);
            //Console.WriteLine("  MD5 of message body: " + response.MD5OfMessageBody);

            if (result == true)
            {
                msg = "Loan data updated successfully for Loan " + id.ToString();
            }
            else
            {
                msg = "Loan data not updated.";
            }

            //

            //return msg;
        }

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{

        //}
    }
}
