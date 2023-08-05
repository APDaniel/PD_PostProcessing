using System.Windows;

namespace PD_ScriptTemplate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ScriptWindow : Window
    {
        public ScriptWindow()
        {
            InitializeComponent();
           
        }
        private void Close_GUI(object sender, RoutedEventArgs e)
        {
            Close(); // this closes the script GUI
        }
    }
}
