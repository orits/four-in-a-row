using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDbGame
{
    /// <summary>
    /// that console create new DB that called fourinrowDB_or_david, for the four row game app.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog= fourinrowDB_or_david;AttachDbFilename=C:\fourinrow\databases\fourinrowDB_or_david.mdf;Integrated Security=True";
            using (var ctx = new GameContext(connectionString))
            {
                ctx.Database.Initialize(force: true);
            }
        }
    }
}
