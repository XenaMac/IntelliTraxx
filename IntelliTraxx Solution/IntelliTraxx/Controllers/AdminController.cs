using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using IntelliTraxx.Common;
using IntelliTraxx.Shared.Identity;
using IntelliTraxx.TruckService;

namespace IntelliTraxx.Controllers
{
    public class AdminController : Controller
    {
        private readonly TruckServiceClient truckService = new TruckServiceClient();

        public ActionResult Index(string tab)
        {
            ViewBag.Tab = tab;
            var admin = new Admin();
            admin.Users = new List<ITUser>();

            //Get Users and apply to ITUsers
            var serviceUsers = truckService.getUsers(new Guid()).OrderBy(u => u.UserLastName).ToList();
            foreach (var u in serviceUsers)
            {
                var itu = new ITUser();
                itu.user = u;
                itu.Roles = truckService.getUserRolesFull(u.UserID);
                itu.Companies = truckService.getUserCompaniesFull(u.UserID);
                admin.Users.Add(itu);
            }

            //Get Roles
            admin.Roles = truckService.getRoles(new Guid()).OrderBy(x => x.roleName).ToList();

            //Get Companies
            admin.Companies = truckService.getCompanies(new Guid()).OrderBy(c => c.CompanyName).ToList();

            //Get Vehicle Classes
            admin.VehicleClasses = truckService.getVehicleClasses().OrderBy(vc => vc.VehicleClassName).ToList();

            #region get vehicles

            var VEDS = truckService.getAllVehicles(true);
            admin.Vehicles = new List<VehicleVM>();
            foreach (var ved in VEDS)
            {
                var nv = new VehicleVM();
                nv.CompanyID = ved.extendedData.companyID;
                var vcompany = truckService.getCompanies(ved.extendedData.companyID);
                nv.Company = vcompany[0].CompanyName;
                nv.VehicleClassID = ved.extendedData.vehicleClassID;
                var vclass = truckService.getVehicleClasses();
                foreach (var vc in vclass)
                    if (vc.VehicleClassID == nv.VehicleClassID)
                        nv.VehicleClass = vc.VehicleClassName;
                nv.HaulLimit = ved.extendedData.haulLimit;
                nv.ID = ved.extendedData.ID;
                nv.VehicleIdentification = ved.extendedData.vehicleID;
                nv.LicensePlate = ved.extendedData.licensePlate;
                nv.Make = ved.extendedData.Make;
                nv.VehicleModel = ved.extendedData.Model;
                nv.Year = ved.extendedData.Year;
                nv.VehicleFriendlyName = ved.extendedData.VehicleFriendlyName;
                admin.Vehicles.Add(nv);
            }

            #endregion

            #region get drivers

            var truckDrivers = truckService.getDrivers();
            admin.Drivers = new List<DriverVM>();
            foreach (var d in truckDrivers)
            {
                var driver = new DriverVM();
                driver.DriverID = d.DriverID;
                driver.CompanyID = d.CompanyID;
                var company = truckService.getCompanies(driver.CompanyID);
                driver.CompanyName = company[0].CompanyName;
                driver.DriverFirstName = d.DriverFirstName;
                driver.DriverEmail = d.DriverEmail;
                driver.DriverNumber = d.DriverNumber;
                driver.DriverLastName = d.DriverLastName;
                driver.DriverPassword = d.DriverPassword;
                driver.PIN = d.PIN;
                driver.DriverPicture = "~/Images/" + d.ProfilePic;
                driver.imageDataField = d.imageData;
                driver.imageTypeField = d.imageType;
                admin.Drivers.Add(driver);
            }

            #endregion

            #region Get App Variables

            var AppVariables = truckService.getAppVars();
            admin.AppVariables = new List<Vars>();
            foreach (var var in AppVariables)
            {
                var v = new Vars();
                v.ID = var.varID;
                v.varName = var.varName;
                v.varValue = var.varVal;
                admin.AppVariables.Add(v);
            }

            #endregion

            #region Get Service Variables

            var SvcVariables = truckService.getServiceVars();
            admin.ServiceVariables = new List<Vars>();
            foreach (var var in SvcVariables)
            {
                var v = new Vars();
                v.ID = var.varID;
                v.varName = var.varName;
                v.varValue = var.varVal;
                v.varMinValue = var.minValue;
                v.email = var.Email;
                admin.ServiceVariables.Add(v);
            }

            #endregion

            #region Vehicles->to->Drivers

            admin.availableDrivers = truckService.getAvailableDrivers();
            admin.availableVehicles = truckService.getAvailableVehicles();
            admin.DriversToVehicles = truckService.driverVehicleReturn();

            #endregion

            return View(admin);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddUser()
        {
            ViewBag.Roles = truckService.getRoles(new Guid());
            ViewBag.Companies = truckService.getCompanies(new Guid());

            return View();
        }

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
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            var u = new User();
            u.UserID = Guid.NewGuid();
            u.UserEmail = model.UserEmail;
            u.UserFirstName = model.UserFirstName;
            u.UserLastName = model.UserLastName;
            u.UserOffice = model.UserOffice;
            u.UserPhone = model.UserPhone;
            u.UserPassword = model.UserPassword;

            var success = truckService.setUser(u, new Guid(sid));
            if (success == "OK")
            {
                foreach (var selection in Companies)
                {
                    var success2 = truckService.addUserToCompany(u.UserID, new Guid(selection), new Guid(sid));
                }

                foreach (var roleSelection in Roles)
                {
                    var success3 = truckService.addUserToRole(u.UserID, new Guid(roleSelection), new Guid(sid));
                }
            }
            else
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
            }

            return Redirect("Index");
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditUser(string userID)
        {
            ViewBag.Roles = truckService.getRoles(new Guid());
            ViewBag.Companies = truckService.getCompanies(new Guid());

            var user = truckService.getUserProfile(new Guid(userID));
            var EditUser = new EditUser();
            EditUser.UserID = user.UserID;
            EditUser.UserLastName = user.UserLastName;
            EditUser.UserFirstName = user.UserFirstName;
            EditUser.UserEmail = user.UserEmail;
            EditUser.UserOffice = user.UserOffice;
            EditUser.UserPhone = user.UserPhone;
            EditUser.UserPassword = user.UserPassword;
            EditUser.UserSalt = user.UserSalt;
            EditUser.VerifyUserPassword = user.UserPassword;
            var _authMgmt = new AuthMgmt();
            var roleNames = _authMgmt.GetUserRoles(user.UserID);
            foreach (var s in roleNames) EditUser.Roles += s + "|";
            var IntelliTruxxUserCompanies = truckService.getUserCompaniesFull(EditUser.UserID);
            foreach (var c in IntelliTruxxUserCompanies) EditUser.Companies += c.CompanyName + "|";

            return View(EditUser);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditUser(EditUser model, string[] Companies, string[] Roles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                var _authMgmt = new AuthMgmt();
                var roleNames = _authMgmt.GetUserRoles(model.UserID);
                foreach (var s in roleNames) model.Roles += s + "|";
                var IntelliTruxxUserCompanies = truckService.getUserCompaniesFull(model.UserID);
                foreach (var c in IntelliTruxxUserCompanies) model.Companies += c.CompanyName + "|";
                return View(model);
            }

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Grab current edited user to see if password has changed
            var eu = truckService.getUserProfile(model.UserID);

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

            var success = truckService.setUser(eu, new Guid(sid));

            #region blow away user roles and companies

            //blow away all roles with userid
            var UserRoles = truckService.getUserRolesGuids(eu.UserID);
            foreach (var roleID in UserRoles)
                success = truckService.removeUserFromRole(eu.UserID, roleID, new Guid(sid));

            //blow away all roles with userid
            var UserCompanies = truckService.getUserCompanies(eu.UserID);
            foreach (var companyID in UserCompanies)
                success = truckService.removeUserFromCompany(eu.UserID, companyID, new Guid(sid));

            #endregion

            #region add user roles and companies

            foreach (var roleSelection in Roles)
                success = truckService.addUserToRole(eu.UserID, new Guid(roleSelection), new Guid(sid));

            foreach (var selection in Companies)
                success = truckService.addUserToCompany(eu.UserID, new Guid(selection), new Guid(sid));

            #endregion

            if (success != "OK")
            {
                ViewBag.Roles = truckService.getRoles(new Guid());
                ViewBag.Companies = truckService.getCompanies(new Guid());
                ViewBag.UserRoles = truckService.getUserRolesFull(model.UserID);
                ViewBag.UserCompanies = truckService.getUserCompaniesFull(model.UserID);
            }

            return Redirect("Index");
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteUser(string userID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get user
            var u = truckService.getUserProfile(new Guid(userID));

            var success = truckService.deleteUser(u, new Guid(sid));

            if (success == "OK")
                return Redirect("Index");
            return RedirectToAction("EditUser", new {userid = userID});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddRole()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddRole(Roles model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            var r = new Role();
            r.RoleID = Guid.NewGuid();
            r.roleName = model.RoleName;
            r.roleDescription = model.RoleDescription;
            if (model.IsAdmin == true)
                r.isAdmin = true;
            else
                r.isAdmin = false;

            var success = truckService.setRole(r, new Guid(sid));

            if (success != "OK") return View(model);

            //return RedirectToAction("/Index", new { tab = "R" });
            return View(model);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditRole(string roleID)
        {
            var RoleID = new Guid(roleID);
            var Role = truckService.getRoles(RoleID);
            var role = new Roles();
            role.RoleID = Role[0].RoleID;
            role.RoleName = Role[0].roleName;
            role.RoleDescription = Role[0].roleDescription;
            role.IsAdmin = Role[0].isAdmin;
            if (Role[0] == null)
                return View("/Index", new Admin());
            return View(role);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteRole(string roleID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get role
            var r = truckService.getRoles(new Guid(roleID));

            var success = truckService.deleteRole(r[0], new Guid(sid));

            if (success == "OK")
            {
                //return RedirectToAction("/Index", new { tab = "R" });
            }
            else
            {
                return RedirectToAction("EditRole", new {roleID = r[0].RoleID});
            }

            return View(roleID);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddCompany()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddCompany(Companies model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            var company = new Company();
            company.CompanyID = Guid.NewGuid();
            company.CompanyName = model.CompanyName;
            company.CompanyAddress = model.CompanyAddress;
            company.CompanyCity = model.CompanyCity;
            company.CompanyState = model.CompanyState;
            company.CompanyCountry = model.CompanyCountry;
            company.isParent = false;
            company.CompanyContact = truckService.getUserProfile(model.Contact);

            var success = truckService.setCompany(company, new Guid(sid));

            if (success != "OK") return View(model);

            //return RedirectToAction("/Index", new { tab = "C" });
            return View(model);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditCompany(string companyID)
        {
            var CompanyID = new Guid(companyID);
            var Co = truckService.getCompanies(CompanyID);
            var company = new Companies();
            company.CompanyID = Co[0].CompanyID;
            company.CompanyName = Co[0].CompanyName;
            company.CompanyAddress = Co[0].CompanyAddress;
            company.CompanyCity = Co[0].CompanyCity;
            company.CompanyState = Co[0].CompanyState;
            company.CompanyCountry = Co[0].CompanyCountry;
            company.IsParent = Co[0].isParent;
            company.Contact = Co[0].CompanyContact.UserID;

            if (Co[0] == null)
                return View();
            return View(company);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditCompany(Companies model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            var company = new Company
            {
                CompanyID = model.CompanyID,
                CompanyName = model.CompanyName,
                CompanyAddress = model.CompanyAddress,
                CompanyCity = model.CompanyCity,
                CompanyState = model.CompanyState,
                CompanyCountry = model.CompanyCountry,
                isParent = model.IsParent,
                CompanyContact = truckService.getUserProfile(model.Contact)
            };

            var success = truckService.setCompany(company, new Guid(sid));

            if (success != "OK") return View(model);

            //return RedirectToAction("/Index", "Admin", new { tab = "C" });
            return View(model);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteCompany(string companyID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get role
            var co = truckService.getCompanies(new Guid(companyID));

            if (co[0].isParent) return RedirectToAction("EditCompany", new {tab = "C"});

            var success = truckService.deleteCompany(co[0], new Guid(sid));

            if (success == "OK")
                return RedirectToAction("EditCompany", new {tab = "C"});
            return RedirectToAction("EditCompany", new {companyID = co[0].CompanyID});
        }

        [HttpPost]
        public ActionResult GetParentCompanyLocation()
        {
            var parentCompany = new Company();
            var companies = truckService.getCompanies(new Guid());

            foreach (var c in companies)
                if (c.isParent)
                    parentCompany = c;

            return Json(parentCompany.CompanyCity + ", " + parentCompany.CompanyState, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getUserProfile()
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var user = truckService.getUserProfile(new Guid(sid));
            var roles = truckService.getUserRolesFull(new Guid(sid));
            return Json(new
            {
                user.UserEmail,
                user.UserFirstName,
                user.UserLastName,
                user.UserID,
                user.UserOffice,
                user.UserPhone,
                UserRole = roles[0].roleName
            }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddVehicleClass()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddVehicleClass(VehicleClass model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get Roles to Role
            var nvc = new VehicleClass
            {
                VehicleClassID = Guid.NewGuid(),
                VehicleClassName = model.VehicleClassName,
                VehicleClassDescription = model.VehicleClassDescription,
                VehicleClassImage = model.VehicleClassImage
            };

            truckService.updateVehicleClass(nvc, new Guid(sid));

            //return RedirectToAction("/Index", new { tab = "VC" });
            return View(model);
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditVehicleClass(string VehicleClassID)
        {
            var vehicleClassID = new Guid(VehicleClassID);
            var vcs = truckService.getVehicleClasses();
            var vcToEdit = new VehicleClasses();
            foreach (var vc in vcs)
                if (vc.VehicleClassID == vehicleClassID)
                {
                    vcToEdit.VehicleClassID = vc.VehicleClassID;
                    vcToEdit.VehicleClassName = vc.VehicleClassName;
                    vcToEdit.VehicleClassDescription = vc.VehicleClassDescription;
                    vcToEdit.VehicleClassImage = vc.VehicleClassImage;
                }

            if (vcToEdit == null)
                return View("/Index", new {tab = "VC"});
            return View(vcToEdit);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditVehicleClass(VehicleClasses model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            var VCToEdit = new VehicleClass();
            VCToEdit.VehicleClassID = model.VehicleClassID;
            VCToEdit.VehicleClassName = model.VehicleClassName;
            VCToEdit.VehicleClassDescription = model.VehicleClassDescription;
            VCToEdit.VehicleClassImage = model.VehicleClassImage;

            truckService.updateVehicleClass(VCToEdit, new Guid(sid));

            return RedirectToAction("/Index", "Admin", new {tab = "VC"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteVehicleClass(string vehicleClassID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            var vcs = truckService.getVehicleClasses();
            var vcToEdit = new VehicleClass();
            foreach (var vc in vcs)
                if (vc.VehicleClassID == new Guid(vehicleClassID))
                {
                    vcToEdit.VehicleClassID = vc.VehicleClassID;
                    vcToEdit.VehicleClassName = vc.VehicleClassName;
                    vcToEdit.VehicleClassDescription = vc.VehicleClassDescription;
                }

            //delete class
            truckService.deleteVehicleClass(vcToEdit, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "VC"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddVehicle()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddVehicle(VehicleVM model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var nv = new VehicleExtendedData();
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

            return RedirectToAction("/Index", new {tab = "V"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditVehicle(string VehicleID)
        {
            var id = new Guid(VehicleID);
            var vehicle = truckService.getVehicleData(id);

            var vehicleToEdit = new VehicleVM();
            vehicleToEdit.ID = vehicle.extendedData.ID;
            vehicleToEdit.VehicleIdentification = vehicle.extendedData.vehicleID;
            vehicleToEdit.CompanyID = vehicle.extendedData.companyID;
            vehicleToEdit.VehicleClassID = vehicle.extendedData.vehicleClassID;
            vehicleToEdit.HaulLimit = vehicle.extendedData.haulLimit;
            vehicleToEdit.LicensePlate = vehicle.extendedData.licensePlate;
            vehicleToEdit.Make = vehicle.extendedData.Make;
            vehicleToEdit.VehicleModel = vehicle.extendedData.Model;
            vehicleToEdit.Year = vehicle.extendedData.Year;
            vehicleToEdit.VehicleFriendlyName = vehicle.extendedData.VehicleFriendlyName;
            vehicleToEdit.VehicleMACAddress = vehicle.extendedData.MACAddress;
            vehicleToEdit.RouterID = vehicle.extendedData.RouterID;

            return View(vehicleToEdit);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditVehicle(VehicleVM model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var nv = new VehicleExtendedData();
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

            return RedirectToAction("/Index", "Admin", new {tab = "V"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteVehicle(string VehicleID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            var vehicles = truckService.getExtendedData();
            var vcToEdit = new VehicleExtendedData();
            foreach (var vc in vehicles)
                if (vc.ID == new Guid(VehicleID))
                    vcToEdit = vc;

            //delete class
            truckService.deleteExtendedData(vcToEdit, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "V"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddDriver()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddDriver(DriverVM model, HttpPostedFileBase fileUpload)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Upload pic
            //string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/drivers/");

            if (fileUpload != null && fileUpload.ContentLength > 10)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var fullPath = Path.GetFullPath(fileUpload.FileName);
                //fileUpload.SaveAs(Path.Combine(directory, fileName));

                //Make VehiclesVM to lataservice.vehicles
                var nd = new Driver();
                nd.DriverID = Guid.NewGuid();
                nd.CompanyID = model.CompanyID;
                nd.DriverFirstName = model.DriverFirstName;
                nd.DriverLastName = model.DriverLastName;
                nd.DriverEmail = model.DriverEmail;
                nd.DriverNumber = model.DriverNumber;
                nd.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                nd.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                nd.ProfilePic = fileUpload.FileName;
                var br = new BinaryReader(fileUpload.InputStream);
                nd.imageData = br.ReadBytes((int) fileUpload.InputStream.Length);
                nd.imageType = fileUpload.ContentType;
                truckService.updateDriver(nd, new Guid(sid));
            }
            else
            {
                var img = Image.FromFile(Server.MapPath("~/Content/Images/defaultDriver.png"));
                byte[] arr;
                var converter = new ImageConverter();
                arr = (byte[]) converter.ConvertTo(img, typeof(byte[]));

                //Make VehiclesVM to lataservice.vehicles
                var nd = new Driver();
                nd.DriverID = Guid.NewGuid();
                nd.CompanyID = model.CompanyID;
                nd.DriverFirstName = model.DriverFirstName;
                nd.DriverLastName = model.DriverLastName;
                nd.DriverEmail = model.DriverEmail;
                nd.DriverNumber = model.DriverNumber;
                nd.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                nd.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                nd.ProfilePic = "defaulDriver.png";
                nd.imageData = arr;
                nd.imageType = "png";
                truckService.updateDriver(nd, new Guid(sid));
            }

            return RedirectToAction("/Index", new {tab = "D"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditDriver(string driverID)
        {
            var drivers = truckService.getDrivers();
            var EditDriver = new DriverVM();
            foreach (var driver in drivers)
                if (driver.DriverID == new Guid(driverID))
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

            return View(EditDriver);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditDriver(DriverVM model, HttpPostedFileBase fileUpload)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Upload pic
            //string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/drivers/");

            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var fullPath = Path.GetFullPath(fileUpload.FileName);
                //Make VehiclesVM to lataservice.vehicles
                var ed = new Driver();
                ed.DriverID = model.DriverID;
                ed.CompanyID = model.CompanyID;
                ed.DriverFirstName = model.DriverFirstName;
                ed.DriverLastName = model.DriverLastName;
                ed.DriverEmail = model.DriverEmail;
                ed.PIN = Convert.ToInt32(model.PIN).ToString("0000");
                ed.DriverNumber = model.DriverNumber;
                ed.DriverPassword = Convert.ToInt32(model.PIN).ToString("0000");
                var br = new BinaryReader(fileUpload.InputStream);
                ed.imageData = br.ReadBytes((int) fileUpload.InputStream.Length);
                ed.imageType = fileUpload.ContentType;
                ed.ProfilePic = "-";
                truckService.updateDriver(ed, new Guid(sid));
            }
            else
            {
                //Make VehiclesVM to lataservice.vehicles
                var ed = new Driver();
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

            return RedirectToAction("/Index", new {tab = "D"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteDriver(string driverID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            var drivers = truckService.getDrivers();
            var driverToEdit = new Driver();
            foreach (var dr in drivers)
                if (dr.DriverID == new Guid(driverID))
                    driverToEdit = dr;

            //delete class
            truckService.deleteDriver(driverToEdit, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "D"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddSetting()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddSetting(Vars model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var email = identity.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var nv = new systemvar();
            nv.varID = Guid.NewGuid();
            nv.varName = model.varName;
            nv.varVal = model.varValue;
            nv.minValue = model.varMinValue;
            nv.varType = 1;
            nv.Email = email;

            truckService.updateAppVar(nv, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "A"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditSetting(string settingID)
        {
            var vars = truckService.getAppVars();
            var AppSetting = new Vars();
            foreach (var var in vars)
                if (var.varID == new Guid(settingID))
                {
                    AppSetting.ID = var.varID;
                    AppSetting.varName = var.varName;
                    AppSetting.varValue = var.varVal;
                    AppSetting.varMinValue = var.minValue;
                }

            return View(AppSetting);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditSetting(Vars model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var email = identity.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var ev = new systemvar();
            ev.varID = model.ID;
            ev.varName = model.varName;
            ev.varVal = model.varValue;
            ev.minValue = model.varMinValue;
            ev.varType = 1;
            ev.Email = email;

            truckService.updateAppVar(ev, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "A"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteSetting(string varID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            var settings = truckService.getAppVars();
            var varToEdit = new systemvar();
            foreach (var v in settings)
                if (v.varID == new Guid(varID))
                    varToEdit = v;

            //delete class
            truckService.deleteAppVar(varToEdit, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "A"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult AddServiceVar()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult AddServiceVar(Vars model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var email = identity.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var nv = new systemvar();
            nv.varID = Guid.NewGuid();
            nv.varName = model.varName;
            nv.varVal = model.varValue;
            nv.minValue = model.varMinValue;
            nv.varType = 0;
            nv.Email = model.email;

            truckService.updateVar(nv, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "VAR"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult EditServiceVar(string varID)
        {
            var vars = truckService.getServiceVars();
            var VarSetting = new Vars();
            foreach (var var in vars)
                if (var.varID == new Guid(varID))
                {
                    VarSetting.ID = var.varID;
                    VarSetting.varName = var.varName;
                    VarSetting.varValue = var.varVal;
                    VarSetting.varMinValue = var.minValue;
                    VarSetting.email = var.Email;
                }

            return View(VarSetting);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult EditServiceVar(Vars model)
        {
            if (!ModelState.IsValid) return View(model);

            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //Make VehiclesVM to lataservice.vehicles
            var ev = new systemvar();
            ev.varID = model.ID;
            ev.varName = model.varName;
            ev.varVal = model.varValue;
            ev.minValue = model.varMinValue;
            ev.Email = model.email;
            ev.varType = 0;

            truckService.updateServiceVar(ev, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "VAR"});
        }

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult DeleteServiceVar(string varID)
        {
            //get current identity and claims
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            //get vehicleclasstoedit
            var settings = truckService.getServiceVars();
            var varToEdit = new systemvar();
            foreach (var v in settings)
                if (v.varID == new Guid(varID))
                    varToEdit = v;

            //delete class
            truckService.deleteServiceVar(varToEdit, new Guid(sid));

            return RedirectToAction("/Index", new {tab = "VAR"});
        }


        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult deleteVehicleDriver(string ID)
        {
            //deleteVehicleDriver
            truckService.deleteVehicleDriver(ID);

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }


    //----------------------------CLASSES-----------------------------------------------//
    public class ITUser
    {
        public User user { get; set; }
        public List<Role> Roles { get; set; }
        public List<Company> Companies { get; set; }
    }

    public class Admin
    {
        public List<Role> Roles { get; set; }
        public List<Company> Companies { get; set; }
        public List<ITUser> Users { get; set; }
        public List<VehicleClass> VehicleClasses { get; set; }
        public List<VehicleVM> Vehicles { get; set; }
        public List<DriverVM> Drivers { get; set; }
        public List<Vars> AppVariables { get; set; }
        public List<Vars> ServiceVariables { get; set; }
        public List<Driver> availableDrivers { get; set; }
        public List<driverVehicleReturn> DriversToVehicles { get; set; }
        public List<VehicleExtendedData> availableVehicles { get; set; }
    }

    public class VehicleVM
    {
        public Guid ID { get; set; }

        [Required]
        [DisplayName("Vehicle ID:")]
        [DataType(DataType.Text)]
        public string VehicleIdentification { get; set; }

        [DisplayName("Vehicle Class:")] public string VehicleClass { get; set; }

        public Guid VehicleClassID { get; set; }

        [DisplayName("Company:")] public string Company { get; set; }

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

    public class DriverVM
    {
        public Guid DriverID { get; set; }

        [Required]
        [DisplayName("Last Name:")]
        [DataType(DataType.Text)]
        public string DriverLastName { get; set; }

        [Required]
        [DisplayName("First Name:")]
        [DataType(DataType.Text)]
        public string DriverFirstName { get; set; }

        [Required]
        [DisplayName("Email:")]
        [DataType(DataType.EmailAddress)]
        public string DriverEmail { get; set; }

        [Required]
        [DisplayName("Number:")]
        [DataType(DataType.Text)]
        public string DriverNumber { get; set; }

        [DisplayName("Password;")]
        [DataType(DataType.Password)]
        public string DriverPassword { get; set; }

        [DisplayName("Verify Password;")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("DriverPassword", ErrorMessage =
            "Confirm password doesn't match, Type again!")]
        public string VerifyDriverPassword { get; set; }

        [DisplayName("PIN:")]
        [StringLength(4, ErrorMessage = "The PIN cannot exceed 4 characters. ")]
        public string PIN { get; set; }

        [Required] [DisplayName("Company:")] public Guid CompanyID { get; set; }

        [DisplayName("Company:")] public string CompanyName { get; set; }

        [DisplayName("Picture:")]
        [DataType(DataType.ImageUrl)]
        public string DriverPicture { get; set; }

        [DisplayName("Picture:")] public byte[] imageDataField { get; set; }

        [DisplayName("Picture String:")] public string image64String { get; set; }

        [DisplayName("Picture Type:")] public string imageTypeField { get; set; }
    }

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

        [DisplayName("Email (For Alerts):")]
        [DataType(DataType.Text)]
        public string email { get; set; }
    }

    public class AddUser
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

        [DataType(DataType.Text)] public string UserOffice { get; set; }

        [DataType(DataType.PhoneNumber)] public string UserPhone { get; set; }

        [Required]
        [DisplayName("Password:")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; }

        [Required]
        [DisplayName("Verify Password:")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("UserPassword", ErrorMessage =
            "Confirm password doesn't match, Type again !")]
        public string VerifyUserPassword { get; set; }

        public List<string> Companies { get; set; }
        public List<string> Roles { get; set; }
    }

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

        [DataType(DataType.Text)] public string UserOffice { get; set; }

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
        [System.ComponentModel.DataAnnotations.Compare("UserPassword", ErrorMessage =
            "Confirm password doesn't match, Type again !")]
        public string VerifyUserPassword { get; set; }

        [DataType(DataType.Password)] public string UserSalt { get; set; }

        public string Roles { get; set; }
        public string Companies { get; set; }
    }

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

        [DisplayName("Is Admin:")] public bool? IsAdmin { get; set; }
    }

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

        [DisplayName("Parent Company:")] public bool IsParent { get; set; }

        [DisplayName("Company Contact:")] public Guid Contact { get; set; }
    }

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