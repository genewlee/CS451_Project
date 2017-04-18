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
