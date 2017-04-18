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
	/// Interaction logic for BusinessTips.xaml
	/// </summary>
	public partial class BusinessTips : Window
	{
		private string connectString;
		private Business m_biz;

		public BusinessTips()
		{
			InitializeComponent();
		}

		public BusinessTips(string dbConnectString, Business new_biz)
		{
			InitializeComponent();
			connectString = dbConnectString;
			m_biz = new_biz;

			showTipDate();
		}

		private void showTipDate()
		{
			setColumns();

			try
			{
				businessNameLabel.Content = m_biz.name;
				businessAddressLabel.Content = m_biz.address;

				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;

						// Execute the tip query
						cmd.CommandText = String.Format(@"SELECT u.name, text, date, likes
																							FROM tip, users u WHERE tip.uid=u.uid AND bid='{0}';", m_biz.bid);

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								string name = reader.GetString(0);
								string text = reader.GetString(1);
								string date = reader.GetString(2);
								int likes = reader.GetInt32(3);
								tipsDataGrid.Items.Add(new TipDataItem() { name = name, text = text, date = date, likes = likes });
							}
						}
					}
					conn.Close();
				}
			}
			catch
			{
				businessNameLabel.Content = "ERROR";
				businessAddressLabel.Content = "Select a business from the search results";
			}
		}

		private void setColumns()
		{
			// Init tips of business Grid
			DataGridTextColumn col1 = new DataGridTextColumn();
			col1.Header = "User";
			col1.Binding = new Binding("name");
			tipsDataGrid.Columns.Add(col1);

			DataGridTextColumn col2 = new DataGridTextColumn();
			col2.Header = "Tip";
			col2.Binding = new Binding("text");
			tipsDataGrid.Columns.Add(col2);

			DataGridTextColumn col3 = new DataGridTextColumn();
			col3.Header = "Date";
			col3.Binding = new Binding("date");
			tipsDataGrid.Columns.Add(col3);

			DataGridTextColumn col4 = new DataGridTextColumn();
			col4.Header = "Likes";
			col4.Binding = new Binding("likes");
			tipsDataGrid.Columns.Add(col4);
		}
	}
}
