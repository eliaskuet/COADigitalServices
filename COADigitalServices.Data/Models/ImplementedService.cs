using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COADigitalServices.Data.Models
{
    public class ImplementedService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service Name is required")]
        [MaxLength(200, ErrorMessage = "Service Name cannot exceed 200 characters")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; }

        [MaxLength(500, ErrorMessage = "Short Brief cannot exceed 500 characters")]
        [Display(Name = "Short Brief")]
        public string ShortBrief { get; set; }

        [Url(ErrorMessage = "Icon URL must be a valid URL")]
        [MaxLength(1000, ErrorMessage = "Icon URL cannot exceed 1000 characters")]
        [Display(Name = "Icon URL")]
        public string IconUrl { get; set; }

        [Required(ErrorMessage = "Service URL is required")]
        [Url(ErrorMessage = "Service URL must be a valid URL")]
        [MaxLength(1000, ErrorMessage = "Service URL cannot exceed 1000 characters")]
        [Display(Name = "Service URL")]
        public string ServiceUrl { get; set; }

        [Required(ErrorMessage = "Created User ID is required")]
        [Display(Name = "Created By User ID")]
        public int CreatedUserId { get; set; }

        [Display(Name = "Creation Date")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Updated By User ID")]
        public int? UpdatedUserId { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
