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
    /// class that represent user history game that save at historyGames table. 
    /// </summary>
    public class HistoryGame
    {
        #region properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GameId { get; set; }

        public string UserNameOne { get; set; }

        public string UserNameTwo { get; set; }

        public DateTime StartingDateTime { get; set; }

        public string WinUserName { get; set; }

        public string LossUserName { get; set; }

        public int UserNameOneScore { get; set; }

        public int UserNameTwoScore { get; set; }

        public DateTime EndingDateTime { get; set; }

        #endregion
    }
}
