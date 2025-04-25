using System;
using System.Collections.Generic;

namespace Volunteer_website.Data;

public partial class Admin
{
    public string AdminId { get; set; } = null!;

    public string? Name { get; set; }

    public string? ImagePath { get; set; }

    public virtual User AdminNavigation { get; set; } = null!;
}
