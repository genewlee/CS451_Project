using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace CS451_Milestone3
{
	/// <summary>
	/// Interaction logic for CategoryStats.xaml
	/// </summary>
	public partial class CategoryStats : Window
	{
		private string connectString;
		private string m_state, m_city, m_zipcode;
		private Business m_biz;

		public CategoryStats()
		{
			InitializeComponent();
		}

		public CategoryStats(string dbConnectString, string state, string city, string zipcode, bool forAvgStars)
		{
			InitializeComponent();
			connectString = dbConnectString;
			m_state = state;
			m_city = city;
			m_zipcode = zipcode;

			if (!forAvgStars)
				showGraphForBusinessPerCat();
			else
				showGraphForAvgStars();
		}

		//overload
		public CategoryStats(string dbConnectString, Business new_Biz)
		{
			InitializeComponent();
			connectString = dbConnectString;
			m_biz = new_Biz;

			showGraphForBizCheckins();
		}

		private void showGraphForBizCheckins()
		{
      catChart.Title = "Number of Checkins Each Day for " + m_biz.name;
			colSeries.Title = "# of Checkins";

			List<KeyValuePair<string, int>> data = new List<KeyValuePair<string, int>>();

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;

					// Execute the tip query
					cmd.CommandText = String.Format(@"SELECT day, num_morning+num_afternoon+num_evening+num_night
																						FROM checkin
																						WHERE bid='{0}' ", m_biz.bid);

					using (var reader = cmd.ExecuteReader())
					{
						var dayArr = new string[7];
						var countArr = new int[7];
						while (reader.Read())
						{
							string day = reader.GetString(0);
							int count = reader.GetInt32(1);
							if(day == "Sunday")
							{
								dayArr[0] = day;
								countArr[0] = count;
							}
							else if (day == "Monday")
							{
								dayArr[1] = day;
								countArr[1] = count;
							}
							else if (day == "Tuesday")
							{
								dayArr[2] = day;
								countArr[2] = count;
              }
							else if (day == "Wednesday")
							{
								dayArr[3] = day;
								countArr[3] = count;
              }
							else if (day == "Thursday")
							{
								dayArr[4] = day;
								countArr[4] = count;
              }
							else if (day == "Friday")
							{
								dayArr[5] = day;
								countArr[5] = count;
              }
							else if (day == "Saturday")
							{
								dayArr[6] = day;
								countArr[6] = count;
              }
							//data.Add(new KeyValuePair<string, int>(day, count));
						}
						for(int i = 0; i < 7; i++)
						{
							if(dayArr[i] != null)
							{
								data.Add(new KeyValuePair<string, int>(dayArr[i], countArr[i]));
							}
						}
					}
				}
				conn.Close();
			}
			catChart.Width = data.Count * 20; // set the width so that chart is scrollable
			catChart.DataContext = data;
		}

		private void showGraphForBusinessPerCat()
		{
			catChart.Title = "Number of Business per Category";
			colSeries.Title = "# of Businesses";

			List<KeyValuePair<string, int>> data = new List<KeyValuePair<string, int>>();

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;

					// Execute the tip query
					cmd.CommandText = String.Format(@"SELECT c.name, COUNT(c.bid)
																						FROM business b, category c
																						WHERE c.bid=b.bid
																						AND state='{0}' ", m_state);

					if (m_city != "")
						cmd.CommandText += String.Format("AND city='{0}' ", m_city);
					if (m_zipcode != "")
						cmd.CommandText += String.Format("AND zipcode='{0}' ", m_zipcode);

					cmd.CommandText += "GROUP BY c.name ORDER BY c.name;";

					using (var reader = cmd.ExecuteReader())
					{

						while (reader.Read())
						{
							string catName = reader.GetString(0);
							int count = reader.GetInt32(1);
							data.Add(new KeyValuePair<string, int>(catName, count));
						}
					}
				}
				conn.Close();
			}
			catChart.Width = data.Count * 20; // set the width so that chart is scrollable
			catChart.DataContext = data;
		}

		private void showGraphForAvgStars()
		{
			catChart.Title = "Average Stars per Category";
			colSeries.Title = "Avg stars";

			List<KeyValuePair<string, double>> data = new List<KeyValuePair<string, double>>();

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;

					// Execute the tip query
					cmd.CommandText = String.Format(@"SELECT c.name, SUM(b.stars)/COUNT(b.stars) as avgStars
																						FROM business b, category c
																						WHERE c.bid=b.bid
																						AND state='{0}' ", m_state);

					if (m_city != "")
						cmd.CommandText += String.Format("AND city='{0}' ", m_city);
					if (m_zipcode != "")
						cmd.CommandText += String.Format("AND zipcode='{0}' ", m_zipcode);

					cmd.CommandText += "GROUP BY c.name ORDER BY c.name;";

					using (var reader = cmd.ExecuteReader())
					{

						while (reader.Read())
						{
							string catName = reader.GetString(0);
							double avgStars = reader.GetDouble(1);
							data.Add(new KeyValuePair<string, double>(catName, avgStars));
						}
					}
				}
				conn.Close();
			}
			catChart.Width = data.Count * 20; // set the width so that chart is scrollable
			catChart.DataContext = data;
		}

	}
}
