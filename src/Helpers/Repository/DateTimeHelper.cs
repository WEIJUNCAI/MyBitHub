using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHub.Helpers.Repository
{
    public static class DateTimeHelper
    {
        // Helper method calculate the elapsed time for files and commits
        // elapsed time expressed in days, hours, min
        // TODO: reimplement to express elapsed time in month and years
        public static string GetTimeDifference(DateTimeOffset timeStamp)
        {
            TimeSpan elapsedTime = DateTimeOffset.UtcNow - timeStamp;
            StringBuilder sb = new StringBuilder();

            if (elapsedTime.Days != 0)
                sb.Append(elapsedTime.Days).Append((elapsedTime.Days != 1) ? " days" : " day");
            else if (elapsedTime.Hours != 0)
                sb.Append(elapsedTime).Append((elapsedTime.Hours != 1) ? " hours" : " hour");
            else if (elapsedTime.Minutes != 0)
                sb.Append(elapsedTime.Minutes).Append(elapsedTime.Minutes != 1 ? " minutes" : " minute");
            else
                sb.Append(elapsedTime.Seconds).Append(elapsedTime.Seconds > 1 ? " seconds" : " second");

            sb.Append(" ago");
            return sb.ToString();
        }
    }
}
