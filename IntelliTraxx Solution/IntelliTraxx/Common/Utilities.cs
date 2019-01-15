﻿using IntelliTraxx.TruckService;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace IntelliTraxx.Common
{
    public static class Utilities
    {


        public static string GetApplicationSettingValue(string applicationSettingVariableName)
        {
            string retvalue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(applicationSettingVariableName))
                {
                    //using (MTCDbContext db = new MTCDbContext())
                    //{
                    //    if (db.MTCApplicationSettings.Any(p => p.Name == applicationSettingVariableName))
                    //    {
                    //        var mtcApplicationSetting = db.MTCApplicationSettings.FirstOrDefault(p => p.Name == applicationSettingVariableName);
                    //        if (mtcApplicationSetting != null)
                    //            retvalue = mtcApplicationSetting.Value;
                    //    }
                    //}
                }
            }
            catch
            {
                // ignored
            }
            return retvalue;
        }

        public static IEnumerable<SelectListItem> GetStateList()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem() {Text="Alabama", Value="AL"},
                new SelectListItem() { Text="Alaska", Value="AK"},
                new SelectListItem() { Text="Arizona", Value="AZ"},
                new SelectListItem() { Text="Arkansas", Value="AR"},
                new SelectListItem() { Text="California", Value="CA"},
                new SelectListItem() { Text="Colorado", Value="CO"},
                new SelectListItem() { Text="Connecticut", Value="CT"},
                new SelectListItem() { Text="District of Columbia", Value="DC"},
                new SelectListItem() { Text="Delaware", Value="DE"},
                new SelectListItem() { Text="Florida", Value="FL"},
                new SelectListItem() { Text="Georgia", Value="GA"},
                new SelectListItem() { Text="Hawaii", Value="HI"},
                new SelectListItem() { Text="Idaho", Value="ID"},
                new SelectListItem() { Text="Illinois", Value="IL"},
                new SelectListItem() { Text="Indiana", Value="IN"},
                new SelectListItem() { Text="Iowa", Value="IA"},
                new SelectListItem() { Text="Kansas", Value="KS"},
                new SelectListItem() { Text="Kentucky", Value="KY"},
                new SelectListItem() { Text="Louisiana", Value="LA"},
                new SelectListItem() { Text="Maine", Value="ME"},
                new SelectListItem() { Text="Maryland", Value="MD"},
                new SelectListItem() { Text="Massachusetts", Value="MA"},
                new SelectListItem() { Text="Michigan", Value="MI"},
                new SelectListItem() { Text="Minnesota", Value="MN"},
                new SelectListItem() { Text="Mississippi", Value="MS"},
                new SelectListItem() { Text="Missouri", Value="MO"},
                new SelectListItem() { Text="Montana", Value="MT"},
                new SelectListItem() { Text="Nebraska", Value="NE"},
                new SelectListItem() { Text="Nevada", Value="NV"},
                new SelectListItem() { Text="New Hampshire", Value="NH"},
                new SelectListItem() { Text="New Jersey", Value="NJ"},
                new SelectListItem() { Text="New Mexico", Value="NM"},
                new SelectListItem() { Text="New York", Value="NY"},
                new SelectListItem() { Text="North Carolina", Value="NC"},
                new SelectListItem() { Text="North Dakota", Value="ND"},
                new SelectListItem() { Text="Ohio", Value="OH"},
                new SelectListItem() { Text="Oklahoma", Value="OK"},
                new SelectListItem() { Text="Oregon", Value="OR"},
                new SelectListItem() { Text="Pennsylvania", Value="PA"},
                new SelectListItem() { Text="Rhode Island", Value="RI"},
                new SelectListItem() { Text="South Carolina", Value="SC"},
                new SelectListItem() { Text="South Dakota", Value="SD"},
                new SelectListItem() { Text="Tennessee", Value="TN"},
                new SelectListItem() { Text="Texas", Value="TX"},
                new SelectListItem() { Text="Utah", Value="UT"},
                new SelectListItem() { Text="Vermont", Value="VT"},
                new SelectListItem() { Text="Virginia", Value="VA"},
                new SelectListItem() { Text="Washington", Value="WA"},
                new SelectListItem() { Text="West Virginia", Value="WV"},
                new SelectListItem() { Text="Wisconsin", Value="WI"},
                new SelectListItem() { Text="Wyoming", Value="WY"}
            };
            return items;
        }

        public static IEnumerable<SelectListItem> GetCountryList()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Canada", Value = "Canada"},
                new SelectListItem{Text = "United Kingdom", Value = "United Kingdom"},
                new SelectListItem{Text = "United States", Value = "United States"},
                new SelectListItem{Text = "Mexico", Value = "Mexico"}
            };
            return items;
        }

        public static IEnumerable<SelectListItem> GetUserList()
        {
            TruckServiceClient truckService = new TruckServiceClient();
            List<User> users = truckService.getUsers(new Guid());
            IList<SelectListItem> items = new List<SelectListItem>();
            foreach (User u in users)
            {
                items.Add(new SelectListItem { Text = u.UserFirstName + " " + u.UserLastName, Value = u.UserID.ToString() });
            }
            return items;
        }

        public static IEnumerable<SelectListItem> GetVehicleClasses()
        {
            TruckServiceClient truckService = new TruckServiceClient();
            List<VehicleClass> classes = truckService.getVehicleClasses();
            IList<SelectListItem> items = new List<SelectListItem>();
            foreach (VehicleClass vc in classes)
            {
                items.Add(new SelectListItem { Text = vc.VehicleClassName, Value = vc.VehicleClassID.ToString() });
            }
            return items;
        }

        public static IEnumerable<SelectListItem> GetCompanies()
        {
            TruckServiceClient truckService = new TruckServiceClient();
            List<Company> companies = truckService.getCompanies(new Guid());
            IList<SelectListItem> items = new List<SelectListItem>();
            foreach (Company c in companies)
            {
                items.Add(new SelectListItem { Text = c.CompanyName, Value = c.CompanyID.ToString() });
            }
            return items;
        }
    }
}