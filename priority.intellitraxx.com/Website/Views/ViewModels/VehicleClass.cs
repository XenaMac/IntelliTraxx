using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.ViewModels
{
    public class VehicleClasses
    {
        public Guid VehicleClassID { get; set; }

        [Required]
        [DisplayName("Vehicle Class Name:")]
        [DataType(DataType.Text)]
        public string VehicleClassName { get; set; }

        [Required]
        [DisplayName("Vehicle Class Description:")]
        [DataType(DataType.Text)]
        public string VehicleClassDescription { get; set; }

        [Required]
        [DisplayName("Vehicle Class Image:")]
        [DataType(DataType.Text)]
        [HiddenInput(DisplayValue = false)]
        public string VehicleClassImage { get; set; }

    }

}