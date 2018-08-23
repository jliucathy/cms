namespace SETask.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Paper")]
    public partial class Paper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Paper()
        {
            Reviews = new HashSet<Review>();
        }

        public int PaperId { get; set; }

        [Required (ErrorMessage = "you must put your article title")]
        [StringLength(50)]
        public string Title { get; set; }

        [Required(ErrorMessage = "KeyWprds are required")]
        [StringLength(50)]
        public string keyWords { get; set; }

        [Required(ErrorMessage ="Abstract is required")]
        public string Abstract { get; set; }

        //[Required(ErrorMessage = "File is required")]
        [StringLength(50)]
        public string PaperFile { get; set; }

        public int ConferenceId { get; set; }

        [StringLength(128)]
        public string AuthorId { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SubmitDate { get; set; }

        public bool Published { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual Conference Conference { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
