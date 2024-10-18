using IDT2025.Properties;
using System.Windows.Controls;

namespace IDT2025
{
    public partial class Dashboard : UserControl
    {
        public string FirstName => Settings.Default.FirstName;

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
 