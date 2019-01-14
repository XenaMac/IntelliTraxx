using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class Vars
    {
        public Guid ID { get; set; }

        [Required]
        [DisplayName("Name:")]
        [DataType(DataType.Text)]
        public string varName { get; set; }

        [Required]
        [DisplayName("Value:")]
        [DataType(DataType.Text)]
        public string varValue { get; set; }

        [Required]
        [DisplayName("Minimum Value:")]
        [DataType(DataType.Text)]
        public string varMinValue { get; set; }

        public int varType { get; set; }

    }
}