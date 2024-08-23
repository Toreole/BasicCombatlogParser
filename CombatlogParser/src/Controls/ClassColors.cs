using System.Windows.Media;

namespace CombatlogParser.Controls;

public static class ClassColors
{
	public static readonly Color Death_Knight = new()
	{
		R = 196,
		G = 30,
		B = 58,
		A = 255
	};
	public static readonly Color Demon_Hunter = new()
	{
		R = 163,
		G = 48,
		B = 201,
		A = 255
	};
	public static readonly Color Druid = new()
	{
		R = 255,
		G = 124,
		B = 10,
		A = 255
	};
	public static readonly Color Evoker = new()
	{
		R = 51,
		G = 147,
		B = 127,
		A = 255
	};
	public static readonly Color Hunter = new()
	{
		R = 170,
		G = 211,
		B = 114,
		A = 255
	};
	public static readonly Color Mage = new()
	{
		R = 63,
		G = 199,
		B = 235,
		A = 255
	};
	public static readonly Color Monk = new()
	{
		R = 0,
		G = 255,
		B = 152,
		A = 255
	};
	public static readonly Color Paladin = new()
	{
		R = 244,
		G = 140,
		B = 186,
		A = 255
	};
	public static readonly Color Priest = Colors.White;
	public static readonly Color Rogue = new()
	{
		R = 255,
		G = 244,
		B = 104,
		A = 255
	};
	public static readonly Color Shaman = new()
	{
		R = 0,
		G = 112,
		B = 221,
		A = 255
	};
	public static readonly Color Warlock = new()
	{
		R = 135,
		G = 136,
		B = 238,
		A = 255
	};
	public static readonly Color Warrior = new()
	{
		R = 198,
		G = 155,
		B = 109,
		A = 255
	};

	public static class Brushes
	{
		public static readonly SolidColorBrush Death_Knight = new(ClassColors.Death_Knight);
		public static readonly SolidColorBrush Demon_Hunter = new(ClassColors.Demon_Hunter);
		public static readonly SolidColorBrush Druid = new(ClassColors.Druid);
		public static readonly SolidColorBrush Evoker = new(ClassColors.Evoker);
		public static readonly SolidColorBrush Hunter = new(ClassColors.Hunter);
		public static readonly SolidColorBrush Mage = new(ClassColors.Mage);
		public static readonly SolidColorBrush Monk = new(ClassColors.Monk);
		public static readonly SolidColorBrush Paladin = new(ClassColors.Paladin);
		public static readonly SolidColorBrush Priest = new(ClassColors.Priest);
		public static readonly SolidColorBrush Rogue = new(ClassColors.Rogue);
		public static readonly SolidColorBrush Shaman = new(ClassColors.Shaman);
		public static readonly SolidColorBrush Warlock = new(ClassColors.Warlock);
		public static readonly SolidColorBrush Warrior = new(ClassColors.Warrior);
	}

	//helper extensions
	public static Color GetClassColor(this ClassId classId)
		=> classId switch
		{
			ClassId.Death_Knight => Death_Knight,
			ClassId.Warrior => Warrior,
			ClassId.Paladin => Paladin,
			ClassId.Hunter => Hunter,
			ClassId.Rogue => Rogue,
			ClassId.Priest => Priest,
			ClassId.Shaman => Shaman,
			ClassId.Mage => Mage,
			ClassId.Warlock => Warlock,
			ClassId.Monk => Monk,
			ClassId.Druid => Druid,
			ClassId.Demon_Hunter => Demon_Hunter,
			ClassId.Evoker => Evoker,
			_ => Colors.Black
		};

	public static SolidColorBrush GetClassBrush(this ClassId classId)
		=> classId switch
		{
			ClassId.Death_Knight => Brushes.Death_Knight,
			ClassId.Warrior => Brushes.Warrior,
			ClassId.Paladin => Brushes.Paladin,
			ClassId.Hunter => Brushes.Hunter,
			ClassId.Rogue => Brushes.Rogue,
			ClassId.Priest => Brushes.Priest,
			ClassId.Shaman => Brushes.Shaman,
			ClassId.Mage => Brushes.Mage,
			ClassId.Warlock => Brushes.Warlock,
			ClassId.Monk => Brushes.Monk,
			ClassId.Druid => Brushes.Druid,
			ClassId.Demon_Hunter => Brushes.Demon_Hunter,
			ClassId.Evoker => Brushes.Evoker,
			_ => new SolidColorBrush(Colors.Black)
		};
}
