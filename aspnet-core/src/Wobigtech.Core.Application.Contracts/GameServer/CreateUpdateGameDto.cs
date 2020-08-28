using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Wobigtech.Core.GameServer
{
    public class CreateUpdateGameDto
    {
        [Required]
        [StringLength(128)]
        public string FriendlyName { get; set; }
        [Required]
        public int SteamID { get; set; }
    }
}
