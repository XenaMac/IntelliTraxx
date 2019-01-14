using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Base_AVL.ViewModels
{
    public class Companies
    {
        public Guid CompanyID { get; set; }

        [Required]
        [DisplayName("Company Name:")]
        [DataType(DataType.Text)]
        public string CompanyName { get; set; }

        [Required]
        [DisplayName("Company Address:")]
        [DataType(DataType.Text)]
        public string CompanyAddress { get; set; }

        [Required]
        [DisplayName("Company City:")]
        [DataType(DataType.Text)]
        public string CompanyCity { get; set; }

        [Required]
        [DisplayName("Company State:")]
        [DataType(DataType.Text)]
        public string CompanyState { get; set; }

        [Required]
        [DisplayName("Company Country:")]
        [DataType(DataType.Text)]
        public string CompanyCountry { get; set; }

        [DisplayName("Parent Company:")]
        public bool IsParent { get; set; }

        [DisplayName("Company Contact:")]
        public Guid Contact { get; set; }

    }

}