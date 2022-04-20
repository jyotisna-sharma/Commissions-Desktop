using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.ViewModel.CommonItems
{

    public static class RoleManager
    {
        public static string LoggedInUser { get; set; }
        public static Guid  userCredentialID {get;set;}
        public static MyAgencyVault.VM.MyAgencyVaultSvc.UserRole Role { get; set; }
        public static bool IsHouseAccount { get; set; }
        public static Guid? LicenseeId { get; set; }
        public static Guid? HouseOwnerId { get; set; }
        public static Guid? AdminId { get; set; }
        public static bool IsNewsToFlash { get; set; }
        public static string LicenseName { get; set; }
        public static List<UserPermissions> UserPermissions { get; set; }
        public static string WebDavPath;
        //Added 09062014
        public static bool? IsEditDisable { get; set; }

        public static ModuleAccessRight UserAccessPermission(MasterModule module)
        {
            if (Role == UserRole.SuperAdmin)
                return ModuleAccessRight.Write;

            if (Role == UserRole.DEP)
            {
                if (module == MasterModule.CompManager)
                    return ModuleAccessRight.Read;
                else if (module == MasterModule.HelpUpdate)
                    return ModuleAccessRight.Write;
            }

            return UserPermissions[((int)module - 1)].Permission;
        }
    }
}
