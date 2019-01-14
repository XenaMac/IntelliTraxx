using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class EditUser
    {
        public Guid UserID { get; set; }
        
        [Required]
        [DisplayName("Last Name:")]
        [DataType(DataType.Text)]
        public string UserLastName { get; set; }

        [Required]
        [DisplayName("First Name:")]
        [DataType(DataType.Text)]
        public string UserFirstName { get; set; }

        [Required]
        [DisplayName("Email:")]
        [DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }

        [DataType(DataType.Text)]
        public string UserOffice { get; set; }

        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:###-###-####}")]
        public string UserPhone { get; set; }

        [Required]
        [DisplayName("Password:")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; }

        [Required]
        [DisplayName("Verify Password:")]
        [DataType(DataType.Password)]
        [Compare("UserPassword", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string VerifyUserPassword { get; set; }

        [DataType(DataType.Password)]
        public string UserSalt { get; set; }

        public string Roles { get; set; }
        public string Companies { get; set; }
    }
}