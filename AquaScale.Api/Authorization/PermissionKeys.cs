namespace AquaScale.Api.Authorization;

public static class PermissionKeys
{   
    // Examples
    public const string EmployeesCreate = "employees.create";
    public const string PaymentsVerify = "payments.verify";
    public const string PropertiesView = "properties.view";

    // add here as new permissions get built — one place, not scattered

    //ROLES 
    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";
}