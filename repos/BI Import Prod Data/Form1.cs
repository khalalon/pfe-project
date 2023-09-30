using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Reflection.Emit;

namespace BI_Import_Prod_Data
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public async void button1_Click(object sender, EventArgs e)
        {

            var ACCESS_TOKEN = GetUserToken();
            var getEmail = "https://testonebrand.atreemo.com//api/BIDWHouse/GetEmailTrendFact";
            List<EmailTrendFact> EmailListDataResponse = new List<EmailTrendFact>();
            APIResponse? JsonAPIResponse = new APIResponse();
            int pageSize = 10;
            bool hasMoreData = true;

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                for (int i = 1; i <= 40; i++)
                {
                    while (hasMoreData)
                    {
                        var parameters = new Dictionary<string, string>
                    {
                        { "PageSize", pageSize.ToString() }
                };
                        var response = await client.GetAsync(getEmail + "?" + string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}")));

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            JsonConvert.DeserializeObject<APIResponse>(result);
                            JsonAPIResponse = JsonConvert.DeserializeObject<APIResponse>(result);
                        }

                        if (JsonAPIResponse.EmailTrendFacts.Count > 0)
                        {
                            EmailListDataResponse.AddRange(JsonAPIResponse.EmailTrendFacts);
                        }
                        else
                        {
                            hasMoreData = false;
                        }
                    }
                    
                }
                hasMoreData = true;

            }



            DataTable EmailTrend = new DataTable();
            EmailTrend.Columns.Add(new DataColumn("EmailTrendFactID", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("CtcID", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("OpCode", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("PrjMkgTitle", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("Subject", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("CategoryID", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("Category", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("SendDate", typeof(DateTime)));
            EmailTrend.Columns.Add(new DataColumn("OpenDate", typeof(DateTime)));
            EmailTrend.Columns.Add(new DataColumn("TimeOfDayEmailOpened", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("DayOfWeekEmailOpened", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("SubmittedBy", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("SenderProfileID", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("SenderProfile", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("BrandName", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("DomainName", typeof(string)));
            EmailTrend.Columns.Add(new DataColumn("IsFollowUp", typeof(bool)));
            EmailTrend.Columns.Add(new DataColumn("IsComplaint", typeof(bool)));
            EmailTrend.Columns.Add(new DataColumn("Status", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("Error", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("FirstTrial", typeof(DateTime)));
            EmailTrend.Columns.Add(new DataColumn("LastTrial", typeof(DateTime)));
            EmailTrend.Columns.Add(new DataColumn("NbTrial", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("NbClicks", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("NbViews", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("Unsubscribe", typeof(int)));
            EmailTrend.Columns.Add(new DataColumn("CostPerItem", typeof(double)));
            EmailTrend.Columns.Add(new DataColumn("ModifiedDate", typeof(DateTime)));
            foreach (EmailTrendFact Email in EmailListDataResponse)
            {
                DataRow dr = EmailTrend.NewRow();
                dr["EmailTrendFactID"] = Email.EmailTrendFactID;
                dr["CtcID"] = Email.CtcID;
                dr["OpCode"] = Email.OpCode;
                dr["PrjMkgTitle"] = Email.PrjMkgTitle;
                dr["Subject"] = Email.Subject;
                dr["CategoryID"] = Email.CategoryID;
                dr["Category"] = Email.Category;
                dr["SendDate"] = Email.SendDate;
                if (Email.OpenDate == DateTime.MinValue)
                {
                    dr["OpenDate"] = DBNull.Value;
                }
                else
                {
                    dr["OpenDate"] = Email.OpenDate;
                }
                dr["TimeOfDayEmailOpened"] = Email.TimeOfDayEmailOpened;
                dr["DayOfWeekEmailOpened"] = Email.DayOfWeekEmailOpened;
                dr["SubmittedBy"] = Email.SubmittedBy;
                dr["SenderProfileID"] = Email.SenderProfileID;
                dr["SenderProfile"] = Email.SenderProfile;
                dr["BrandName"] = Email.BrandName;
                dr["DomainName"] = Email.DomainName;
                dr["IsFollowUp"] = Email.IsFollowUp;
                dr["IsComplaint"] = Email.IsComplaint;
                dr["Status"] = Email.Status;
                dr["Error"] = Email.Error;
                dr["FirstTrial"] = Email.FirstTrial;
                dr["LastTrial"] = Email.LastTrial;
                dr["NbTrial"] = Email.NbTrial;
                dr["NbClicks"] = Email.NbClicks;
                dr["NbViews"] = Email.NbViews;
                dr["Unsubscribe"] = Email.Unsubscribe;
                dr["CostPerItem"] = Email.CostPerItem;
                dr["ModifiedDate"] = Email.ModifiedDate;

                EmailTrend.Rows.Add(dr);

            }

            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=(localdb)\Prod;Initial Catalog=PFE Prod Database;Integrated Security=True";
            cnn = new SqlConnection(connetionString);
            cnn.Open();
            // 4. Insert the data into the database
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(cnn))
            {
                bulkCopy.DestinationTableName = "EmailTrend";
                bulkCopy.ColumnMappings.Add("EmailTrendFactID", "EmailTrendFactID");
                bulkCopy.ColumnMappings.Add("CtcID", "CtcID");
                bulkCopy.ColumnMappings.Add("OpCode", "OpCode");
                bulkCopy.ColumnMappings.Add("PrjMkgTitle", "PrjMkgTitle");
                bulkCopy.ColumnMappings.Add("Subject", "Subject");
                bulkCopy.ColumnMappings.Add("CategoryID", "CategoryID");
                bulkCopy.ColumnMappings.Add("Category", "Category");
                bulkCopy.ColumnMappings.Add("SendDate", "SendDate");
                bulkCopy.ColumnMappings.Add("OpenDate", "OpenDate");
                bulkCopy.ColumnMappings.Add("TimeOfDayEmailOpened", "TimeOfDayEmailOpened");
                bulkCopy.ColumnMappings.Add("DayOfWeekEmailOpened", "DayOfWeekEmailOpened");
                bulkCopy.ColumnMappings.Add("SubmittedBy", "SubmittedBy");
                bulkCopy.ColumnMappings.Add("SenderProfileID", "SenderProfileID");
                bulkCopy.ColumnMappings.Add("SenderProfile", "SenderProfile");
                bulkCopy.ColumnMappings.Add("BrandName", "BrandName");
                bulkCopy.ColumnMappings.Add("DomainName", "DomainName");
                bulkCopy.ColumnMappings.Add("IsFollowUp", "IsFollowUp");
                bulkCopy.ColumnMappings.Add("IsComplaint", "IsComplaint");
                bulkCopy.ColumnMappings.Add("Status", "Status");
                bulkCopy.ColumnMappings.Add("Error", "Error");
                bulkCopy.ColumnMappings.Add("FirstTrial", "FirstTrial");
                bulkCopy.ColumnMappings.Add("LastTrial", "LastTrial");
                bulkCopy.ColumnMappings.Add("NbTrial", "NbTrial");
                bulkCopy.ColumnMappings.Add("NbClicks", "NbClicks");
                bulkCopy.ColumnMappings.Add("NbViews", "NbViews");
                bulkCopy.ColumnMappings.Add("Unsubscribe", "Unsubscribe");
                bulkCopy.ColumnMappings.Add("CostPerItem", "CostPerItem");
                bulkCopy.ColumnMappings.Add("ModifiedDate", "ModifiedDate");

                MessageBox.Show("Connection Open !");
                bulkCopy.WriteToServer(EmailTrend);
            }
            cnn.Close();






        }

        public class EmailTrendFact
        {
            public int EmailTrendFactID { get; set; }
            public int CtcID { get; set; }
            public int OpCode { get; set; }
            public string PrjMkgTitle { get; set; }
            public string Subject { get; set; }
            public int CategoryID { get; set; }
            public string Category { get; set; }
            public DateTime SendDate { get; set; }
            public DateTime? OpenDate { get; set; }
            public int TimeOfDayEmailOpened { get; set; }
            public int DayOfWeekEmailOpened { get; set; }
            public string SubmittedBy { get; set; }
            public int SenderProfileID { get; set; }
            public string SenderProfile { get; set; }
            public string BrandName { get; set; }
            public string DomainName { get; set; }
            public bool IsFollowUp { get; set; }
            public bool IsComplaint { get; set; }
            public int Status { get; set; }
            public int Error { get; set; }
            public DateTime FirstTrial { get; set; }
            public DateTime LastTrial { get; set; }
            public int NbTrial { get; set; }
            public int NbClicks { get; set; }
            public int NbViews { get; set; }
            public int Unsubscribe { get; set; }
            public double CostPerItem { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        public class APIResponse
        {
            public bool Response { get; set; }
            public List<EmailTrendFact> EmailTrendFacts { get; set; }
            public List<object> Errors { get; set; }
        }

        public class Token

        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string userName { get; set; }
            public string issued { get; set; }
            public string expires { get; set; }
            public string email { get; set; }

        }
        public static string GetUserToken()

        {

            string UserName = "";
            string PassWord = "";

            UserName = "Khalil Kossentini";
            PassWord = "Redzedx123*";
            var getTokenUrl = "https://testonebrand.atreemo.com/token"; /* TO BE REMOVED */
            using (HttpClient httpClient = new HttpClient())
            {

                HttpContent content = new FormUrlEncodedContent(new[]

                {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", UserName),
                        new KeyValuePair<string, string>("password", PassWord)

                    });
                HttpResponseMessage request = httpClient.PostAsync(getTokenUrl, content).Result;
                if (!request.IsSuccessStatusCode)

                {
                    return "Username or Password are invalid.";
                }

                else

                {

                    string resultContent = request.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<Token>(resultContent);
                    return token.access_token;

                }

            }
        }


    }
}