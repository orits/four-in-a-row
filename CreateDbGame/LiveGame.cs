using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDbGame
{
    /// <summary>
    /// class that represent user live game that save at liveGames table. 
    /// </summary>
    public class LiveGame
    {
        #region properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }

        public string UserNameOne { get; set; }

        public string UserNameTwo { get; set; }

        public DateTime StartingDateTime { get; set; }

        #endregion

    }
}
