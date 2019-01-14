using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class VehicleVM
    {
        public Guid ID { get; set; }

        [Required]
        [DisplayName("Vehicle ID:")]
        [DataType(DataType.Text)]
        public string VehicleIdentification { get; set; }

        [DisplayName("Vehicle Class:")]
        public string VehicleClass { get; set; }

        public Guid VehicleClassID { get; set; }

        [DisplayName("Company:")]
        public string Company { get; set; }

        public Guid CompanyID { get; set; }

        [Required]
        [DisplayName("License Plate:")]
        [DataType(DataType.Text)]
        public string LicensePlate { get; set; }

        [Required]
        [DisplayName("Vehicle Make:")]
        [DataType(DataType.Text)]
        public string Make { get; set; }

        [Required]
        [DisplayName("Vehicle Model:")]
        [DataType(DataType.Text)]
        public string VehicleModel { get; set; }

        [Required]
        [DisplayName("Vehicle Year:")]
        [RegularExpression("([1-9][0-9][0-9][0-9]*)", ErrorMessage = "Count must be a natural number")]
        public int Year { get; set; }

        [Required]
        [DisplayName("Haul Limit:")]
        [DataType(DataType.Text)]
        public int HaulLimit { get; set; }

        [Required]
        [DisplayName("Friendly Vehicle Name:")]
        [DataType(DataType.Text)]
        public string VehicleFriendlyName { get; set; }

        [Required]
        [DisplayName("Router MAC Address:")]
        [DataType(DataType.Text)]
        public string VehicleMACAddress { get; set; }

        [Required]
        [DisplayName("Router ID (Find on back of router):")]
        [DataType(DataType.Text)]
        public string RouterID { get; set; }
    }

}