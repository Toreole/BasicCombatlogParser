using System.Windows.Media;

namespace CombatlogParser.Controls;

public static class SpellSchoolColors
{
	public static Brush Shadow => ClassColors.Brushes.Warlock; //warlock color = shadow

	public static readonly Brush Fire = new SolidColorBrush(Color.FromRgb(214, 42, 15));
	public static Brush Nature => ClassColors.Brushes.Shaman;
	public static Brush Physical => ClassColors.Brushes.Warrior;
	public static Brush Arcane => ClassColors.Brushes.Mage;

	public static Brush GetSchoolBrush(this SpellSchool school)
	{
		if ((school & SpellSchool.Shadow) == SpellSchool.Shadow)
			return Shadow;
		else if ((school & SpellSchool.Fire) == SpellSchool.Fire)
			return Fire;
		else if ((school & SpellSchool.Nature) == SpellSchool.Nature)
			return Nature;
		else if ((school & SpellSchool.Arcane) == SpellSchool.Arcane)
			return Arcane;
		else if ((school & SpellSchool.Physical) == SpellSchool.Physical)
			return Physical;
		return Brushes.Red; //fallback.
	}
}
