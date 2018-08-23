namespace SETask.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Review")]
    public partial class Review
    {
        public int ReviewId { get; set; }

        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public int PaperId { get; set; }

        [Required]
        [StringLength(128)]
        public string ReviewerId { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual Paper Paper { get; set; }
    }
}
