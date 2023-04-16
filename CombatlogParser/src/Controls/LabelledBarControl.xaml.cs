using CombatlogParser.Formatting;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CombatlogParser.Controls
{
    /// <summary>
    /// Interaction logic for LabelledBarControl.xaml
    /// </summary>
    public partial class LabelledBarControl : UserControl
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), 
            typeof(double), 
            typeof(LabelledBarControl),
            new UIPropertyMetadata(1.0d, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is LabelledBarControl bar) 
            { 
                bar.Maximum = (double)e.NewValue;
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), 
            typeof(double), 
            typeof(LabelledBarControl),
            new UIPropertyMetadata(0.0d, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LabelledBarControl bar)
            {
                bar.Value = (double)e.NewValue;
            }
        }

        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register(
            nameof(FillColor),
            typeof(Brush),
            typeof(LabelledBarControl),
            new UIPropertyMetadata(Brushes.Green, new PropertyChangedCallback(OnForegroundChanged)));

        public static readonly DependencyProperty TextColorPoperty = DependencyProperty.Register(
            "TextColor",
            typeof(Brush),
            typeof(LabelledBarControl),
            new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnTextColorChanged)));

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LabelledBarControl bar)
            {
                bar.FillColor = (Brush)e.NewValue;
            }
        }

        private static void OnTextColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LabelledBarControl bar)
            {
                bar.AmountLabel.Foreground = (Brush)e.NewValue;
            }
        }

        public double Maximum { 
            get => FillBar.Maximum; 
            set => FillBar.Maximum = value; 
        }
        public double Value
        {
            get => FillBar.Value;
            set
            {
                FillBar.Value = value;
                AmountLabel.Content = value.ToShortFormString();
            }
        }

        public LabelledBarControl()
        {
            InitializeComponent();
        }

        public Brush FillColor
        {
            get => FillBar.Foreground;
            set => FillBar.Foreground = value;
        }

        public Brush TextColor
        {
            get => AmountLabel.Foreground;
            set => AmountLabel.Foreground = value;
        }
    }
}
