using System.Windows.Controls;

namespace IDT2025
{
    public partial class Dashboard : UserControl
    {
        public string FirstName => App.FirstName;

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
 