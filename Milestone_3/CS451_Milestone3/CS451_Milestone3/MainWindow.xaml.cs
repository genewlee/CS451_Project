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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using Npgsql;
using CS451_Milestone3;

namespace CS451_Milestone3
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// For User Tab
	/// </summary>
	public partial class MainWindow : Window
	{
		private string connectString = "Host=127.0.0.1;Username=postgres;password=K@m3r0n4;Database=Milestone2DB";
		private User m_user;

		public MainWindow()
		{
			InitializeComponent();

			// initially hide error labels
			setUserIdValidLabel.Visibility = Visibility.Hidden;
			friendsValidLabel.Visibility = Visibility.Hidden;

			// Init business tab view
			InitializeBusinessTab();
		}

		/// <summary>
		/// Sets the global user
		/// </summary>
		private void setUserIdButton_Click(object sender, RoutedEventArgs e)
		{
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					// Retrieve all rows
					cmd.CommandText = String.Format("SELECT * FROM Users where uid='{0}';", setUserIdTextBox.Text);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string uid = reader.GetString(0);
							string name = reader.GetString(1);
							double avgStars = reader.GetDouble(2);
							string yelpingSince = reader.GetString(3);
							int numFans = reader.GetInt32(4);
							int cool = reader.GetInt32(5);
							int funny = reader.GetInt32(6);
							int useful = reader.GetInt32(7);
							//Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6} {7}", uid, name, avgStars, yelpingSince, numFans, cool, funny, useful));
							m_user = new User(uid, name, avgStars, yelpingSince, numFans, cool, funny, useful);
						}
						if (reader.HasRows) // valid user id
						{
							// Populate the view with the fetched data of user
							setUserIdValidLabel.Visibility = Visibility.Hidden;
							setUserInfo();
							displayTipsByFriends();
							displayFriends();
							selectedBusinessGroupBoxEnabled(); // Now that the user is set, the user can add tip text and/or checkin in the Business tab
						}
						else // invalid user id
						{
							setUserIdValidLabel.Visibility = Visibility.Visible;
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// Displays user info in the User Information Group Box
		/// </summary>
		private void setUserInfo()
		{
			userInfoNameTextBox.Text = m_user.name;
			userInfoStarsTextBox.Text = m_user.avgStars.ToString();
			userInfoFansTextBox.Text = m_user.numFans.ToString();
			userInfoYelpingSinceTextBox.Text = m_user.yelpingSince;
			userInfoVotesFunnyTextBox.Text = m_user.funnyVotes.ToString();
			userInfoVotesCoolTextBox.Text = m_user.coolVotes.ToString();
			userInfoVotesUsefulTextBox.Text = m_user.usefulVotes.ToString();
		}

		/// <summary>
		/// Fetches Db and displays Tips By Friends Group Box
		/// </summary>
		private void displayTipsByFriends()
		{
			tipsByFriendsDataGrid.Items.Clear();
			tipsByFriendsDataGrid.Columns.Clear();
			initColumnsToTipByFriendsGrid();
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					// Retrieve all rows
					cmd.CommandText = String.Format(@"SELECT F.name, Business.name, Business.city, Tip.text, Tip.Date 
																						FROM(
																									SELECT Users.uid, Users.name
																									FROM Friends, Users
																									WHERE Users.uid = Friends.fid
																									AND Friends.uid = '{0}'
																								) AS F, Tip, Business
																									WHERE F.uid = Tip.uid AND Tip.bid = Business.bid
																								ORDER BY Tip.Date DESC; ", m_user.uid);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string name = reader.GetString(0);
							string bname = reader.GetString(1);
							string bcity = reader.GetString(2);
							string text = reader.GetString(3);
							string date = reader.GetString(4);
							//Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}", name, bname, bcity, text, date));
							tipsByFriendsDataGrid.Items.Add(new TipDataItem { name = name, bname = bname, bcity = bcity, text = text, date = date });
						}
						setUserInfo();
					}
				}
				conn.Close();
			}
		}


		/// <summary>
		/// Fetches Db and displays Friends Group Box
		/// </summary>
		private void displayFriends()
		{
			friendsDataGrid.Items.Clear();
			friendsDataGrid.Columns.Clear();
			initColumnsToFriendsGrid();
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					// Retrieve all rows
					cmd.CommandText = String.Format(@"SELECT Users.name, Users.average_stars, Users.yelping_since, Users.uid
																						FROM Friends, Users
																						WHERE Users.uid = Friends.fid
																						AND Friends.uid = '{0}'
																						ORDER BY Users.name", m_user.uid);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string name = reader.GetString(0);
							double stars = reader.GetDouble(1);
							string since = reader.GetString(2);
							string uid = reader.GetString(3);
							friendsDataGrid.Items.Add(new User() { name = name, avgStars = stars, yelpingSince = since, uid = uid });
						}
						setUserInfo();
					}
				}
				conn.Close();
			}
		}


		/// <summary>
		/// Delete friend button was clicked
		/// If selection of friend was not made -> error message is displayed
		/// </summary>
		private void friendsRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			if (friendsDataGrid.SelectedIndex != -1)
			{
				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						User selectedRowFriend = friendsDataGrid.SelectedItem as User;

						// Execute the delete query
						cmd.CommandText = String.Format("DELETE FROM Friends WHERE uid='{0}' AND fid='{1}';", m_user.uid, selectedRowFriend.uid);
						cmd.ExecuteNonQuery();

						// refresh the friends data grid
						displayFriends();
					}
					conn.Close();
				}
			}
			else // friend was selected from the list
			{
				friendsValidLabel.Content = "Select a friend";
				friendsValidLabel.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Rate a selected friend
		/// If no friend is selected, error message is displayed
		/// If no rating was selected, error message is displayed
		/// </summary>
		private void friendsRateButton_Click(object sender, RoutedEventArgs e)
		{
			if (friendsDataGrid.SelectedIndex != -1)
			{
				if (rateFriendComboBox.SelectedIndex != -1)
				{
					friendsValidLabel.Visibility = Visibility.Hidden;
					using (var conn = new NpgsqlConnection(connectString))
					{
						conn.Open();
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = conn;
							User selectedRowFriend = friendsDataGrid.SelectedItem as User;

							// Grab the value from the combo box and recalcualte average stars
							double newAvgStars = Math.Round((selectedRowFriend.avgStars + double.Parse(rateFriendComboBox.SelectedValue.ToString())) / 2, 2);

							// Execute the update query
							cmd.CommandText = String.Format("UPDATE Users SET average_stars = {0} WHERE uid = '{1}';", newAvgStars, selectedRowFriend.uid);
							cmd.ExecuteNonQuery();

							// refresh the friends data grid
							displayFriends();
						}
						conn.Close();
					}
				}
				else // rating wasn't selected from combo box
				{
					friendsValidLabel.Content = "Select a rating";
					friendsValidLabel.Visibility = Visibility.Visible;
				}
			}
			else // friend was selected from the list
			{
				friendsValidLabel.Content = "Select a friend";
				friendsValidLabel.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Initializes the column headers in TipsByFriends grid
		/// </summary>
		private void initColumnsToTipByFriendsGrid()
		{
			// Init TipByFriends Grid
			DataGridTextColumn col1 = new DataGridTextColumn();
			col1.Header = "User Name";
			col1.Binding = new Binding("name");
			col1.Width = 75;
			tipsByFriendsDataGrid.Columns.Add(col1);

			DataGridTextColumn col2 = new DataGridTextColumn();
			col2.Header = "Business";
			col2.Binding = new Binding("bname");
			tipsByFriendsDataGrid.Columns.Add(col2);

			DataGridTextColumn col3 = new DataGridTextColumn();
			col3.Header = "City";
			col3.Binding = new Binding("bcity");
			tipsByFriendsDataGrid.Columns.Add(col3);

			DataGridTextColumn col4 = new DataGridTextColumn();
			col4.Header = "Text";
			col4.Binding = new Binding("text");
			tipsByFriendsDataGrid.Columns.Add(col4);

			DataGridTextColumn col5 = new DataGridTextColumn();
			col5.Header = "Date";
			col5.Binding = new Binding("date");
			tipsByFriendsDataGrid.Columns.Add(col5);
		}

		/// <summary>
		/// When a friend is selected, only show tips made from this friend
		/// </summary>
		private void friendTipsButton_Click(object sender, RoutedEventArgs e)
		{
			if (friendsDataGrid.SelectedIndex != -1)
			{
				tipsByFriendsDataGrid.Items.Clear();
				tipsByFriendsDataGrid.Columns.Clear();
				initColumnsToTipByFriendsGrid();
				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						var friend = friendsDataGrid.SelectedItem as User;

						cmd.Connection = conn;
						// Retrieve all rows
						cmd.CommandText = String.Format(@"SELECT F.name, Business.name, Business.city, Tip.text, Tip.Date 
																						FROM(
																									SELECT uid, name
																									FROM Users
																									WHERE uid = '{0}'
																								) AS F, Tip, Business
																									WHERE F.uid = Tip.uid AND Tip.bid = Business.bid
																								ORDER BY Tip.Date DESC; ", friend.uid);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								string name = reader.GetString(0);
								string bname = reader.GetString(1);
								string bcity = reader.GetString(2);
								string text = reader.GetString(3);
								string date = reader.GetString(4);
								tipsByFriendsDataGrid.Items.Add(new TipDataItem { name = name, bname = bname, bcity = bcity, text = text, date = date });
							}
							setUserInfo();
						}
					}
					conn.Close();
				}
			}
			else // friend was selected from the list
			{
				friendsValidLabel.Content = "Select a friend";
				friendsValidLabel.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Don't show this error message when a selection is made in friends data grid
		/// </summary>
		private void friendsDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			friendsValidLabel.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// When show all tips button is clicked, show default - tips of all friends
		/// </summary>
		private void allTipsButton_Click(object sender, RoutedEventArgs e)
		{
			displayTipsByFriends();
		}

		/// <summary>
		/// Initializes the column headers in Friends grid
		/// </summary>
		private void initColumnsToFriendsGrid()
		{
			// Init Friends Grid
			DataGridTextColumn col1 = new DataGridTextColumn();
			col1.Header = "Name";
			col1.Binding = new Binding("name");
			col1.Width = 80;
			friendsDataGrid.Columns.Add(col1);

			DataGridTextColumn col2 = new DataGridTextColumn();
			col2.Header = "Avg Stars";
			col2.Binding = new Binding("avgStars");
			col2.Width = 50;
			friendsDataGrid.Columns.Add(col2);

			DataGridTextColumn col3 = new DataGridTextColumn();
			col3.Header = "Yelping Since";
			col3.Binding = new Binding("yelpingSince");
			col3.Width = 83;
			friendsDataGrid.Columns.Add(col3);
		}
	}

	/// <summary>
	/// The following from hereafter is for logic in the Business Tab
	/// </summary>
	public partial class MainWindow : Window
	{
		Location m_loc; // A object to store the selected locations
		Business m_biz;

		private void InitializeBusinessTab()
		{
			m_loc = new Location();

			// populate states in business tab
			addStates();

			// init search results datagrid column headers
			initColumnsToSearchResultsGrid();

			// init disable filter of day/time until search is clicked
			resetComboBoxOpenBusiness();

			// init disable search of business till state, city and/or zip is selected
			searchBusinessButton.IsEnabled = false;
		}

		private void resetComboBoxOpenBusiness ()
		{
			dayOfWeekComboBox.IsEnabled = false;
			dayOfWeekComboBox.SelectedIndex = -1;
			timeFromComboBox.IsEnabled = false;
			timeFromComboBox.SelectedIndex = -1;
			timeToComboBox.IsEnabled = false;
			timeToComboBox.SelectedIndex = -1;
			show24HoursButton.IsEnabled = false;
		}

		/// <summary>
		/// Initializes the states where businesses are from the db
		/// </summary>
		private void addStates()
		{
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = "SELECT DISTINCT state FROM business ORDER BY state;";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							stateComboBox.Items.Add(reader.GetString(0));
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// When a state is selected, queries and populates the cities and zipcodes in that city
		/// </summary>
		private void stateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// reset and disable the date/time filters
			resetComboBoxOpenBusiness();

			// clear city list and zip code list
			cityListBox.Items.Clear();
			zipcodeListBox.Items.Clear();
			selectedCategoriesListBox.Items.Clear();

			// disable search for business button
			searchBusinessButton.IsEnabled = false;

			// enable the numBusinessperCatButton and avgStarsPerCatButton
			numBusinessperCatButton.IsEnabled = true;
			avgStarsPerCatButton.IsEnabled = true;

			m_loc.clear();  // clear the location object
			m_loc.state = stateComboBox.SelectedItem.ToString();
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = String.Format("SELECT DISTINCT city FROM business WHERE state='{0}' ORDER BY city", m_loc.state);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							cityListBox.Items.Add(reader.GetString(0));
						}
					}
					cmd.CommandText = String.Format("SELECT DISTINCT zipcode FROM business WHERE state='{0}' ORDER BY zipcode", m_loc.state);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							zipcodeListBox.Items.Add(reader.GetString(0));
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// When a selection in the city listbox is made queries and populates zip codes and also categories in the city
		/// </summary>
		private void cityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// clear city list and zip code list
			zipcodeListBox.Items.Clear();
			categoryListBox.Items.Clear();

			// enable search for business button
			searchBusinessButton.IsEnabled = true;

			m_loc.city = cityListBox.SelectedIndex != -1 ? cityListBox.SelectedItem.ToString() : null; // only set if its selected 
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = String.Format("SELECT DISTINCT zipcode FROM business WHERE state='{0}' AND city='{1}' ORDER BY zipcode", m_loc.state, m_loc.city);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							zipcodeListBox.Items.Add(reader.GetString(0));
						}
					}
					cmd.CommandText = String.Format(@"SELECT DISTINCT(category.name)
																						FROM business, category
																						WHERE business.bid = category.bid 
																						AND state ='{0}' AND city = '{1}'  ORDER BY category.name;", m_loc.state, m_loc.city);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							categoryListBox.Items.Add(reader.GetString(0));
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// When a selection in the zipcode listbox is made
		/// Queries for business categories matching state, city and/or zipcode if city is provided
		/// </summary>
		private void zipcodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// clear the categories list
			categoryListBox.Items.Clear();

			// enable the search button
			searchBusinessButton.IsEnabled = true;

			m_loc.zipcode = zipcodeListBox.SelectedIndex != -1 ? zipcodeListBox.SelectedItem.ToString() : null; // only set if its selected 
			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					if (m_loc.city != null) // city is also selected so include in query
					{
						cmd.CommandText = String.Format(@"SELECT DISTINCT(category.name)
																						FROM business, category
																						WHERE business.bid = category.bid 
																						AND state ='{0}' AND city = '{1}' AND zipcode = '{2}' ORDER BY category.name;", m_loc.state, m_loc.city, m_loc.zipcode);
					}
					else
					{
						cmd.CommandText = String.Format(@"SELECT DISTINCT(category.name)
																						FROM business, category
																						WHERE business.bid = category.bid 
																						AND state ='{0}' AND zipcode = '{1}' ORDER BY category.name;", m_loc.state, m_loc.zipcode);
					}
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							categoryListBox.Items.Add(reader.GetString(0));
						}
					}
				}
				conn.Close();
			}
		}


		/// <summary>
		///  Adds selected category from list to categories selected to search for
		///  Provides error message in the add button when nothing is selected to add
		/// </summary>
		private void addCategoryButton_Click(object sender, RoutedEventArgs e)
		{
			if (categoryListBox.SelectedIndex != -1)
			{
				selectedCategoriesListBox.Items.Add(categoryListBox.SelectedItem);
			}
			else
			{
				addCategoryButton.Foreground = Brushes.Red;
				addCategoryButton.Content = "Select a category to +";
			}
		}

		/// <summary>
		/// Resets error message on add button when there is a selection
		/// </summary>
		private void categoryListBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			addCategoryButton.Foreground = Brushes.Black;
			addCategoryButton.Content = "+";
		}

		/// <summary>
		///  Removes selected category from list of categories selected to search for
		///  Provides error message in the remove button when nothing is selected to remove
		/// </summary>
		private void removeCategoryButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedCategoriesListBox.SelectedIndex != -1)
			{
				selectedCategoriesListBox.Items.Remove(selectedCategoriesListBox.SelectedItem);
			}
			else
			{
				removeCategoryButton.Foreground = Brushes.Red;
				removeCategoryButton.Content = "Select a category to -";
			}
		}

		/// <summary>
		/// Resets error message on remove button when there is a selection
		/// </summary>
		private void selectedCategoriesListBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			removeCategoryButton.Foreground = Brushes.Black;
			removeCategoryButton.Content = "-";
		}

		/// <summary>
		/// Searches for busineses with selected location and categories
		/// </summary>
		private void searchBusinessButton_Click(object sender, RoutedEventArgs e)
		{
			// clear the search results
			searchResultsdataGrid.Items.Clear();

			// new search reset the day/time combox box filters
			resetComboBoxOpenBusiness();

			if (m_loc.state != null && (m_loc.city != null || m_loc.zipcode != null))
			{
				dayOfWeekComboBox.IsEnabled = true;
				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = buildSearchQueryWithCategories();
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								string bid = reader.GetString(0);
								string name = reader.GetString(1);
								string address = reader.GetString(2);
								string city = reader.GetString(3);
								string state = reader.GetString(4);
								string zipcode = reader.GetString(5);
								int reviewCount = reader.GetInt32(6);
								// TODO: NEED TO MAKE SURE DB DOESN'T HAVE NULL VALUES FOR THIS COLUMN
								int numCheckins;
								try
								{
									numCheckins = reader.GetInt32(7);
								}
								catch
								{
									numCheckins = 0;
								}
								searchResultsdataGrid.Items.Add(new Business() { bid = bid, name = name, address = address, city = city, state = state, zipcode = zipcode, totalCheckins = numCheckins, numTips = reviewCount});
							}
						}
					}
					conn.Close();
				}
			}
		}

		/// <summary>
		/// Day of the week is filtered for when business is open 
		/// </summary>
		private void dayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			searchResultsdataGrid.Items.Clear();
			timeFromComboBox.IsEnabled = true;
			timeFromComboBox.SelectedIndex = -1;
			timeToComboBox.SelectedIndex = -1;
			show24HoursButton.IsEnabled = true;

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = buildSearchQueryWithCategories(String.Format(" AND H.day='{0}'", dayOfWeekComboBox.SelectedItem));
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string bid = reader.GetString(0);
							string name = reader.GetString(1);
							string address = reader.GetString(2);
							string city = reader.GetString(3);
							string state = reader.GetString(4);
							string zipcode = reader.GetString(5);
							int reviewCount = reader.GetInt32(6);
							// TODO: NEED TO MAKE SURE DB DOESN'T HAVE NULL VALUES FOR THIS COLUMN
							int numCheckins;
							try
							{
								numCheckins = reader.GetInt32(7);
							}
							catch
							{
								numCheckins = 0;
							}
							string open = reader.GetString(8);
							string close = reader.GetString(9);
							searchResultsdataGrid.Items.Add(new Business() { bid = bid, name = name, address = address, city = city, state = state, zipcode = zipcode, totalCheckins = numCheckins, numTips = reviewCount, open = open, close = close });
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// Open time is filtered for when business is open 
		/// </summary>
		private void timeFromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// time-from is selected so clear and enable the time-to
			timeToComboBox.IsEnabled = true;
			timeToComboBox.SelectedIndex = -1;

			searchResultsdataGrid.Items.Clear();

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = buildSearchQueryWithCategories(String.Format(@" AND H.day='{0}' 
																																					AND H.open<='{1}' AND H.close>'{2}'",
																																					dayOfWeekComboBox.SelectedItem, timeFromComboBox.SelectedItem, timeFromComboBox.SelectedItem));
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string bid = reader.GetString(0);
							string name = reader.GetString(1);
							string address = reader.GetString(2);
							string city = reader.GetString(3);
							string state = reader.GetString(4);
							string zipcode = reader.GetString(5);
							int reviewCount = reader.GetInt32(6);
							// TODO: NEED TO MAKE SURE DB DOESN'T HAVE NULL VALUES FOR THIS COLUMN
							int numCheckins;
							try
							{
								numCheckins = reader.GetInt32(7);
							}
							catch
							{
								numCheckins = 0;
							}
							string open = reader.GetString(8);
							string close = reader.GetString(9);
							searchResultsdataGrid.Items.Add(new Business() { bid = bid, name = name, address = address, city = city, state = state, zipcode = zipcode, totalCheckins = numCheckins, numTips = reviewCount, open = open, close = close });
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// Close time is filtered for when business is open 
		/// </summary>
		private void timeToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			searchResultsdataGrid.Items.Clear();

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = buildSearchQueryWithCategories(String.Format(@" AND H.day='{0}' 
																																					AND H.open<='{1}' AND '{2}'<H.close
																																					AND H.open<='{3}' AND '{4}'<=H.close", 
																																					dayOfWeekComboBox.SelectedItem, timeFromComboBox.SelectedItem, timeFromComboBox.SelectedItem, timeToComboBox.SelectedItem, timeToComboBox.SelectedItem));
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string bid = reader.GetString(0);
							string name = reader.GetString(1);
							string address = reader.GetString(2);
							string city = reader.GetString(3);
							string state = reader.GetString(4);
							string zipcode = reader.GetString(5);
							int reviewCount = reader.GetInt32(6);
							// TODO: NEED TO MAKE SURE DB DOESN'T HAVE NULL VALUES FOR THIS COLUMN
							int numCheckins;
							try
							{
								numCheckins = reader.GetInt32(7);
							}
							catch
							{
								numCheckins = 0;
							}
							string open = reader.GetString(8);
							string close = reader.GetString(9);
							searchResultsdataGrid.Items.Add(new Business() { bid = bid, name = name, address = address, city = city, state = state, zipcode = zipcode, totalCheckins = numCheckins, numTips = reviewCount, open = open, close = close });
						}
					}
				}
				conn.Close();
			}
		}

		private void show24HoursButton_Click(object sender, RoutedEventArgs e)
		{
			searchResultsdataGrid.Items.Clear();
			timeFromComboBox.SelectedIndex = -1;
			timeToComboBox.SelectedIndex = -1;

			using (var conn = new NpgsqlConnection(connectString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = buildSearchQueryWithCategories(String.Format(@" AND H.day='{0}' AND H.open='00:00' AND '00:00'=H.close", dayOfWeekComboBox.SelectedItem));
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string bid = reader.GetString(0);
							string name = reader.GetString(1);
							string address = reader.GetString(2);
							string city = reader.GetString(3);
							string state = reader.GetString(4);
							string zipcode = reader.GetString(5);
							int reviewCount = reader.GetInt32(6);
							// TODO: NEED TO MAKE SURE DB DOESN'T HAVE NULL VALUES FOR THIS COLUMN
							int numCheckins;
							try
							{
								numCheckins = reader.GetInt32(7);
							}
							catch
							{
								numCheckins = 0;
							}
							string open = reader.GetString(8);
							string close = reader.GetString(9);
							searchResultsdataGrid.Items.Add(new Business() { bid = bid, name = name, address = address, city = city, state = state, zipcode = zipcode, totalCheckins = numCheckins, numTips = reviewCount, open = open, close = close });
						}
					}
				}
				conn.Close();
			}
		}

		/// <summary>
		/// Query string to query with state, city and/or zipcode and any selected categories
		/// </summary>
		/// <param name="queryToAppend"> additional queries after this more generic one</param>
		/// <returns></returns>
		private String buildSearchQueryWithCategories(String queryToAppend = "")
		{
			// build the query string based on if city and/or zip was selected
			StringBuilder query = new StringBuilder();

			if (queryToAppend == "") // There is no filtering of date/time
			{
				query.Append(String.Format(@"SELECT DISTINCT(B.bid), B.name, B.full_address, B.city, B.state, B.zipcode, B.review_count, B.numcheckins 
																					FROM business as B, category as C
																					WHERE B.bid = C.bid AND state='{0}' ", m_loc.state));
			}
			else
			{
				query.Append(String.Format(@"SELECT DISTINCT(B.bid), B.name, B.full_address, B.city, B.state, B.zipcode, B.review_count, B.numcheckins,
																					H.open, H.close
																					FROM business as B, category as C, hours H
																					WHERE B.bid = C.bid AND B.bid = H.bid
																					AND state='{0}' ", m_loc.state));
			}

			if (m_loc.city != null)
				query.Append(String.Format("AND city='{0}' ", m_loc.city));

			if (m_loc.zipcode != null)
				query.Append(String.Format("AND zipcode='{0}' ", m_loc.zipcode));

			// Get the categories selected to filter by category and append to query
			ItemCollection cats = selectedCategoriesListBox.Items;
			if (cats.Count > 0)
			{
				query.Append("AND (");
				foreach (var cat in cats)
					query.Append(String.Format("C.name='{0}' OR ", cat.ToString()));
				query.Remove(query.Length - 3, 3); // remove the last OR
				query.Append(")");
			}

			query.Append(queryToAppend); // if nothing is passed in, empty string by default

			return query.ToString();
		}

		private void selectedBusinessGroupBoxEnabled()
		{
			selectedBusinessGroupBox.IsEnabled = !selectedBusinessGroupBox.IsEnabled;
			checkinButton.IsEnabled = !checkinButton.IsEnabled;
			addTipButton.IsEnabled = !addTipButton.IsEnabled;
		}

		/// <summary>
		/// When business is selected from the search results grip
		/// Set the member variable business object and update the text box that shows selected business
		/// </summary>
		private void searchResultsdataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_biz = searchResultsdataGrid.SelectedItem as Business; // set the selected business as the member variable business object
			if (m_biz != null) // only set the selected business when selection is made not when the data grid refreshes also
			{
				selectedBusinessTextBox.Text = m_biz.name;
				showTipsButton.IsEnabled = true;
				showCheckinsButton.IsEnabled = true;
			}
			else // reset
			{
				selectedBusinessTextBox.Text = null;
				showTipsButton.IsEnabled = false;
				showCheckinsButton.IsEnabled = false;
			}
		}

		/// <summary>
		/// Checkin button was clicked
		/// Checkins user to this business
		/// </summary>
		private void checkinButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedBusinessTextBox.Text != "")
			{
				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						DateTime dt = DateTime.Now;
						String dayOfWeek = dt.DayOfWeek.ToString();
						int hour = dt.Hour;

						String checkinTime = "";
						if (6 <= hour && hour < 12)
							checkinTime = "num_morning = num_morning + 1";
						else if (12 <= hour && hour < 17)
							checkinTime = "num_afternoon = num_afternoon + 1";
						else if (17 <= hour && hour < 23)
							checkinTime = "num_evening = num_evening + 1";
						else
							checkinTime = "num_night = num_night + 1";

						cmd.Connection = conn;

						// Execute the checkin query
						cmd.CommandText = String.Format(@"UPDATE checkin 
																						SET {0} 
																					  WHERE bid ='{1}' and day='{2}';", checkinTime, m_biz.bid, dayOfWeek);
						int rowsAffected = cmd.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							checkinTipSuccessLabel.Foreground = Brushes.Black;
							checkinTipSuccessLabel.Content = "Checked In!";
						}
						else
						{
							checkinTipSuccessLabel.Foreground = Brushes.Red;
							checkinTipSuccessLabel.Content = "Error";
						}
					}
					conn.Close();
				}
			}
			else
			{
				checkinTipSuccessLabel.Foreground = Brushes.Red;
				checkinTipSuccessLabel.Content = "Select a business";
			}
			// Set a timer to display and remove the success label
			Timer timer = new Timer(5000);
			timer.Elapsed += new ElapsedEventHandler(clear_checkinTipSuccessLabel);
			timer.Enabled = true;
		}

		/// <summary>
		/// Clears the checkin/tip success label
		/// </summary>
		private void clear_checkinTipSuccessLabel(object source, ElapsedEventArgs e)
		{
			// Change UI component in UI thread
			Dispatcher.Invoke(() =>
			{
				checkinTipSuccessLabel.Content = "";
			});
		}

		/// <summary>
		/// Add tip button was clicked
		/// Adds the tip to the tip table in db for this business
		/// </summary>
		private void addTipButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedBusinessTextBox.Text != "")
			{
				using (var conn = new NpgsqlConnection(connectString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						DateTime dt = DateTime.Now;
						String date = dt.ToString("yyyy-MM-dd");

						cmd.Connection = conn;

						// Execute the tip query
						cmd.CommandText = String.Format(@"INSERT INTO tip(uid,bid,text,likes,date) VALUES ('{0}','{1}', '{2}', 0,'{3}');", m_user.uid, m_biz.bid, tipTextBox.Text, date);

						int rowsAffected = cmd.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							checkinTipSuccessLabel.Foreground = Brushes.Black;
							checkinTipSuccessLabel.Content = "Added Tip!";
						}
						else
						{
							checkinTipSuccessLabel.Foreground = Brushes.Red;
							checkinTipSuccessLabel.Content = "Error";
						}
					}
					conn.Close();
				}
			}
			else
			{
				checkinTipSuccessLabel.Foreground = Brushes.Red;
				checkinTipSuccessLabel.Content = "Select a business";
			}
			// Set a timer to display and remove the success label
			Timer timer = new Timer(5000);
			timer.Elapsed += new ElapsedEventHandler(clear_checkinTipSuccessLabel);
			timer.Enabled = true;
		}

		/// <summary>
		/// Creates a graph with checkins for that business
		/// </summary>
		private void showCheckinsButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
			CategoryStats win = new CategoryStats(connectString, m_biz);
			win.Show();
		}

		/// <summary>
		/// Creates a graph that shows with tips provided for the selected business
		/// </summary>
		private void showTipsButton_Click(object sender, RoutedEventArgs e)
		{
			BusinessTips win = new BusinessTips(connectString, m_biz);
			win.Show();
		}

		/// <summary>
		/// Creates a graph that shows number of business per category for the businesses
		/// </summary>
		private void numBusinessperCatButton_Click(object sender, RoutedEventArgs e)
		{
			string state = "", city = "", zip = "";
			if (stateComboBox.SelectedIndex != -1)
				state = stateComboBox.SelectedItem.ToString();
			if (cityListBox.SelectedIndex != -1)
				city = cityListBox.SelectedItem.ToString();
			if (zipcodeListBox.SelectedIndex != -1)
				zip = zipcodeListBox.SelectedItem.ToString();

			CategoryStats win = new CategoryStats(connectString, state, city, zip, false);
			win.Show();
		}

		/// <summary>
		/// Creates a graph that shows average number of star ratings per category
		/// </summary>
		private void avgStarsPerCatButton_Click(object sender, RoutedEventArgs e)
		{
			string state = "", city = "", zip = "";
			if (stateComboBox.SelectedIndex != -1)
				state = stateComboBox.SelectedItem.ToString();
			if (cityListBox.SelectedIndex != -1)
				city = cityListBox.SelectedItem.ToString();
			if (zipcodeListBox.SelectedIndex != -1)
				zip = zipcodeListBox.SelectedItem.ToString();

			CategoryStats win = new CategoryStats(connectString, state, city, zip, true);
			win.Show();
		}

		/// <summary>
		/// Initializes the column headers in SearchResults grid
		/// </summary>
		private void initColumnsToSearchResultsGrid()
		{
			// Init TipByFriends Grid
			DataGridTextColumn col1 = new DataGridTextColumn();
			col1.Header = "Business Name";
			col1.Binding = new Binding("name");
			searchResultsdataGrid.Columns.Add(col1);

			DataGridTextColumn col2 = new DataGridTextColumn();
			col2.Header = "Address";
			col2.Binding = new Binding("address");
			searchResultsdataGrid.Columns.Add(col2);

			DataGridTextColumn col3 = new DataGridTextColumn();
			col3.Header = "#ofTips";
			col3.Binding = new Binding("numTips");
			searchResultsdataGrid.Columns.Add(col3);

			DataGridTextColumn col4 = new DataGridTextColumn();
			col4.Header = "TotalCheckins";
			col4.Binding = new Binding("totalCheckins");
			searchResultsdataGrid.Columns.Add(col4);

			DataGridTextColumn col5 = new DataGridTextColumn();
			col5.Header = "Open";
			col5.Binding = new Binding("open");
			searchResultsdataGrid.Columns.Add(col5);

			DataGridTextColumn col6 = new DataGridTextColumn();
			col6.Header = "Close";
			col6.Binding = new Binding("close");
			searchResultsdataGrid.Columns.Add(col6);
		}
	}
}