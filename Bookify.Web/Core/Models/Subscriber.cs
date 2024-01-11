﻿using System.Xml.Linq;

namespace Bookify.Web.Core.Models
{
    [Index(nameof(NationalId), IsUnique = true)]
    [Index(nameof(MobileNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Subscriber:BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; } = null!;

        [MaxLength(15)]
        public string MobileNumber { get; set; } = null!;

        [MaxLength(150)]
        public string Email { get; set; } = null!;
        public bool HasWhatsApp { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; } = null!;

        [MaxLength(500)]
        public string ImageThumbnailPath { get; set; } = null!;

        public int AreaId { get; set; }
        public Area? Area { get; set; }

        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }

    }
}
