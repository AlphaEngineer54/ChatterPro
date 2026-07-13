using System;
using System.Collections.Generic;

namespace UserService.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;
}
