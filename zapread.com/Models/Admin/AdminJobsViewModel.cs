using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Admin
{
    /// <summary>
    /// 
    /// </summary>
    public class AdminJobsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Hangfire.Storage.RecurringJobDto> RecurringJobs { get; set; }
    }
}