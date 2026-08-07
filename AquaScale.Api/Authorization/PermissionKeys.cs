namespace AquaScale.Api.Authorization;

public static class PermissionKeys
{   
    // [ADMINISTRATION]
    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";
    public const string EmployeesCreate = "employees.create";

    // [CUSTOMERS]
    public const string CustomersView = "customers.view";
    public const string PropertiesView = "properties.view";

    // [FIELD OPERATIONS]
    public const string MeterReadingsCreate = "meter_readings.create";

    // [ACCOUNTING]
    public const string PaymentsVerify = "payments.verify";

    // [SERVICE REQUESTS]
    public const string ServiceRequestsManage = "service_requests.manage";
}