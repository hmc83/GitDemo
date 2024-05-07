using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTO
{
    public class LoginDTO
    {
        [EmailAddress(ErrorMessage = "Email should be in proper email address format")]
        [Required(ErrorMessage = "Email cannot be blank")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password cannot be blank")]
        public string Password { get; set; }
    }
}
