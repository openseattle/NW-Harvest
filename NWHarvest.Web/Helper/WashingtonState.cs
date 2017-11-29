using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWHarvest.Web.Helper
{
    public class WashingtonState
    {
        public static List<string> GetCounties()
        {
            return new List<String>()
            {
                    "Adams",
                    "Asotin",
                    "Benton",
                    "Chelan",
                    "Clallam",
                    "Clark",
                    "Columbia",
                    "Cowlitz",
                    "Douglas",
                    "Ferry",
                    "Franklin",
                    "Garfield",
                    "Grant",
                    "Grays Harbor",
                    "Island",
                    "Jefferson",
                    "King",
                    "Kitsap",
                    "Kittitas",
                    "Klickitat",
                    "Lewis",
                    "Lincoln",
                    "Mason",
                    "Okanogan",
                    "Pacific",
                    "Pend Oreille",
                    "Pierce",
                    "San Juan",
                    "Skagit",
                    "Skamania",
                    "Snohomish",
                    "Spokane",
                    "Stevens",
                    "Thurston",
                    "Wahkiakum",
                    "Walla Walla",
                    "Whatcom",
                    "Whitman",
                    "Yakima",
                    "Unknown"
            };
        }
    }
}