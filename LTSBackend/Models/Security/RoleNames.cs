using LTSBackend.Comman.Enum;

namespace LTSBackend.Models.Security;

public static class RoleNames
{
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);
    public const string FirmAdmin = nameof(UserRole.FirmAdmin);
    public const string Partner = nameof(UserRole.Partner);
    public const string AssociateLawyer = nameof(UserRole.AssociateLawyer);
    public const string Moharrir = nameof(UserRole.Moharrir);
    public const string InternParalegal = nameof(UserRole.InternParalegal);






    public const string SuperAdminOnly = SuperAdmin;









    public const string FirmAdminOnly = FirmAdmin;














    public const string FirmAdminAndAbove = FirmAdmin + "," + Partner;






    public const string PartnerAndAbove = Partner + "," + FirmAdmin;




    public const string AllLawyers = FirmAdmin + "," + Partner + "," + AssociateLawyer + "," + Moharrir;




    public const string AllFirmUsers = FirmAdmin + "," + Partner + "," + AssociateLawyer + "," +
                                      Moharrir + "," + InternParalegal;









    public const string AllFirmUsersAndSuperAdmin = AllFirmUsers;




    public const string CanViewDocuments = Partner + "," + AssociateLawyer + "," + Moharrir;





    public const string CanCreateCases = Partner + "," + FirmAdmin;





    public const string CanAssignCases = Partner + "," + FirmAdmin;
}