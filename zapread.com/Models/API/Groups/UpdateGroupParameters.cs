using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    public class UpdateGroupParameters: AddGroupParameters
    {
        public int GroupId { get; set; }
    }
}