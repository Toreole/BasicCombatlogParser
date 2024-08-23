using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;
using Microsoft.EntityFrameworkCore;

namespace CombatlogParser
{
	public static partial class Queries
	{
		/// <summary>
		/// Gets the stored Metadata for all players whose names start with the provided string. This should be case-insenstive.
		/// </summary>
		public static PlayerMetadata[] FindPlayersWithNameLike(string start)
		{
			using CombatlogDBContext dbContext = new();
			var likeExpression = start + '%';
			var result = dbContext.Players.Where(p => EF.Functions.Like(p.Name, likeExpression)).Take(10).ToArray();
			return result;
		}
	}
}
