﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Domain
{
    public class LoginResult
    {
        public int UsuarioId { get; set; }
        public string? Apelido { get; set; }
        public string? Email { get; set; }
        public string? AppOrigem { get; set; }
        public string? IpOrigem { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
    }
}
