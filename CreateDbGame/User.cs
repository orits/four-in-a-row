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
    /// class that represent user at the game DB that save at Users table. 
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string EmojiIcon { get; set; }
    }
}
