using Amazon.SQS;
using Amazon.SQS.Model;
using LoanApproval.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LoanApproval.Model
{
    public class Banker
    {
        public int LoanApplication_ID { get; set; }
        public int Applicant_ID { get; set; }
        public string Applicant_fname { get; set; }
        public string Applicant_mname { get; set; }
        public string Applicant_lname { get; set; }
        public int Business_ID { get; set; }
        public string Business_Name { get; set; }
        public decimal LoanApplication_Amount { get; set; }
        public DateTime LoanApplication_Date { get; set; }
        public string LoanApplication_Description { get; set; }
        public int LoanApplication_Status { get; set; }
        public string LoanApplication_BankerComment { get; set; }

        public List<Banker> Get_All_Loan()
        {
            List<Banker> loanlist = new List<Banker>();
            DBHelper dbHelper = new DBHelper();
            try
            {
                dbHelper.Connect(dbHelper.GetConnStr());

                //MySqlParameter[] banker_para = new MySqlParameter[1];
                //banker_para[0] = new MySqlParameter("Business_ID", Business_ID);
                MySqlDataReader loanReader = dbHelper.ExecuteReader("Get_All_Loans_For_Banker", DBHelper.QueryType.StotedProcedure, null);

                while (loanReader.Read())
                {
                    Banker loaniobj = new Banker();

                    loaniobj.LoanApplication_ID = int.Parse(loanReader["LoanApplication_ID"].ToString());
                    loaniobj.Applicant_ID = int.Parse(loanReader["Applicant_ID"].ToString());
                    loaniobj.Applicant_fname = loanReader["Applicant_fname"].ToString();
                    loaniobj.Applicant_mname = loanReader["Applicant_mname"].ToString();
                    loaniobj.Applicant_lname = loanReader["Applicant_lname"].ToString();
                    loaniobj.Business_ID = int.Parse(loanReader["Business_ID"].ToString());
                    loaniobj.Business_Name = loanReader["BusinessName"].ToString();
                    loaniobj.LoanApplication_Amount = Convert.ToDecimal(loanReader["LoanAmount"].ToString());
                    loaniobj.LoanApplication_Description = loanReader["LoanDescription"].ToString();
                    loaniobj.LoanApplication_Status = int.Parse(loanReader["LoanStatus"].ToString());
                    loaniobj.LoanApplication_Date = Convert.ToDateTime(loanReader["LoanApplication_Date"].ToString());
                    loaniobj.LoanApplication_BankerComment = loanReader["LoanBanker_Comment"].ToString();

                    loanlist.Add(loaniobj);
                }

                return loanlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbHelper.DisConnect();
                dbHelper = null;
            }
        }

        public bool Update_LoanInfo(Banker BankerPara, int id)
        {
            DBHelper dbHelper = new DBHelper();
            bool Result = false;
            try
            {
                dbHelper.Connect(dbHelper.GetConnStr());

                MySqlParameter[] loan_para = new MySqlParameter[3];
                loan_para[0] = new MySqlParameter("LoanApplication_ID", id);
                loan_para[1] = new MySqlParameter("LoanApplication_Status", BankerPara.LoanApplication_Status);
                loan_para[2] = new MySqlParameter("LoanApplication_BankerComment", BankerPara.LoanApplication_BankerComment);

                int r = dbHelper.Execute("Update_Loan_From_Banker", DBHelper.QueryType.StotedProcedure, loan_para);

                if (r == 1)
                {
                    Result = true;
                }

                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbHelper.DisConnect();
                dbHelper = null;
            }
        }

        public bool CheckIsClosed(int LoanApplication_ID)
        {
            DBHelper dbHelper = new DBHelper();
            bool Result = false;
            try
            {
                dbHelper.Connect(dbHelper.GetConnStr());

                MySqlParameter[] loan_para = new MySqlParameter[1];
                loan_para[0] = new MySqlParameter("Loan_ID", LoanApplication_ID);

                DataSet dsloan = dbHelper.ExecuteDS("Get_Loan_Info_By_Id", DBHelper.QueryType.StotedProcedure, loan_para);

                if (dsloan.Tables[0].Rows.Count > 0)
                {
                    if (int.Parse(dsloan.Tables[0].Rows[0]["LoanStatus"].ToString()) == 8)// Loan Status 8 i.e. Closed by External Service
                    {
                        Result = true;
                    }
                }

                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbHelper.DisConnect();
                dbHelper = null;
            }
        }

        public void LogMessage(string msg)
        {
            DBHelper dbHelper = new DBHelper();
            try
            {
                dbHelper.Connect(dbHelper.GetConnStr());

                MySqlParameter[] app_para = new MySqlParameter[1];
                app_para[0] = new MySqlParameter("LogMsg", msg);

                int r = dbHelper.Execute("Add_LogMsg", DBHelper.QueryType.StotedProcedure, app_para);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbHelper.DisConnect();
                dbHelper = null;
            }
        }
    }
}
