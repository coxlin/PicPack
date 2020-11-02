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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PicPack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupSizeBox();
            SetupFileTypeBox();
        }

        private void SetupSizeBox()
        {
            var list = new List<string>()
            {
                "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
            };

            foreach (var i in list)
            {
                MaxSizeBox.Items.Add(i);
                MaxHeightBox.Items.Add(i);
            }
        }

        private void SetupFileTypeBox()
        {
            TypeBox.Items.Add("PNG");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PackButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
