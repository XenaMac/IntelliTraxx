using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class DriverVM
    {
        public Guid DriverID { get; set; }

        [Required]
        [DisplayName("Driver Last Name:")]
        [DataType(DataType.Text)]
        public string DriverLastName { get; set; }

        [Required]
        [DisplayName("Driver First Name:")]
        [DataType(DataType.Text)]
        public string DriverFirstName { get; set; }

        [Required]
        [DisplayName("Driver Email:")]
        [DataType(DataType.EmailAddress)]
        public string DriverEmail { get; set; }

        [Required]
        [DisplayName("Driver Number:")]
        [DataType(DataType.Text)]
        public string DriverNumber { get; set; }

        [DisplayName("Driver Password;")]
        [DataType(DataType.Password)]
        public string DriverPassword { get; set; }

        [DisplayName("Verify Password;")]
        [DataType(DataType.Password)]
        [Compare("DriverPassword", ErrorMessage = "Confirm password doesn't match, Type again!")]
        public string VerifyDriverPassword { get; set; }

        [DisplayName("PIN:")]
        [StringLength(4, ErrorMessage = "The PIN cannot exceed 4 characters. ")]
        public String PIN { get; set; }

        [Required]
        [DisplayName("Company:")]
        public Guid CompanyID { get; set; }

        [DisplayName("Company:")]
        public string CompanyName { get; set; }

        [DisplayName("Driver Picture:")]
        [DataType(DataType.ImageUrl)]
        public string DriverPicture { get; set; }

        [DisplayName("Driver Picture:")]
        public byte[] imageDataField { get; set; }

        [DisplayName("Driver Picture String:")]
        public string image64String { get; set; }
        
        [DisplayName("Driver Picture Type:")]
        public string imageTypeField { get; set; }
    }

}