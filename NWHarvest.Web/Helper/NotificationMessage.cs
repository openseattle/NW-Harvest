using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWHarvest.Web.Helper
{
    public class NotificationMessage
    {
        public string DestinationPhoneNumber { get; set; }

        public string DestinationEmailAddress { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}