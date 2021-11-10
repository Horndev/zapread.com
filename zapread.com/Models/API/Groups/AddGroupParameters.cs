using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    public class AddGroupParameters
    {
        public string GroupName { get; set; }

        public int ImageId { get; set; }

        public string Tags { get; set; }

        public string Language { get; set; }
    }
}