using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    public class Achievement
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public byte[] Image { get; set; }

        // An intrinsic assigned value to the achievement
        public int Value { get; set; }
    }

    public class UserAchievement
    {
        public int Id { get; set; }

        [InverseProperty("Achievements")]
        public User AchievedBy { get; set; }

        public Achievement Achievement { get; set; }

        public DateTime? DateAchieved { get; set; }
    }
}