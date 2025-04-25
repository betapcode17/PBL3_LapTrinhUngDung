using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class Account
{
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public virtual User User { get; set; } = null!;
}
