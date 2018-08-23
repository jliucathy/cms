namespace SETask.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Conference")]
    public partial class Conference
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Conference()
        {
            Papers = new HashSet<Paper>();
        }

        public int ConferenceId { get; set; }

        [Column("Conference Name")]
        [Required]
        [StringLength(50)]
        public string Conference_Name { get; set; }

        [Column("Topic")]
        [Required(ErrorMessage ="Please set the conference topic")]
        public string Topic { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PaperDeadLine { get; set; }

        [Required]
        [StringLength(50)]
        public string Location { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public string StartingDate { get; set; }

        public int MaximumNoOfPaperSubmitted { get; set; }

        public int MaximumNoOfPaperPublished { get; set; }

        //[Required]
        [StringLength(128)]
        public string ChairId { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Paper> Papers { get; set; }
    }
}
