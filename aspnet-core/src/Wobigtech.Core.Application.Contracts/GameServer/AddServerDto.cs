using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Wobigtech.Core.GameServer
{
    public class AddServerDto
    {
        [Required]
        public string IPAddress { get; set; }
        [Required]
        public int Port { get; set; }
        public string FriendlyName { get; set; }
        [Required]
        [StringLength(128)]
        public string Secret { get; set; }
    }
}
