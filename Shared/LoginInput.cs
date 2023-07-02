using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SharpBlaze.Shared
{
    public class LoginInput
    {
        public string ID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(maximumLength: 10,
            MinimumLength = 3,
            ErrorMessage = "Username must be 3 to 10 characters in length.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(maximumLength: 50,
            MinimumLength = 5,
            ErrorMessage = "Password must be 5 to 50 characters in length.")]
        public string Password { get; set; }
    }
}
