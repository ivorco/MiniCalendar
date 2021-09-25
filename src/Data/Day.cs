using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCalendar.Data
{
    public struct Day
    {
        public DateTime Date { get; set; }
        public List<Event> Events { get; set; }
        public bool IsDayFuture { get { return Date.Date >= DateTime.Today; } }
    }
}
