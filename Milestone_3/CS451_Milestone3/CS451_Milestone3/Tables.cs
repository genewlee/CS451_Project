using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS451_Milestone3
{
	public class User
	{
		public string uid { get; set; }
		public string name { get; set; }
		public double avgStars { get; set; }
		public string yelpingSince { get; set; }
		public int numFans { get; set; }
		public int coolVotes { get; set; }
		public int funnyVotes { get; set; }
		public int usefulVotes { get; set; }

		public User() { }

		public User(string newUid, string newName, double newAvgStars, string newYelpingSince, int newNumFans, int newCool, int newFunny, int newUseful)
		{
			uid = newUid; name = newName; avgStars = newAvgStars; yelpingSince = newYelpingSince; numFans = newNumFans; coolVotes = newCool; funnyVotes = newFunny; usefulVotes = newUseful;
		}
	}

	public class FriendsTipDataItem
	{
		public string name { get; set; }
		public string bname { get; set; }
		public string bcity { get; set; }
		public string text { get; set; }
		public string date { get; set; }
	}

	public class Location
	{
		public string state { get; set; }
		public string city { get; set; }
		public string zipcode { get; set; }

		public void clear ()
		{
			state = null;
			city = null;
			zipcode = null;
		}
	}

	public class Business
	{
		public string bid { get; set; }
		public string name { get; set; }
		public string address { get; set; }
		public int numTips { get; set; }
		public int totalCheckins { get; set; }
		public string open { get; set; }
		public string close { get; set; }
	}
}
