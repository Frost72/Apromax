using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apromax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void LineMethod_Click(object sender, RoutedEventArgs e)
        {
            Line line = new Line();
            line.Show();

        }
        private void QuadraticMethod_Click(object sender, RoutedEventArgs e)
        {
            Quadratic quadratic = new Quadratic();
            quadratic.Show();
        }
        private void CubicMethod_Click(object sender, RoutedEventArgs e)
        {
            Cubic cubic = new Cubic();
            cubic.Show();
        }
       private void LagranjMethod_Click(object sender, RoutedEventArgs e)
        {
            Lagranj lagranj = new Lagranj();
            lagranj.Show();
        }

        private void NutonMethod_Click(object sender, RoutedEventArgs e)
        {
            Nuton nuton = new Nuton();  
            nuton.Show();
        }
        private void MnkMethod_Click(object sender, RoutedEventArgs e)
        {
            MNK mNK = new MNK();
            mNK.Show();
        }

        private void TeilorMethod_Click(object sender, RoutedEventArgs e)
        {
            Teilor tilor = new Teilor();
            tilor.Show();
        }

        private void ChebshevMethod_Click(object sender, RoutedEventArgs e)
        {
            Chebshev chebshev = new Chebshev();
            chebshev.Show();
        }

        private void FurieMethod_Click(object sender, RoutedEventArgs e)
        {
            Furie furie = new Furie();
            furie.Show();
        }

     
    }
}