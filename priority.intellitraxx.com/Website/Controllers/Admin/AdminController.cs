using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Base_AVL.LATAService;
using Base_AVL.ViewModels;
using System.Security.Claims;
using System.Threading;
using Base_AVL.Toastr;
using Base_AVL.Common;
using System.IO;

namespace Base_AVL.Controllers.Administration
{
    public class AdminController : MessageControllerBase
    {
        TruckServiceClient truckService = new TruckServiceClient();

        // GET: Admin
        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult Index(string tab)
        {
            truckService.reloadVehicles();

            ViewBag.Tab = tab;
            Admin admin = new Admin();
            admin.Users = new List<ITUser>();

            //Get Users and apply to ITUsers
            List<User> serviceUsers = truckService.getUsers(new Guid()).OrderBy(u => u.UserLastName).ToList<User>();
            foreach(User u in serviceUsers)
            {
                ITUser itu = new ITUser();
                itu.user = u;
                itu.Roles = truckService.getUserRolesFull(u.UserID);
                itu.Companies = truckService.getUserCompaniesFull(u.UserID);
                admin.Users.Add(itu);
            }
            
            //Get Roles
            admin.Roles = truckService.getRoles(new Guid()).OrderBy(x => x.roleName).ToList<Role>();

            //Get Companies
            admin.Companies = truckService.getCompanies(new Guid()).OrderBy(c => c.CompanyName).ToList<Company>();

            //Get Vehicle Classes
            admin.VehicleClasses = truckService.getVehicleClasses().OrderBy(vc => vc.VehicleClassName).ToList<VehicleClass>();

            #region get vehicles
            List<VehicleExtendedData> VEDS = truckService.getExtendedData();
            admin.Vehicles = new List<VehicleVM>();
            foreach (VehicleExtendedData ved in VEDS)
            {
                VehicleVM nv = new VehicleVM();
                nv.CompanyID = ved.companyID;
                List<Company> vcompany = truckService.getCompanies(ved.companyID);
                nv.Company = vcompany[0].CompanyName;
                nv.VehicleClassID = ved.vehicleClassID;
                List<VehicleClass> vclass = truckService.getVehicleClasses();
                foreach (VehicleClass vc in vclass)
                {
                    if (vc.VehicleClassID == nv.VehicleClassID)
                    {
                        nv.VehicleClass = vc.VehicleClassName;
                    }
                }
                nv.HaulLimit = ved.haulLimit;
                nv.ID = ved.ID;
                nv.VehicleIdentification = ved.vehicleID;
                nv.LicensePlate = ved.licensePlate;
                nv.Make = ved.Make;
                nv.VehicleModel = ved.Model;
                nv.Year = ved.Year;
                nv.VehicleFriendlyName = ved.VehicleFriendlyName;
                admin.Vehicles.Add(nv);
            }
            #endregion

            #region get drivers
            List<Driver> truckDrivers = truckService.getDrivers();
            admin.Drivers = new List<DriverVM>();
            foreach (Driver d in truckDrivers)
            {
                DriverVM driver = new DriverVM();
                driver.DriverID = d.DriverID;
                driver.CompanyID = d.CompanyID;
                List<Company> company = truckService.getCompanies(driver.CompanyID);
                driver.CompanyName = company[0].CompanyName;
                driver.DriverFirstName = d.DriverFirstName;
                driver.DriverEmail = d.DriverEmail;
                driver.DriverNumber = d.DriverNumber;
                driver.DriverLastName = d.DriverLastName;
                driver.DriverPassword = d.DriverPassword;
                driver.PIN = d.PIN;
                //driver.DriverPicture = "~/Images/drivers/" + d.ProfilePic;
                driver.imageDataField = d.imageData;
                driver.imageTypeField = d.imageType;
                admin.Drivers.Add(driver);
            }
            #endregion

            #region Get App Variables
            List<systemvar> AppVariables = truckService.getAppVars();
            admin.AppVariables = new List<Vars>();
            foreach(systemvar var in AppVariables)
            {
                Vars v = new Vars();
                v.ID = var.varID;
                v.varName = var.varName;
                v.varValue = var.varVal;
                admin.AppVariables.Add(v);
            }
            #endregion

            #region Get Service Variables
            List<systemvar> SvcVariables = truckService.getServiceVars();
            admin.ServiceVariables = new List<Vars>();
            foreach (systemvar var in SvcVariables)
            {
                Vars v = new Vars();
                v.ID = var.varID;
                v.varName = var.varName;
                v.varValue = var.varVal;
                v.varMinValue = var.minValue;
                v.email = var.Email;
                admin.ServiceVariables.Add(v);
            }
            #endregion

            return View(admin);
        }

        [OutputCache(Duration = 5)]
        public ActionResult GetUsers()
        {
            List<ITUser> ITUsersList = new List<ITUser>();

            List<User> users = truckService.getUsers(new Guid());
            foreach(User user in users)
            {
                ITUser ITUser = new ITUser();
                ITUser.user = user;
                ITUser.Roles = truckService.getUserRolesFull(user.UserID);
                ITUser.Companies = truckService.getUserCompaniesFull(user.UserID);
                ITUsersList.Add(ITUser);
            }

            return Json(ITUsersList.OrderBy(c => c.user.UserLastName), JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddUser()
        {
            ViewBag.Roles= truckService.getRoles(new Guid());
            ViewBag.Companies = truckService.getCompanies(new Guid());
           
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddUser(AddUser model, string[] Companies, string[] Roles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                return View(model);
            }
            
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            LATAService.User u = new LATAService.User();
            u.UserID = Guid.NewGuid();
            u.UserEmail = model.UserEmail;
            u.UserFirstName = model.UserFirstName;
            u.UserLastName = model.UserLastName;
            u.UserOffice = model.UserOffice;
            u.UserPhone = model.UserPhone;
            u.UserPassword = model.UserPassword;

            string success = truckService.setUser(u, new Guid(sid));
            if (success == "OK")
            {
                foreach(var selection in Companies)
                {
                    string success2 = truckService.addUserToCompany(u.UserID, new Guid(selection), new Guid(sid));
                }

                foreach (var roleSelection in Roles)
                {
                    string success3 = truckService.addUserToRole(u.UserID, new Guid(roleSelection), new Guid(sid));
                }
            } else
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                this.AddToastMessage("Error", success, ToastType.Error);
                return View(model);
            }

            this.AddToastMessage("User Created", "Roles/Companies assigned", ToastType.Success);
            return Redirect("Index");
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditUser(string userID)
        {
            ViewBag.Roles = truckService.getRoles(new Guid());
            ViewBag.Companies = truckService.getCompanies(new Guid());

            User user = truckService.getUserProfile(new Guid(userID));
            EditUser EditUser = new EditUser();
            EditUser.UserID = user.UserID;
            EditUser.UserLastName = user.UserLastName;
            EditUser.UserFirstName = user.UserFirstName;
            EditUser.UserEmail = user.UserEmail;
            EditUser.UserOffice = user.UserOffice;
            EditUser.UserPhone = user.UserPhone;
            EditUser.UserPassword = user.UserPassword;
            EditUser.UserSalt = user.UserSalt;
            EditUser.VerifyUserPassword = user.UserPassword;
            AuthMgmt _authMgmt = new AuthMgmt();
            List<string> roleNames = _authMgmt.getUserRoles(user.UserID);
            foreach (string s in roleNames)
            {
                EditUser.Roles += s + "|";
            }
            List<Company> IntelliTruxxUserCompanies = truckService.getUserCompaniesFull(EditUser.UserID);
            foreach (Company c in IntelliTruxxUserCompanies)
            {
                EditUser.Companies += c.CompanyName + "|";
            }

            return View(EditUser);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditUser(EditUser model, string[] Companies, string[] Roles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                AuthMgmt _authMgmt = new AuthMgmt();
                List<string> roleNames = _authMgmt.getUserRoles(model.UserID);
                foreach (string s in roleNames)
                {
                    model.Roles += s + "|";
                }
                List<Company> IntelliTruxxUserCompanies = truckService.getUserCompaniesFull(model.UserID);
                foreach (Company c in IntelliTruxxUserCompanies)
                {
                    model.Companies += c.CompanyName + "|";
                }
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Grab current edited user to see if password has changed
            LATAService.User eu = truckService.getUserProfile(model.UserID); 

            //compare EU to model tos ee if pwd has changed
            if (eu.UserPassword != model.UserPassword)
            {
                eu.UserID = model.UserID;
                eu.UserEmail = model.UserEmail;
                eu.UserFirstName = model.UserFirstName;
                eu.UserLastName = model.UserLastName;
                eu.UserOffice = model.UserOffice;
                eu.UserPhone = model.UserPhone;
                eu.UserSalt = string.Empty;
                eu.UserPassword = model.UserPassword;
            }
            else
            {
                eu.UserID = model.UserID;
                eu.UserEmail = model.UserEmail;
                eu.UserFirstName = model.UserFirstName;
                eu.UserLastName = model.UserLastName;
                eu.UserOffice = model.UserOffice;
                eu.UserPhone = model.UserPhone;
                eu.UserSalt = string.Empty;
                eu.UserPassword = model.UserPassword;
                eu.UserPassword = model.UserPassword;
                eu.UserSalt = model.UserSalt;
            }

            string success = truckService.setUser(eu, new Guid(sid));

            #region blow away user roles and companies
            //blow away all roles with userid
            List<Guid> UserRoles = truckService.getUserRolesGuids(eu.UserID);
            foreach (Guid roleID in UserRoles)
            {
                success = truckService.removeUserFromRole(eu.UserID, roleID, new Guid(sid));
            }

            //blow away all roles with userid
            List<Guid> UserCompanies = truckService.getUserCompanies(eu.UserID);
            foreach (Guid companyID in UserCompanies)
            {
                success = truckService.removeUserFromCompany(eu.UserID, companyID, new Guid(sid));
            }
            #endregion

            #region add user roles and companies
            foreach (var roleSelection in Roles)
            {
                success = truckService.addUserToRole(eu.UserID, new Guid(roleSelection), new Guid(sid));
            }

            foreach (var selection in Companies)
            {
                success = truckService.addUserToCompany(eu.UserID, new Guid(selection), new Guid(sid));
            }
            #endregion

            if(success != "OK")
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                ViewBag.UserRoles = truckService.getUserRolesFull(model.UserID);
                ViewBag.UserCompanies = truckService.getUserCompaniesFull(model.UserID);
                this.AddToastMessage("Error", success, ToastType.Error);
                return View(model);
            }

            this.AddToastMessage("User Modified", "", ToastType.Success);
            return Redirect("Index");
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteUser(string userID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get user
            User u = truckService.getUserProfile(new Guid(userID));

            string success = truckService.deleteUser(u, new Guid(sid));

            if (success == "OK")
            {
                this.AddToastMessage("Success", "User was deleted", ToastType.Success);
                return Redirect("Index");
            }
            else
            {
                this.AddToastMessage("Error", success, ToastType.Error);
                return RedirectToAction("EditUser", new { userid = userID });
            }
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddRole()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddRole(Roles model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            Role r = new Role();
            r.RoleID = Guid.NewGuid();
            r.roleName = model.RoleName;
            r.roleDescription = model.RoleDescription;
            if(model.IsAdmin == true)
            {
                r.isAdmin = true;
            }
            else
            {
                r.isAdmin = false;
            }

            string success = truckService.setRole(r, new Guid(sid));

            if (success != "OK")
            {
                this.AddToastMessage("Error", success, ToastType.Success);
                return View(model);
            }

            this.AddToastMessage("Role Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "R" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditRole(string roleID)
        {
            Guid RoleID = new Guid(roleID);
            List<Role> Role = truckService.getRoles(RoleID);
            Roles role = new Roles();
            role.RoleID = Role[0].RoleID;
            role.RoleName = Role[0].roleName;
            role.RoleDescription = Role[0].roleDescription;
            role.IsAdmin = Role[0].isAdmin;
            if(Role[0] == null)
            {
                this.AddToastMessage("Error: Role does not exist", "The role you are trying to edit does not seem to be in our database.", ToastType.Success);
                return View("Index", new { tab = "R" });
            }
            else
            {
                return View(role);
            }            
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteRole(string roleID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get role
            List<Role> r = truckService.getRoles(new Guid(roleID));

            string success = truckService.deleteRole(r[0], new Guid(sid));

            if (success == "OK")
            {
                this.AddToastMessage("Success", "Role was deleted", ToastType.Success);
                return RedirectToAction("Index", new { tab = "R" });
            }
            else
            {
                this.AddToastMessage("Error", success, ToastType.Error);
                return RedirectToAction("EditRole", new { roleID = r[0].RoleID });
            }
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditRole(Roles model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


            LATAService.Role r = new LATAService.Role();
            r.RoleID = model.RoleID;
            r.roleName = model.RoleName;
            r.roleDescription = model.RoleDescription;
            if(model.IsAdmin == true)
            {
                r.isAdmin = true;
            }
            else
            {
                r.isAdmin = false;
            }

            string success = truckService.setRole(r, new Guid(sid));

            if (success != "OK")
            {
                this.AddToastMessage("Error", success, ToastType.Error);
                return View(model);
            }

            this.AddToastMessage("Role Modified", "", ToastType.Success);
            return RedirectToAction("Index", "Admin", new { tab = "R" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddCompany()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddCompany(Companies model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            Company company = new Company();
            company.CompanyID = Guid.NewGuid();
            company.CompanyName = model.CompanyName;
            company.CompanyAddress = model.CompanyAddress;
            company.CompanyCity = model.CompanyCity;
            company.CompanyState = model.CompanyState;
            company.CompanyCountry = model.CompanyCountry;
            company.isParent = false;
            company.CompanyContact = truckService.getUserProfile(model.Contact);

            string success = truckService.setCompany(company, new Guid(sid));

            if (success != "OK")
            {
                this.AddToastMessage("Error", success, ToastType.Success);
                return View(model);
            }

            this.AddToastMessage("Role Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "C" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditCompany(string companyID)
        {
            Guid CompanyID = new Guid(companyID);
            List<Company> Co = truckService.getCompanies(CompanyID);
            Companies company = new Companies();
            company.CompanyID = Co[0].CompanyID;
            company.CompanyName = Co[0].CompanyName;
            company.CompanyAddress = Co[0].CompanyAddress;
            company.CompanyCity = Co[0].CompanyCity;
            company.CompanyState = Co[0].CompanyState;
            company.CompanyCountry = Co[0].CompanyCountry;
            company.IsParent = Co[0].isParent;
            company.Contact = Co[0].CompanyContact.UserID;

            if (Co[0] == null)
            {
                this.AddToastMessage("Error: Company does not exist", "The company you are trying to edit does not seem to be in our database.", ToastType.Success);
                return View("Index", new { tab = "R" });
            }
            else
            {
                return View(company);
            }
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditCompany(Companies model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            LATAService.Company company = new LATAService.Company();
            company.CompanyID = model.CompanyID;
            company.CompanyName = model.CompanyName;
            company.CompanyAddress = model.CompanyAddress;
            company.CompanyCity = model.CompanyCity;
            company.CompanyState = model.CompanyState;
            company.CompanyCountry = model.CompanyCountry;
            company.isParent = model.IsParent;
            company.CompanyContact = truckService.getUserProfile(model.Contact);

            string success = truckService.setCompany(company, new Guid(sid));

            if (success != "OK")
            {
                this.AddToastMessage("Error", success, ToastType.Error);
                return View(model);
            }

            this.AddToastMessage("Company Modified", "", ToastType.Success);
            return RedirectToAction("Index", "Admin", new { tab = "C" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteCompany(string companyID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get role
            List<Company> co = truckService.getCompanies(new Guid(companyID));

            if (co[0].isParent == true)
            {
                this.AddToastMessage("Error", "Cannot delete Parent Company", ToastType.Success);
                return RedirectToAction("Index", new { tab = "C" });
            }
            else
            {
                string success = truckService.deleteCompany(co[0], new Guid(sid));

                if (success == "OK")
                {
                    this.AddToastMessage("Success", "Company was deleted", ToastType.Success);
                    return RedirectToAction("Index", new { tab = "C" });
                }
                else
                {
                    this.AddToastMessage("Error", success, ToastType.Error);
                    return RedirectToAction("EditCompany", new { companyID = co[0].CompanyID });
                }
            }

           
        }

        [HttpPost]
        public ActionResult GetParentCompanyLocation()
        {
            Company parentCompany = new Company(); 
            List<Company> companies = truckService.getCompanies(new Guid());

            foreach(Company c in companies)
            {
                if(c.isParent == true)
                {
                    parentCompany = c;
                }
            }

            return Json(parentCompany.CompanyCity + ", " + parentCompany.CompanyState, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddVehicleClass()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddVehicleClass(VehicleClass model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            VehicleClass nvc = new VehicleClass();
            nvc.VehicleClassID = Guid.NewGuid();
            nvc.VehicleClassName = model.VehicleClassName;
            nvc.VehicleClassDescription = model.VehicleClassDescription;
            nvc.VehicleClassImage = model.VehicleClassImage;

            truckService.updateVehicleClass(nvc, new Guid(sid));

            this.AddToastMessage("Vehicle Class Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "VC" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditVehicleClass(string VehicleClassID)
        {
            Guid vehicleClassID = new Guid(VehicleClassID);
            List<VehicleClass> vcs = truckService.getVehicleClasses();
            VehicleClasses vcToEdit = new VehicleClasses();
            foreach(VehicleClass vc in vcs)
            {
                if(vc.VehicleClassID == vehicleClassID)
                {
                    vcToEdit.VehicleClassID = vc.VehicleClassID;
                    vcToEdit.VehicleClassName = vc.VehicleClassName;
                    vcToEdit.VehicleClassDescription = vc.VehicleClassDescription;
                    vcToEdit.VehicleClassImage = vc.VehicleClassImage;
                }
            }

            if (vcToEdit == null)
            {
                this.AddToastMessage("Error: Vehicle Class does not exist", "The vehicle Class you are trying to edit does not seem to be in our database.", ToastType.Success);
                return View("Index", new { tab = "VC" });
            }
            else
            {
                return View(vcToEdit);
            }
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditVehicleClass(VehicleClasses model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            VehicleClass VCToEdit = new VehicleClass();
            VCToEdit.VehicleClassID = model.VehicleClassID;
            VCToEdit.VehicleClassName = model.VehicleClassName;
            VCToEdit.VehicleClassDescription = model.VehicleClassDescription;
            VCToEdit.VehicleClassImage = model.VehicleClassImage;

            truckService.updateVehicleClass(VCToEdit, new Guid(sid));

            this.AddToastMessage("Vehicle Class Modified", "", ToastType.Success);
            return RedirectToAction("Index", "Admin", new { tab = "VC" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteVehicleClass(string vehicleClassID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            List<VehicleClass> vcs = truckService.getVehicleClasses();
            VehicleClass vcToEdit = new VehicleClass();
            foreach (VehicleClass vc in vcs)
            {
                if (vc.VehicleClassID == new Guid(vehicleClassID))
                {
                    vcToEdit.VehicleClassID = vc.VehicleClassID;
                    vcToEdit.VehicleClassName = vc.VehicleClassName;
                    vcToEdit.VehicleClassDescription = vc.VehicleClassDescription;
                }
            }

            //delete class
            truckService.deleteVehicleClass(vcToEdit, new Guid(sid));

            this.AddToastMessage("Success", "Vehicle Class was deleted", ToastType.Success);
            return RedirectToAction("Index", new { tab = "VC" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddVehicle()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddVehicle(VehicleVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            VehicleExtendedData nv = new VehicleExtendedData();
            nv.ID = Guid.NewGuid();
            nv.vehicleID = model.VehicleIdentification;
            nv.companyID = new Guid(model.Company);
            nv.vehicleClassID = new Guid(model.VehicleClass);
            nv.haulLimit = model.HaulLimit;
            nv.licensePlate = model.LicensePlate;
            nv.Make = model.Make;
            nv.Model = model.VehicleModel;
            nv.Year = model.Year;
            nv.VehicleFriendlyName = model.VehicleFriendlyName;
            nv.MACAddress = model.VehicleMACAddress;
            nv.RouterID = model.RouterID;

            truckService.updateExtendedData(nv, new Guid(sid));

            this.AddToastMessage("Vehicle Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "V" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditVehicle(string VehicleID)
        {
            Guid vehicleID = new Guid(VehicleID);
            List<VehicleExtendedData> vcs = truckService.getExtendedData();
            VehicleVM vehicleToEdit = new VehicleVM();
            foreach (VehicleExtendedData vc in vcs)
            {
                if (vc.ID == vehicleID)
                {
                    vehicleToEdit.ID = vc.ID;
                    vehicleToEdit.VehicleIdentification = vc.vehicleID;
                    vehicleToEdit.CompanyID = vc.companyID;
                    vehicleToEdit.VehicleClassID = vc.vehicleClassID;
                    vehicleToEdit.HaulLimit = vc.haulLimit;
                    vehicleToEdit.LicensePlate = vc.licensePlate;
                    vehicleToEdit.Make = vc.Make;
                    vehicleToEdit.VehicleModel = vc.Model;
                    vehicleToEdit.Year = vc.Year;
                    vehicleToEdit.VehicleFriendlyName = vc.VehicleFriendlyName;
                    vehicleToEdit.VehicleMACAddress = vc.MACAddress;
                    vehicleToEdit.RouterID = vc.RouterID;
                }
            }

            return View(vehicleToEdit);

        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditVehicle(VehicleVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            VehicleExtendedData nv = new VehicleExtendedData();
            nv.ID = model.ID;
            nv.vehicleID = model.VehicleIdentification;
            nv.companyID = model.CompanyID;
            nv.vehicleClassID = model.VehicleClassID;
            nv.haulLimit = model.HaulLimit;
            nv.licensePlate = model.LicensePlate;
            nv.Make = model.Make;
            nv.Model = model.VehicleModel;
            nv.Year = model.Year;
            nv.VehicleFriendlyName = model.VehicleFriendlyName;
            nv.MACAddress = model.VehicleMACAddress;
            nv.RouterID = model.RouterID;

            truckService.updateExtendedData(nv, new Guid(sid));

            this.AddToastMessage("Vehicle Modified", "", ToastType.Success);           
            return RedirectToAction("Index", "Admin", new { tab = "V" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteVehicle(string VehicleID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            List<VehicleExtendedData> vehicles = truckService.getExtendedData();
            VehicleExtendedData vcToEdit = new VehicleExtendedData();
            foreach (VehicleExtendedData vc in vehicles)
            {
                if (vc.ID == new Guid(VehicleID))
                {
                    vcToEdit = vc;
                }
            }

            //delete class
            truckService.deleteExtendedData(vcToEdit, new Guid(sid));

            this.AddToastMessage("Success", "Vehicle was deleted", ToastType.Success);
            return RedirectToAction("Index", new { tab = "V" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddDriver()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddDriver(DriverVM model, HttpPostedFileBase fileUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Upload pic
            //string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/drivers/");

            if (fileUpload != null && fileUpload.ContentLength > 10)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var fullPath = Path.GetFullPath(fileUpload.FileName);
                //fileUpload.SaveAs(Path.Combine(directory, fileName));
                
                //Make VehiclesVM to lataservice.vehicles
                Driver nd = new Driver();
                nd.DriverID = Guid.NewGuid();
                nd.CompanyID = model.CompanyID;
                nd.DriverFirstName = model.DriverFirstName;
                nd.DriverLastName = model.DriverLastName;
                nd.DriverEmail = model.DriverEmail;
                nd.DriverNumber = model.DriverNumber;
                nd.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                nd.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                nd.ProfilePic = fileUpload.FileName;
                BinaryReader br = new BinaryReader(fileUpload.InputStream);
                nd.imageData = br.ReadBytes((int)fileUpload.InputStream.Length);
                nd.imageType = fileUpload.ContentType;
                truckService.updateDriver(nd, new Guid(sid));
            }
            
            this.AddToastMessage("Driver Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "D" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditDriver(string driverID)
        {
            List<Driver> drivers = truckService.getDrivers();
            DriverVM EditDriver = new DriverVM();
            foreach(Driver driver in drivers)
            {
                if(driver.DriverID == new Guid(driverID))
                {
                    EditDriver.DriverID = driver.DriverID;
                    EditDriver.DriverLastName = driver.DriverLastName;
                    EditDriver.DriverFirstName = driver.DriverFirstName;
                    EditDriver.CompanyID = driver.CompanyID;
                    EditDriver.DriverEmail = driver.DriverEmail;
                    EditDriver.DriverNumber = driver.DriverNumber;
                    EditDriver.DriverPassword = driver.DriverPassword;
                    EditDriver.VerifyDriverPassword = driver.DriverPassword;
                    EditDriver.PIN = driver.PIN;
                    //EditDriver.DriverPicture = driver.ProfilePic;
                    EditDriver.imageDataField = driver.imageData;
                    EditDriver.imageTypeField = driver.imageType;
                }
            }
            return View(EditDriver);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditDriver(DriverVM model, HttpPostedFileBase fileUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Upload pic
            //string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/drivers/");

            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var fullPath = Path.GetFullPath(fileUpload.FileName);
                //Make VehiclesVM to lataservice.vehicles
                Driver ed = new Driver();
                ed.DriverID = model.DriverID;
                ed.CompanyID = model.CompanyID;
                ed.DriverFirstName = model.DriverFirstName;
                ed.DriverLastName = model.DriverLastName;
                ed.DriverEmail = model.DriverEmail;
                ed.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                ed.DriverNumber = model.DriverNumber;
                ed.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                BinaryReader br = new BinaryReader(fileUpload.InputStream);
                ed.imageData = br.ReadBytes((int)fileUpload.InputStream.Length);
                ed.imageType = fileUpload.ContentType;
                ed.ProfilePic = "-";
                truckService.updateDriver(ed, new Guid(sid));
            }
            else
            {
                //Make VehiclesVM to lataservice.vehicles
                Driver ed = new Driver();
                ed.DriverID = model.DriverID;
                ed.CompanyID = model.CompanyID;
                ed.DriverFirstName = model.DriverFirstName;
                ed.DriverLastName = model.DriverLastName;
                ed.DriverEmail = model.DriverEmail;
                ed.DriverNumber = model.DriverNumber;
                ed.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                ed.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                ed.ProfilePic = "-";
                ed.imageData = model.imageDataField;
                ed.imageType = model.imageTypeField;
                truckService.updateDriver(ed, new Guid(sid));
            }          

            this.AddToastMessage("Driver Modified", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "D" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteDriver(string driverID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            List<Driver> drivers = truckService.getDrivers();
            Driver driverToEdit = new Driver();
            foreach (Driver dr in drivers)
            {
                if (dr.DriverID == new Guid(driverID))
                {
                    driverToEdit = dr;
                }
            }

            //delete class
            truckService.deleteDriver(driverToEdit, new Guid(sid));

            this.AddToastMessage("Success", "Driver was deleted", ToastType.Success);
            return RedirectToAction("Index", new { tab = "D" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddSetting()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddSetting(Vars model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            systemvar nv = new systemvar();
            nv.varID = Guid.NewGuid();
            nv.varName = model.varName;
            nv.varVal = model.varValue;
            nv.minValue = model.varMinValue;
            nv.varType = 1;

            truckService.updateAppVar(nv, new Guid(sid));

            this.AddToastMessage("App Setting Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "A" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditSetting(string settingID)
        {
            List<systemvar> vars = truckService.getAppVars();
            Vars AppSetting = new Vars();
            foreach (systemvar var in vars)
            {
                if (var.varID == new Guid(settingID))
                {
                    AppSetting.ID = var.varID;
                    AppSetting.varName = var.varName;
                    AppSetting.varValue = var.varVal;
                    AppSetting.varMinValue = var.minValue;
                }
            }
            return View(AppSetting);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditSetting(Vars model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            systemvar ev = new systemvar();
            ev.varID = model.ID;
            ev.varName = model.varName;
            ev.varVal = model.varValue;
            ev.minValue = model.varMinValue;
            ev.varType = 1;

            truckService.updateAppVar(ev, new Guid(sid));

            this.AddToastMessage("Application Setting Modified", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "A" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteSetting(string varID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            List<systemvar> settings = truckService.getAppVars();
            systemvar varToEdit = new systemvar();
            foreach (systemvar v in settings)
            {
                if (v.varID == new Guid(varID))
                {
                    varToEdit = v;
                }
            }

            //delete class
            truckService.deleteAppVar(varToEdit, new Guid(sid));

            this.AddToastMessage("Success", "Application Setting was deleted", ToastType.Success);
            return RedirectToAction("Index", new { tab = "A" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddServiceVar()
        {
            return View();
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddServiceVar(Vars model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            systemvar nv = new systemvar();
            nv.varID = Guid.NewGuid();
            nv.varName = model.varName;
            nv.varVal = model.varValue;
            nv.minValue = model.varMinValue;
            nv.varType = 0;
            nv.Email = model.email;

            truckService.updateVar(nv, new Guid(sid));

            this.AddToastMessage("Service Variable Added", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "VAR" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditServiceVar(string varID)
        {
            List<systemvar> vars = truckService.getServiceVars();
            Vars VarSetting = new Vars();
            foreach (systemvar var in vars)
            {
                if (var.varID == new Guid(varID))
                {
                    VarSetting.ID = var.varID;
                    VarSetting.varName = var.varName;
                    VarSetting.varValue = var.varVal;
                    VarSetting.varMinValue = var.minValue;
                    VarSetting.email = var.Email;
                }
            }
            return View(VarSetting);
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditServiceVar(Vars model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            systemvar ev = new systemvar();
            ev.varID = model.ID;
            ev.varName = model.varName;
            ev.varVal = model.varValue;
            ev.minValue = model.varMinValue;
            ev.Email = model.email;
            ev.varType = 0;

            truckService.updateServiceVar(ev, new Guid(sid));

            this.AddToastMessage("Service Variable Modified", "", ToastType.Success);
            return RedirectToAction("Index", new { tab = "VAR" });
        }

        [CheckSessionOut]
        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteServiceVar(string varID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            List<systemvar> settings = truckService.getServiceVars();
            systemvar varToEdit = new systemvar();
            foreach (systemvar v in settings)
            {
                if (v.varID == new Guid(varID))
                {
                    varToEdit = v;
                }
            }

            //delete class
            truckService.deleteServiceVar(varToEdit, new Guid(sid));

            this.AddToastMessage("Success", "Service Variable was deleted", ToastType.Success);
            return RedirectToAction("Index", new { tab = "VAR" });
        }
    }
}