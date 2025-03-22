using System.Windows.Controls;

namespace CombatlogParser.Controls.Components;

/// <summary>
/// Interaction logic for LabelledProgressBar.xaml
/// </summary>
public partial class LabelledProgressBar : UserControl
{
	public LabelledProgressBar()
	{
		InitializeComponent();
	}

	public string DescriptionText
	{
		get => (string)Description.Content;
		set => Description.Content = value;
	}
	public double ProgressPercent
	{
		get => ProgressBar.Value;
		set => ProgressBar.Value = Math.Clamp(value, 0, 100);
	}

	public void UpdateDisplay(double progressPercent, string description)
	{
		ProgressPercent = progressPercent;
		DescriptionText = description;
	}
}
