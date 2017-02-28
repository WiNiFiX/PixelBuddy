using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Helpers;
using Rotation;
using Mouse = Helpers.Mouse;

namespace PixelBuddyWPF
{
    /// <summary>
    ///     Interaction logic for wpfSpell.xaml
    /// </summary>
    public partial class wpfSpell
    {
        private Color color;
        private readonly CombatRoutine combatRoutine;
        private readonly POINT point;
        private Key key;

        public wpfSpell(Color color, POINT point, CombatRoutine combatRoutine)
        {
            this.color = color;
            this.point = point;
            this.combatRoutine = combatRoutine;
            InitializeComponent();
        }

        private void WpfSpell_OnLoaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            cmdRefresh_Click(null, null);

            txtKey.KeyDown += TxtKey_KeyDown;
            txtXY.Text = point.X + ", " + point.Y;
            comboBox.Items.Add("=");
            comboBox.Items.Add("!=");
            cmbType.Items.Add("Single Target");
            cmbType.Items.Add("AOE");
            if (combatRoutine.Type == CombatRoutine.RotationType.SingleTarget)
                cmbType.Text = "Single Target";
            else
                cmbType.Text = "AOE";
            
            comboBox.Text = "=";
            var b = new BrushConverter();
            txtColor.Background = (Brush)b.ConvertFrom(color.ToString());
        }

        private void TxtKey_KeyDown(object sender, KeyEventArgs e)
        {
            key = e.Key;
        }

        private void cmdAddSpell_Click(object sender, RoutedEventArgs e)
        {
            var spell = new Spell(color, key, point, txtSpellName.Text, comboBox.Text, cmbType.Text);
            combatRoutine.Spells.Add(spell);

            Log.Write("Added Spell: [" + spell.Name + "] => Keybind: [" + spell.Key + "]", Brushes.Azure);
            Close();
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            color = Pixels.GetPixelColor(point);
            var b = new BrushConverter();
            txtColor.Background = (Brush)b.ConvertFrom(color.ToString());

            Log.Write($"RGB = {color.R},{color.G},{color.B}", Brushes.WhiteSmoke);
        }
    }
}