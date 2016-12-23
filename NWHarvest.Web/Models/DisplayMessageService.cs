using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWHarvest.Web.Models
{
    public class DisplayMessageService
    {
        public static string HomePage = "Home Page";
        private ApplicationDbContext _db;
        public DisplayMessageService(ApplicationDbContext db)
        {
            this._db = db;
        }

        public List<string> GetMessages(string displayDescription)
        {
            var messageList = new List<string>();

            var descriptionId = _db.DisplayDescriptions
                .Where(d => d.Description == displayDescription)
                .FirstOrDefault();

            if (descriptionId != null) { 

                messageList = _db.DisplayMessages
                    .Where(b => b.DisplayDescriptionId == descriptionId.DisplayDescriptionId)
                    .OrderBy(b => b.SortOrder)
                    .Select(b => b.Message)
                    .ToList();
            }

            return messageList;
        }
    }
}