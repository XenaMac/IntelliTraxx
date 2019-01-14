using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.GlobalData
{
    public static class Helpers
    {
        /// <summary>
        /// Check to see if the user requesting the change is an admin user. If not, it will
        /// blow back and not allow the operation to continue
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        public static bool isAdmin(Guid operatorID)
        {
            bool ok = false;

            try
            {
                var roles = from ur in Users.GlobalUserData.userRoleList
                            join r in Users.GlobalUserData.roleList on ur.RoleID equals r.RoleID
                            where ur.UserID == operatorID
                            select new { r };
                foreach (var r in roles)
                {
                    if (r.r.isAdmin == true)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return ok;
        }

        public static DateTime makeDTFromTablet(string dateData)
        {
            try
            {
                string[] splitDate = dateData.Split(' ');
                string moDate = getMonth(splitDate[1]);
                string dtString = moDate + "/" + splitDate[2].ToString() + "/" + splitDate[5].ToString() + " " + splitDate[3].ToString();
                DateTime dtDate;
                if (DateTime.TryParse(dtString, out dtDate))
                {
                    return dtDate;
                }
                else
                {
                    return Convert.ToDateTime("01/01/2001 00:00:00");
                }
            }
            catch (Exception ex)
            {
                return Convert.ToDateTime("01/01/2001 00:00:00");
            }
        }

        private static string getMonth(string moName)
        {
            switch (moName.ToUpper())
            {
                case "JAN":
                    return "01";
                case "FEB":
                    return "02";
                case "MAR":
                    return "03";
                case "APR":
                    return "04";
                case "MAY":
                    return "05";
                case "JUN":
                    return "06";
                case "JUL":
                    return "07";
                case "AUG":
                    return "08";
                case "SEP":
                    return "09";
                case "OCT":
                    return "10";
                case "NOV":
                    return "11";
                case "DEC":
                    return "12";
                default:
                    return "00";
            }
        }
    }
}