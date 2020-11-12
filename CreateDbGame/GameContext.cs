using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDbGame
{
    public class GameContext : DbContext
    {
        public GameContext(string databaseName) : base(databaseName) { }
        public DbSet<User> Users { get; set; }
        public DbSet<LiveGame> LiveGames { get; set; }
        public DbSet<HistoryGame> GamesHistory { get; set; }
    }
}
