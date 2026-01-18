using Microsoft.AspNetCore.Identity;

namespace MVC.Models;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public int? ImageUploadedId { get; set; }
    public ImageUploaded? Avatar { get; set; }
}