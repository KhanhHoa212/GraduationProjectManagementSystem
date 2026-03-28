using GPMS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels
{
    public class ScheduleViewModel
    {
        public List<ScheduleEventViewModel> UpcomingEvents { get; set; } = new List<ScheduleEventViewModel>();
    }

    public class ScheduleEventViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public RoundType EventType { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
