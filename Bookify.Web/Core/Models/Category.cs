﻿namespace Bookify.Web.Core.Models
{
    public class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        //nullable
        public DateTime? LastUpdatedOn { get; set; }

    }
}