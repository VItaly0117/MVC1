namespace MVC.Models.Forms;

public class UserRolesForm
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<UserRoleItem> Roles { get; set; } = new();
}

public class UserRoleItem
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}