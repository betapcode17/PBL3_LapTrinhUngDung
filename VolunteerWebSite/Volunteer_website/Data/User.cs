using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class User
{
    public string Userid { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public int? Role { get; set; }

    public string? RandomKey { get; set; }

    public bool? IsActive { get; set; }
}
