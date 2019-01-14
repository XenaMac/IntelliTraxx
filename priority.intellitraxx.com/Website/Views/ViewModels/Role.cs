using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class Roles
    {
        public Guid RoleID { get; set; }

        [Required]
        [DisplayName("Role Name:")]
        [DataType(DataType.Text)]
        public string RoleName { get; set; }

        [Required]
        [DisplayName("Role Description:")]
        [DataType(DataType.Text)]
        [MaxLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string RoleDescription { get; set; }

        [DisplayName("Is Admin:")]
        public bool? IsAdmin { get; set; }
    }

}