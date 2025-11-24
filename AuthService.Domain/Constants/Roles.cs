namespace AuthService.Domain.Constants;

public static class Roles
{
    // Super Admin - Has access to everything
    public const string SuperAdmin = "SuperAdmin";
    
    // Admin Roles - Full CRUD within their department
    public const string Admin = "Admin";
    public const string FinanceAdmin = "FinanceAdmin";
    public const string HRAdmin = "HRAdmin";
    public const string ITAdmin = "ITAdmin";
    
    // Manager Roles - Full CRUD except Delete within their department
    public const string Manager = "Manager";
    public const string FinanceManager = "FinanceManager";
    public const string HRManager = "HRManager";
    public const string ITManager = "ITManager";
    
    // Analyst/Executive Roles - View + Create only (no Edit/Delete)
    public const string Analyst = "Analyst";
    public const string Executive = "Executive";
    public const string FinanceAnalyst = "FinanceAnalyst";
    public const string HRAnalyst = "HRAnalyst";
    
    // Staff Roles - View only
    public const string Staff = "Staff";
    public const string FinanceStaff = "FinanceStaff";
    public const string HRStaff = "HRStaff";
    
    // Legacy/Other Roles
    public const string User = "User";
    public const string PendingUser = "PendingUser";
    public const string Accountant = "Accountant";
    public const string Auditor = "Auditor";
}
