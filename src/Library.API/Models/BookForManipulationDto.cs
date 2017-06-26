using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "Please fill out a title.")]
        [MaxLength(100, ErrorMessage = "The title cannot have more than 100 characters.")]
        public string Title { get; set; }

        [Required()]
        [MaxLength(500, ErrorMessage = "The description cannot have more than 500 characters.")]
        public virtual string Description { get; set; }


    }
}
