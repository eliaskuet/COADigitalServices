using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace COADigitalServices.Data.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
