using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using PFE_Dahboard;
using PFE_Dahboard.Models;

namespace PFE_Dahboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }





        public ActionResult GetEmailsOpenedByDayOfWeek(DateTime startDate, DateTime endDate, string category,string ageRange)
        {
            // Initialize a list to store the retrieved data
            List<EmailsOpenedModel> model = new List<EmailsOpenedModel>();

            // Connection string for the multidimensional database
            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";

            // MDX query to retrieve the number of opened emails grouped by day of the week
            string mdxQuery = "SELECT [Measures].[FACT Count] ON COLUMNS," +
                "  ORDER(FILTER([DIM DOEO].[Description].[Description].Members, NOT[DIM DOEO].[Description].CurrentMember.MemberValue = 'Unknown')," +
                "    CASE[DIM DOEO].[Description].CurrentMember.MemberValue" +
                "      WHEN 'Monday' THEN 1" +
                "      WHEN 'Tuesday' THEN 2" +
                "      WHEN 'Wednesday' THEN 3" +
                "      WHEN 'Thursday' THEN 4" +
                "      WHEN 'Friday' THEN 5" +
                "      WHEN 'Saturday' THEN 6" +
                "      WHEN 'Sunday' THEN 7" +
                "    END, ASC) ON ROWS" +
                " FROM [Prod Data Warehouse]" +
                " WHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "], [DIM Category].[Category].&[" + category + "]";
            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";
            // Open a connection to the multidimensional database
            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                // Create a command object with the MDX query
                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    // Add parameters to the command object for the start and end dates
                    command.Parameters.Add(new AdomdParameter("StartDate", startDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("EndDate", endDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("Category", category));


                    // Execute the query and retrieve the results as a CellSet
                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 1; i < cellSet.Axes[1].Positions.Count; i++)
                    {
                        // Retrieve the day of the week and number of opened emails for the current row
                        string dayOfWeek = cellSet.Axes[1].Positions[i].Members[0].Caption;
                        int emailsOpened = 0;

                        if (cellSet.Cells[i].FormattedValue != null)
                        {
                            try
                            {
                                emailsOpened = Convert.ToInt32(cellSet.Cells[i].FormattedValue);
                            }
                            catch (FormatException )
                            {

                                emailsOpened = 0;
                            }
                        }

                        // Create a new EmailsOpenedModel object with the retrieved data
                        EmailsOpenedModel data = new EmailsOpenedModel
                        {
                            DayOfWeek = dayOfWeek,
                            EmailsOpened = emailsOpened
                        };

                        // Add the object to the list of retrieved data
                        model.Add(data);
                    }



                }
            }

            // Return the list of retrieved data as a JSON result
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmailsByTimeOfDay(DateTime startDate, DateTime endDate, string category,string ageRange)
        {
            List<EmailsTimeModel> model = new List<EmailsTimeModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";
            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON COLUMNS,[DIM TOEO].[Description].[Description] ON ROWS FROM[Prod Data Warehouse]  WHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "], [DIM Category].[Category].&[" + category + "]";
            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";
            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    command.Parameters.Add(new AdomdParameter("StartDate", startDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("EndDate", endDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("Category", category));

                    CellSet cellSet = command.ExecuteCellSet();

                    Axis columnAxis = cellSet.Axes[0];
                    Axis rowAxis = cellSet.Axes[1];

                    for (int i = 1; i < rowAxis.Positions.Count - 1; i++)
                    {
                        string timeOfDay = rowAxis.Positions[i].Members[0].Caption;
                        int emailCount = Convert.ToInt32(cellSet.Cells[i].Value);
                        string formattedValue = emailCount.ToString();

                        EmailsTimeModel timeModel = new EmailsTimeModel()
                        {
                            TimeOfDay = timeOfDay,
                            EmailCount = formattedValue
                        };

                        model.Add(timeModel);
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetEmailsByStatus(DateTime startDate, DateTime endDate, string category, string ageRange)
        {
            List<EmailsStatusModel> model = new List<EmailsStatusModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";
            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON COLUMNS,\r\n[DIM Status].[Description].[Description] ON ROWS \r\nFROM[Prod Data Warehouse]\r\nWHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "], [DIM Category].[Category].&[" + category + "]";

            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";

            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    command.Parameters.Add(new AdomdParameter("StartDate", startDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("EndDate", endDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("Category", category));

                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 0; i < cellSet.Axes[1].Positions.Count - 1; i++)
                    {
                        EmailsStatusModel data = new EmailsStatusModel
                        {
                            Count = cellSet.Cells[i].Value != null ? cellSet.Cells[i].Value.ToString() : "0",
                            Status = cellSet.Axes[1].Positions[i].Members[0].Caption
                        };

                        model.Add(data);
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClicksAndViewsByDescription(DateTime startDate, DateTime endDate, string category, string ageRange)
        {
            List<ClicksAndViewsByDescriptionModel> model = new List<ClicksAndViewsByDescriptionModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";
            string mdxQuery = "SELECT {[Measures].[Nb Clicks], [Measures].[Nb Views]} ON COLUMNS,\r\nORDER([DIM DOEO].[Description].[Description].Members, \r\nCASE [DIM DOEO].[Description].CurrentMember.MemberValue\r\n    WHEN 'Monday' THEN 1\r\n    WHEN 'Tuesday' THEN 2\r\n    WHEN 'Wednesday' THEN 3\r\n    WHEN 'Thrusday' THEN 4\r\n    WHEN 'Friday' THEN 5\r\n    WHEN 'Saturday' THEN 6\r\n    WHEN 'Sunday' THEN 7\r\nEND, ASC) ON ROWS\r\nFROM [Prod Data Warehouse]\r\nWHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "], [DIM Category].[Category].&[" + category + "]";
            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";
            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    command.Parameters.Add(new AdomdParameter("StartDate", startDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("EndDate", endDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("Category", category));

                    CellSet cellSet = command.ExecuteCellSet();
                    for (int i = 0; i < cellSet.Axes[1].Positions.Count; i++)
                    {
                        string dayOfWeek = cellSet.Axes[1].Positions[i].Members[0].Caption;

                        // Ignore the unknown row and the row without a day of week
                        if (dayOfWeek != "Unknown" && dayOfWeek != "")
                        {
                            ClicksAndViewsByDescriptionModel data = new ClicksAndViewsByDescriptionModel
                            {
                                Clicks = cellSet.Cells[i * 2].Value != null ? cellSet.Cells[i * 2].Value.ToString() : "0",
                                Views = cellSet.Cells[i * 2 + 1].Value != null ? cellSet.Cells[i * 2 + 1].Value.ToString() : "0",
                                DayOfWeek = dayOfWeek
                            };

                            model.Add(data);
                        }
                    }


                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetEmailsByError(DateTime startDate, DateTime endDate , string category, string ageRange)
        {
            List<EmailsErrorModel> model = new List<EmailsErrorModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";

            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON COLUMNS,\r\n[DIM Error].[Description].[Description] ON ROWS \r\nFROM[Prod Data Warehouse]\r\n " +
                "WHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]," +
                "[DIM Category].[Category].&[" + category + "]";
            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";

            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    command.Parameters.Add(new AdomdParameter("StartDate", startDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("EndDate", endDate.ToString("yyyy-MM-ddTHH:mm:ss")));
                    command.Parameters.Add(new AdomdParameter("Category", category));

                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 1; i < cellSet.Axes[1].Positions.Count - 1; i++)
                    {
                        EmailsErrorModel data = new EmailsErrorModel
                        {
                            Count = cellSet.Cells[i].Value != null ? cellSet.Cells[i].Value.ToString() : "0",
                            ErrorDescription = cellSet.Axes[1].Positions[i].Members[0].Caption
                        };

                        model.Add(data);
                    }
                }
            }


            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllEmails(DateTime startDate, DateTime endDate, string category)
        {
            if (category == "Central")
            {
                category = "1";
            }
            else if (category == "No Tracking")
            {
                category = "2";
            }
            else
            {
                category = "3";
            }
            string connectionString = "Data Source=DESKTOP-0I4VE1T;Initial Catalog=Prod Data Warehouse;Integrated Security=True;";
            string query = $"SELECT TOP (20) * FROM [Prod Data Warehouse].[dbo].[FACT] WHERE SendDate BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' AND CategoryID = '{category}' OPTION (FAST 20);";

            List<EmailModel> emails = new List<EmailModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);
                command.CommandTimeout = 120;

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    EmailModel email = new EmailModel
                    {
                        EmailID = (int)reader["EmailID"],
                        OpCode = (int)reader["OpCode"],
                        CategoryID = (int)reader["CategoryID"],
                        SenderProfileID = (int)reader["SenderProfileID"],
                        Status = (int)reader["Status"],
                        Error = (int)reader["Error"],
                        CtcID = (int)reader["CtcID"],
                        SendDate = (DateTime)reader["SendDate"],
                        NbClicks = (int)reader["NbClicks"],
                        NbViews = (int)reader["NbViews"],
                        Unsubscribe = (int)reader["Unsubscribe"],
                        CostPerItem = (float)(double)reader["CostPerItem"]
                    };


                    emails.Add(email);
                }

                reader.Close();
            }

            return Json(emails, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGenderCounts(DateTime startDate, DateTime endDate  , string ageRange , string category)
        {
            List<GenderCountModel> model = new List<GenderCountModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";
            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON ROWS, {[DIM Gender].[Description].[Description]} ON COLUMNS FROM [Prod Data Warehouse] " +
                "WHERE ([Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]:[Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "], " +
                "[DIM Category].[Category].&[" + category + "]";

            if (!string.IsNullOrEmpty(ageRange))
            {
                mdxQuery += ", [DIM Age].[Description].&[" + ageRange + "]";
            }

            mdxQuery += ")";


            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    CellSet cellSet = command.ExecuteCellSet();
                    for (int i = 0; i < cellSet.Axes[1].Positions.Count ; i++)
                    {
                        for (int j = 0; j < cellSet.Axes[0].Positions.Count-1; j++)
                        {
                            GenderCountModel data = new GenderCountModel
                            {
                                Count = cellSet.Cells[j, i].Value != null ? int.Parse(cellSet.Cells[j, i].Value.ToString()) : 0,
                                Gender = cellSet.Axes[1].Positions[i].Members[0].Caption
                            };

                            model.Add(data);
                        }
                    }


                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetGenderAgeData(DateTime startDate, DateTime endDate, string category)
        {
            List<GenderAgeDataModel> model = new List<GenderAgeDataModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Prod Data Analysis\";";

            string mdxQuery = "SELECT [DIM Gender].[Description].[Description] ON ROWS," +
                " [DIM Age].[Description].[Description] ON COLUMNS FROM [Prod Data Warehouse]" +
                " WHERE ( [Measures].[FACT Count] , [Calendar].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]:[Calendar].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]," +
                " [DIM Category].[Category].&[" + category + "])";

            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    CellSet cellSet = command.ExecuteCellSet();

                    int rowCount = cellSet.Axes[0].Positions.Count;
                    int columnCount = cellSet.Axes[1].Positions.Count;

                    for (int row = 0; row < rowCount - 1; row++)
                    {
                        string gender = cellSet.Axes[0].Positions[row].Members[0].Caption;

                        for (int column = 0; column < columnCount - 1; column++)
                        {
                            string age = cellSet.Axes[1].Positions[column].Members[0].Caption;

                            int count = cellSet.Cells[row, column].Value != null ? int.Parse(cellSet.Cells[row, column].Value.ToString()) : 0;

                            GenderAgeDataModel data = new GenderAgeDataModel
                            {
                                Gender = gender,
                                Age = age,
                                Count = count
                            };

                            model.Add(data);
                        }
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


    }
}

