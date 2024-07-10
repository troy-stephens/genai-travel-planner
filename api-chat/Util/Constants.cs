using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_chat.Util
{
    public static class Constants
    {
        public const string AskForStartingDetails = @"Where will you be starting your trip from?  How long are you planning to travel?  What activities are you interested in?  

            Example: 1200 Central Ave. Charlotte, NC 28204.  5 Days and I like sight seeting, hiking, fly fishing, mountain biking.";
        public const string ICanHelpWith = @"I apologize but I can only help you with travel related requests i.e. recommendations for travel, activities etc.  Please make sure your request is travel related.";
    }
}
