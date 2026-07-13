using System;
using System.Collections.Generic;

namespace AuthService.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public override string ToString()
    {
        return $"[User Id: {Id}, Email: {Email}]";
    }
}
