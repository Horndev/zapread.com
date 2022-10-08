﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Tag
{
    /// <summary>
    /// 
    /// </summary>
    public class GetTagsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<TagItem> Tags { get; set; }
    }
}