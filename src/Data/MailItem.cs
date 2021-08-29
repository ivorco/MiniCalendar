using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar.Data
{
    public class MailItem : Item
    {
        public static MailItem FromOutlook(Outlook.MailItem mailItem)
        {
            var subject = mailItem.Subject;

            return new MailItem { ID = mailItem.EntryID, Subject = subject, Start = mailItem.ReceivedTime, IsRightToLeft = IsStringRTL(subject) };
        }
    }
}
