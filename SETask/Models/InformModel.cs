using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SETask.Models
{
    public class InformModel
    {
        public string AuthorId { get; set; }
        
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public HttpPostedFileBase Upload { get; set; }
    }
}