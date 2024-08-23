using System.Windows.Media;

namespace CombatlogParser.Controls;

public static class SpellSchoolColors
{
	public static Brush Shadow => ClassColors.Brushes.Warlock; //warlock color = shadow

	public static readonly Brush Fire = new SolidColorBrush(Color.FromRgb(214, 42, 15));
	public static Brush Nature => ClassColors.Brushes.Shaman;
	public static Brush Physical => ClassColors.Brushes.Warrior;
	public static Brush Arcane => ClassColors.Brushes.Mage;
	public static Brush Holy => ClassColors.Brushes.Monk;
	public static Brush Frost => new SolidColorBrush(Color.FromRgb(173, 216, 230));

	private static Brush CreateGradientBrush(params Brush[] brushes)
	{
		var gradientBrush = new LinearGradientBrush();
		double offsetIncrement = 1.0 / (brushes.Length - 1);

		for (int i = 0; i < brushes.Length; i++)
		{
			if (brushes[i] is SolidColorBrush solidColorBrush)
			{
				gradientBrush.GradientStops.Add(new GradientStop(solidColorBrush.Color, i * offsetIncrement));
			}
		}

		return gradientBrush;
	}

	public static Brush GetSchoolBrush(this SpellSchool school)
	{
		// Check for Basic Schools
		if (school == SpellSchool.Shadow)
			return Shadow;
		else if (school == SpellSchool.Fire)
			return Fire;
		else if (school == SpellSchool.Nature)
			return Nature;
		else if (school == SpellSchool.Physical)
			return Physical;
		else if (school == SpellSchool.Arcane)
			return Arcane;
		else if (school == SpellSchool.Holy)
			return Holy;
		else if (school == SpellSchool.Frost)
			return Frost;

		// Check for school combinations
		var brushes = new List<Brush>();

		if ((school & SpellSchool.Shadow) == SpellSchool.Shadow)
			brushes.Add(Shadow);
		if ((school & SpellSchool.Fire) == SpellSchool.Fire)
			brushes.Add(Fire);
		if ((school & SpellSchool.Nature) == SpellSchool.Nature)
			brushes.Add(Nature);
		if ((school & SpellSchool.Arcane) == SpellSchool.Arcane)
			brushes.Add(Arcane);
		if ((school & SpellSchool.Physical) == SpellSchool.Physical)
			brushes.Add(Physical);
		if ((school & SpellSchool.Holy) == SpellSchool.Holy)
			brushes.Add(Holy);
		if ((school & SpellSchool.Frost) == SpellSchool.Frost)
			brushes.Add(Frost);

		if (brushes.Count > 1)
		{
			return CreateGradientBrush(brushes.ToArray());
		}

		return Brushes.Red; //fallback.
	}
}
