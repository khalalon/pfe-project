using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Reflection.Emit;

namespace Visits_Import_Data
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
            var getVisit = "https://testonebrand.atreemo.com/api/BIDWHouse/GetVisitsFact";
            List<VisitsFact> VisitListDataResponse = new List<VisitsFact>();
            APIResponse? JsonAPIResponse = new APIResponse();
            int pageSize = 10000;
            bool hasMoreData = true;

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(60);
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
                        var response = await client.GetAsync(getVisit + "?" + string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}")));

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            JsonConvert.DeserializeObject<APIResponse>(result);
                            JsonAPIResponse = JsonConvert.DeserializeObject<APIResponse>(result);
                        }

                        if (JsonAPIResponse.VisitsFacts.Count > 0)
                        {
                            VisitListDataResponse.AddRange(JsonAPIResponse.VisitsFacts);
                        }
                        else
                        {
                            hasMoreData = false;
                        }
                    }

                }
                hasMoreData = true;

            }



            DataTable VisitTrend = new DataTable();
            VisitTrend.Columns.Add(new DataColumn("VisitsFactID", typeof(int)));
            VisitTrend.Columns.Add(new DataColumn("CtcID", typeof(int)));
            VisitTrend.Columns.Add(new DataColumn("VisitDateTime", typeof(DateTime)));
            VisitTrend.Columns.Add(new DataColumn("SiteID", typeof(int)));
            VisitTrend.Columns.Add(new DataColumn("SiteName", typeof(string)));
            VisitTrend.Columns.Add(new DataColumn("DirectSpent", typeof(double)));
            VisitTrend.Columns.Add(new DataColumn("IndirectSpent", typeof(double)));
            VisitTrend.Columns.Add(new DataColumn("Source", typeof(string)));
            VisitTrend.Columns.Add(new DataColumn("ModifiedDate", typeof(DateTime)));


            foreach (VisitsFact Visit in VisitListDataResponse)
            {
                DataRow dr = VisitTrend.NewRow();
                dr["VisitsFactID"] = Visit.VisitsFactID;
                dr["CtcID"] = Visit.CtcID;
                dr["VisitDateTime"] = Visit.VisitDateTime;
                dr["SiteID"] = Visit.SiteID;
                dr["SiteName"] = Visit.SiteName;
                dr["DirectSpent"] = Visit.DirectSpent;
                dr["IndirectSpent"] = Visit.IndirectSpent;
                dr["Source"] = Visit.Source;
                dr["ModifiedDate"] = Visit.ModifiedDate;

                VisitTrend.Rows.Add(dr);

            }

            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=DESKTOP-0I4VE1T;Initial Catalog=""PFE Visits"";Integrated Security=True";
            cnn = new SqlConnection(connetionString);
            cnn.Open();
            // 4. Insert the data into the database
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(cnn))
            {
                bulkCopy.DestinationTableName = "VisitTrend";
                bulkCopy.ColumnMappings.Add("VisitsFactID", "VisitsFactID");
                bulkCopy.ColumnMappings.Add("CtcID", "CtcID");
                bulkCopy.ColumnMappings.Add("VisitDateTime", "VisitDateTime");
                bulkCopy.ColumnMappings.Add("SiteID", "SiteID");
                bulkCopy.ColumnMappings.Add("SiteName", "SiteName");
                bulkCopy.ColumnMappings.Add("DirectSpent", "DirectSpent");
                bulkCopy.ColumnMappings.Add("IndirectSpent", "IndirectSpent");
                bulkCopy.ColumnMappings.Add("Source", "Source");
                bulkCopy.ColumnMappings.Add("ModifiedDate", "ModifiedDate");


                MessageBox.Show("Connection Open !");
                bulkCopy.WriteToServer(VisitTrend);
            }
            cnn.Close();






        }

        public class VisitsFact
        {
            public int VisitsFactID { get; set; }
            public int CtcID { get; set; }
            public DateTime VisitDateTime { get; set; }
            public int SiteID { get; set; }
            public string SiteName { get; set; }
            public double DirectSpent { get; set; }
            public double IndirectSpent { get; set; }
            public string Source { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        public class APIResponse
        {
            public bool Response { get; set; }
            public List<VisitsFact> VisitsFacts { get; set; }
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