﻿using System.Collections.Generic;

namespace Siccar.UI.Admin.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }
}
