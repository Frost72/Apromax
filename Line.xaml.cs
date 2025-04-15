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
using WpfLine = System.Windows.Shapes.Line;


namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для line.xaml
    /// </summary>
    public partial class Line : Window
    {
        public Line()
        {
            InitializeComponent();
        }

        private void calculateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double x0 = double.Parse(firstx.Text);
                double y0 = double.Parse(firsty.Text);
                double x1 = double.Parse(secondx.Text);
                double y1 = double.Parse(secondy.Text);
                double x = double.Parse(pointinter.Text);

                if (x1 == x0)
                {
                    MessageBox.Show("x0 и x1 не должны быть равны.");
                    return;
                }

               
                double y = y0 + (y1 - y0) * (x - x0) / (x1 - x0);
                result.Text = y.ToString("F2");

                
                plotCanvas.Children.Clear();

                double canvasWidth = plotCanvas.ActualWidth;
                double canvasHeight = plotCanvas.ActualHeight;

                double scaleX = canvasWidth / (Math.Abs(x1 - x0) + 2);
                double scaleY = canvasHeight / (Math.Abs(y1 - y0) + 2);

                double offsetX = 1 - Math.Min(x0, x1);
                double offsetY = 1 - Math.Min(y0, y1);

                
                WpfLine line = new WpfLine
                {
                    X1 = (x0 + offsetX) * scaleX,
                    Y1 = canvasHeight - (y0 + offsetY) * scaleY,
                    X2 = (x1 + offsetX) * scaleX,
                    Y2 = canvasHeight - (y1 + offsetY) * scaleY,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2
                };

                plotCanvas.Children.Add(line);

                // Нарисовать точку интерполяции
                Ellipse point = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(point, (x + offsetX) * scaleX - 3);
                Canvas.SetTop(point, canvasHeight - (y + offsetY) * scaleY - 3);
                plotCanvas.Children.Add(point);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message);
            }

        }
        
    }
}
