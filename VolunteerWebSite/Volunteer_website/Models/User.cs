using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? Role { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Volunteer? Volunteer { get; set; }
}
