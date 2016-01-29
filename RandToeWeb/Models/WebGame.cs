using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RandToeWeb.Models
{
    public class WebGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        public string GameId { get; set; }

        public List<WebMove> Moves { get; set; }
        
        public bool IsOver { get; set; } 

        public string StartingPlayerId { get; set; }
    }
}