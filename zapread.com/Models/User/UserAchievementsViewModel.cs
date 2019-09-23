﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    public class UserAchievementsViewModel
    {
        public List<UserAchievementViewModel> Achievements { get; set; }
    }

    public class UserAchievementViewModel
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public string Name { get; set; }
    }
}