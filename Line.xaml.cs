using System;
using System.Collections.Generic;
using System.Globalization;
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
using OxyPlot;
using OxyPlot.Series;


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

        private void OnBuildGraphClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputTextBox.Text;
                var points = ParseInput(input);
                if (points.Count < 2)
                {
                    MessageBox.Show("Нужно ввести хотя бы 2 точки.", "Ошибка");
                    return;
                }

                points.Sort((a, b) => a.X.CompareTo(b.X)); // упорядочим по X

                var model = new PlotModel { Title = "Линейная интерполяция" };

                var scatterSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColors.Red
                };

                var lineSeries = new LineSeries
                {
                    Color = OxyColors.Cyan,
                    StrokeThickness = 2
                };

                for (int i = 0; i < points.Count; i++)
                {
                    scatterSeries.Points.Add(new ScatterPoint(points[i].X, points[i].Y));
                }

                for (int i = 0; i < points.Count - 1; i++)
                {
                    double x0 = points[i].X, y0 = points[i].Y;
                    double x1 = points[i + 1].X, y1 = points[i + 1].Y;

                    lineSeries.Points.Add(new DataPoint(x0, y0));

                    for (double xi = x0 + 0.1; xi < x1; xi += 0.1)
                    {
                        double yi = y0 + (y1 - y0) / (x1 - x0) * (xi - x0);
                        lineSeries.Points.Add(new DataPoint(xi, yi));
                    }
                }

                lineSeries.Points.Add(new DataPoint(points[^1].X, points[^1].Y));

                model.Series.Add(scatterSeries);
                model.Series.Add(lineSeries);
                PlotView.Model = model;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private List<DataPoint> ParseInput(string input)
        {
            var result = new List<DataPoint>();
            var pairs = input.Split(';');
            foreach (var pair in pairs)
            {
                var coords = pair.Trim().Split(',');
                if (coords.Length != 2)
                    throw new FormatException("Неверный формат точек. Используй: x1,y1; x2,y2; ...");

                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);

                result.Add(new DataPoint(x, y));
            }
            return result;
        }
    }
    
}
