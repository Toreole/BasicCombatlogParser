using CombatlogParser.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

namespace CombatlogParser;

public class Program
{
	[STAThread]
	static void Main()
	{
#if DEBUG
		Debug.AutoFlush = true;
#endif
		// setup basic logger for application.
		using var log = new LoggerConfiguration()
			.WriteTo.File($"log-{DateTimeOffset.Now:yyyy-MM-dd}.txt")
			.CreateLogger();
		Log.Logger = log;

		// ensure DB is up to date and exists.
		using (CombatlogDBContext db = new())
			db.Database.Migrate();

		MainWindow app = new();
		app.ShowDialog();
	}
}