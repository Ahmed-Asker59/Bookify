﻿namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
        public bool? HasWhatsApp { get; set; }
        public string? Email { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageThumbnailPath { get; set; }
        public string? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Address { get; set; }
        public bool IsBlackListed { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}