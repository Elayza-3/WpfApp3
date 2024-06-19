using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;


namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
        public string track;

        public NewWindow(List<string> files)
        {

            InitializeComponent();

            foreach (var item in files)
            {
                listening_history.Items.Add(Path.GetFileNameWithoutExtension(item)); 
            }
            

        }

        private void listening_history_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            track = listening_history.SelectedItem.ToString();
            DialogResult = true;
        }
    }
}
