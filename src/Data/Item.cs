using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar.Data
{
    public class Item
    {
        public string Subject { get; set; }
        public bool IsRightToLeft { get; set; }
        public DateTime Start { get; set; }
        public string ID { get; set; }

        internal static bool IsStringRTL(string theString)
        {
            if (string.IsNullOrWhiteSpace(theString))
                return false;

            var firstLetter = theString[0].ToString();

            var isHeberw = Regex.IsMatch(firstLetter, @"\p{IsHebrew}");
            var isArabic = Regex.IsMatch(firstLetter, @"\p{IsArabic}");

            return isHeberw || isArabic;
        }
    }
}