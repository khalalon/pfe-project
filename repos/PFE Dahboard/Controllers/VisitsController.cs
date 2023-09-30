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
    public class VisitsController : Controller
    {
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult GetAllVisits(DateTime startDate, DateTime endDate)
        {

            string connectionString = "Data Source=DESKTOP-0I4VE1T;Initial Catalog=PFE Visits;Integrated Security=True;";
            string query = $"SELECT TOP (20) * FROM [PFE Visits].[dbo].[VisitTrend] WHERE VisitDateTime BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' OPTION (FAST 20);";

            List<VisitModel> visits = new List<VisitModel>();

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
                    VisitModel visit = new VisitModel
                    {
                        VisitsFactID = (int)reader["VisitsFactID"],
                        Source = (string)reader["Source"],
                        SiteName = (string)reader["SiteName"],
                        DirectSpent = (double)reader["DirectSpent"],
                        IndirectSpent = (double)reader["IndirectSpent"],
                        CtcID = (int)reader["CtcID"],
                        VisitDateTime = (DateTime)reader["VisitDateTime"],
                        ModifiedDate = (DateTime)reader["ModifiedDate"],
                    };


                    visits.Add(visit);
                }

                reader.Close();
            }

            return Json(visits, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVisitsBySource(DateTime startDate, DateTime endDate, string ageRange)
        {
            List<VisitsBySourceModel> model = new List<VisitsBySourceModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Visits Data Analysis\";";

            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON COLUMNS, [DIM Source].[Source].[Source] ON ROWS FROM [Visits Data Warehouse]" +
                " WHERE ([Date].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Date].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]";

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
                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 0; i < cellSet.Axes[1].Positions.Count - 1 ; i++)
                    {
                        VisitsBySourceModel data = new VisitsBySourceModel
                        {
                            Count = cellSet.Cells[i].Value != null ? int.Parse(cellSet.Cells[i].Value.ToString()) : 0,
                            Source = cellSet.Axes[1].Positions[i].Members[0].Caption
                        };

                        model.Add(data);
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVisitsByDay(DateTime startDate, DateTime endDate, string ageRange)
        {
            List<VisitsByDayModel> model = new List<VisitsByDayModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Visits Data Analysis\";";
            string mdxQuery = "WITH \r\n  MEMBER [Date].[Day Of Week].[Monday] AS [Date].[Day Of Week].[Day 2]\r\n " +
                " MEMBER [Date].[Day Of Week].[Tuesday] AS [Date].[Day Of Week].[Day 3]\r\n " +
                " MEMBER [Date].[Day Of Week].[Wednesday] AS [Date].[Day Of Week].[Day 4]\r\n" +
                " MEMBER [Date].[Day Of Week].[Thursday] AS [Date].[Day Of Week].[Day 5]\r\n" +
                " MEMBER [Date].[Day Of Week].[Friday] AS [Date].[Day Of Week].[Day 6]\r\n " +
                " MEMBER [Date].[Day Of Week].[Saturday] AS [Date].[Day Of Week].[Day 7]\r\n " +
                " MEMBER [Date].[Day Of Week].[Sunday] AS [Date].[Day Of Week].[Day 1]\r\n" +
                "SELECT \r\n  {[Measures].[FACT Count]} ON COLUMNS, \r\n  " +
                "{[Date].[Day Of Week].[Monday]," +
                " [Date].[Day Of Week].[Tuesday]," +
                " [Date].[Day Of Week].[Wednesday], " +
                " [Date].[Day Of Week].[Thursday]," +
                " [Date].[Day Of Week].[Friday], [Date].[Day Of Week].[Saturday]," +
                " \r\n   [Date].[Day Of Week].[Sunday]} ON ROWS \r\n" +
                "FROM [Visits Data Warehouse]\r\n " +
                "WHERE ([Date].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Date].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]";
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
                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 0; i < cellSet.Axes[1].Positions.Count ; i++)
                    {
                        VisitsByDayModel data = new VisitsByDayModel
                        {
                            Count = cellSet.Cells[i].Value != null ? int.Parse(cellSet.Cells[i].Value.ToString()) : 0,
                            Day = cellSet.Axes[1].Positions[i].Members[0].Caption
                        };

                        model.Add(data);
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMostVisited(DateTime startDate, DateTime endDate, string ageRange)
        {
            List<MostVisitedModel> model = new List<MostVisitedModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Visits Data Analysis\";";
            string mdxQuery = "SELECT {[Measures].[FACT Count]} ON COLUMNS, \r\n  TopCount(Order([DIM Site].[Site Name].[Site Name].Members, [Measures].[FACT Count], DESC), 8) ON ROWS \r\nFROM [Visits Data Warehouse]\r\n WHERE ([Date].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Date].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]";
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
                    CellSet cellSet = command.ExecuteCellSet();

                    for (int i = 0; i < cellSet.Axes[1].Positions.Count ; i++)
                    {
                        MostVisitedModel data = new MostVisitedModel
                        {
                            Count = cellSet.Cells[i].Value != null ? int.Parse(cellSet.Cells[i].Value.ToString()) : 0,
                            Name = cellSet.Axes[1].Positions[i].Members[0].Caption
                        };

                        model.Add(data);
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVisitData(DateTime startDate, DateTime endDate)
        {
            List<VisitDataModel> model = new List<VisitDataModel>();

            string connectionString = "Provider=MSOLAP.8;Data Source=DESKTOP-0I4VE1T;Integrated Security=SSPI;Initial Catalog=\"Visits Data Analysis\";";
            string mdxQuery = "WITH \r\n  " +
                "MEMBER [Date].[Day Of Week].[Monday] AS [Date].[Day Of Week].[Day 2]\r\n " +
                "MEMBER [Date].[Day Of Week].[Tuesday] AS [Date].[Day Of Week].[Day 3]\r\n" +
                "MEMBER [Date].[Day Of Week].[Wednesday] AS [Date].[Day Of Week].[Day 4]\r\n " +
                "MEMBER [Date].[Day Of Week].[Thursday] AS [Date].[Day Of Week].[Day 5]\r\n " +
                "MEMBER [Date].[Day Of Week].[Friday] AS [Date].[Day Of Week].[Day 6]\r\n" +
                "MEMBER [Date].[Day Of Week].[Saturday] AS [Date].[Day Of Week].[Day 7]\r\n" +
                "MEMBER [Date].[Day Of Week].[Sunday] AS [Date].[Day Of Week].[Day 1]" +
                "SELECT   {[Date].[Day Of Week].[Monday]," +
                "[Date].[Day Of Week].[Tuesday]," +
                "[Date].[Day Of Week].[Wednesday],  " +
                "[Date].[Day Of Week].[Thursday], " +
                "[Date].[Day Of Week].[Friday]," +
                "[Date].[Day Of Week].[Saturday], \r\n   " +
                "[Date].[Day Of Week].[Sunday]} ON COLUMNS, " +
                "[DIM Age].[Description].[Description] ON ROWS " +
                "FROM [Visits Data Warehouse] " +
                "WHERE ([Measures].[FACT Count] , [Date].[Date].&[" + startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "]: [Date].[Date].&[" + endDate.ToString("yyyy-MM-ddTHH:mm:ss") + "])";

            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {
                connection.Open();

                using (AdomdCommand command = new AdomdCommand(mdxQuery, connection))
                {
                    CellSet cellSet = command.ExecuteCellSet();

                    int rowCount = cellSet.Axes[1].Positions.Count;
                    int columnCount = cellSet.Axes[0].Positions.Count;

                    for (int row = 0; row < rowCount - 1; row++)
                    {
                        string ageRange = cellSet.Axes[1].Positions[row].Members[0].Caption;

                        for (int column = 0; column < columnCount; column++)
                        {
                            string dayOfWeek = cellSet.Axes[0].Positions[column].Members[0].Caption;

                            int count = cellSet.Cells[column, row].Value != null ? int.Parse(cellSet.Cells[column, row].Value.ToString()) : 0;

                            VisitDataModel data = new VisitDataModel
                            {
                                AgeRange = ageRange,
                                DayOfWeek = dayOfWeek,
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