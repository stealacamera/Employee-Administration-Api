namespace EmployeeAdministration.Domain.Enums;

public enum Roles : ushort
{
    Administrator = 1,
    Employee = 2
}

//public sealed class Roles : SmartEnum<Roles, ushort>
//{
//    public static readonly Roles Admin = new("Administrator", 1);
//    public static readonly Roles Employee = new("Employee", 2);

//    private Roles(string name, ushort value) : base(name, value) { }
//}
