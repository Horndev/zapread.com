using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Admin
{
    public class AdminJobsViewModel
    {
        public List<Hangfire.Storage.RecurringJobDto> RecurringJobs { get; set; }
    }
}