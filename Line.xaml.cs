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
using OxyPlot.Axes;
using OxyPlot.Series;


namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для line.xaml
    /// </summary>
    public partial class Line : Window
    {
        private List<MyDataPoint> points = new List<MyDataPoint>();
        private PlotModel plotModel;
        public Line()
        {
            InitializeComponent();
            plotModel = new PlotModel { Title = "Линейная интерполяция" };
            plotView.Model = plotModel;
        }

       
        private void UpdatePointsList()
        {
            lstPoints.ItemsSource = null;
            lstPoints.ItemsSource = points;
        }
        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                points.Add(new MyDataPoint(x, y));
                UpdatePointsList();
                txtX.Clear();
                txtY.Clear();
            }
            else
            {
                MessageBox.Show("Введите корректные числовые значения для X и Y.");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            UpdatePointsList();
            txtEquation.Clear();
            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {

            if (points.Count < 2)
            {
                MessageBox.Show("Для линейной интерполяции нужно как минимум 2 точки.");
                return;
            }

            // Сортируем точки по X
            points = points.OrderBy(p => p.X).ToList();

            // Вычисляем коэффициенты для каждой пары точек
            List<string> equations = new List<string>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                double x0 = points[i].X;
                double y0 = points[i].Y;
                double x1 = points[i + 1].X;
                double y1 = points[i + 1].Y;

                double k = (y1 - y0) / (x1 - x0);
                double b = y0 - k * x0;

                equations.Add($"Для x ∈ [{x0:0.##}, {x1:0.##}]: y = {k:0.###}x + {b:0.###}");
            }

            txtEquation.Text = string.Join(Environment.NewLine, equations);

            plotModel.Series.Clear();
            plotModel.Axes.Clear();

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            // Добавляем точки как ScatterPoint
            var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 5 };
            foreach (var point in points)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }
            plotModel.Series.Add(scatterSeries);

            // Добавляем линии как DataPoint
            var lineSeries = new LineSeries { Title = "Линейная интерполяция" };
            for (int i = 0; i < points.Count - 1; i++)
            {
                double x0 = points[i].X;
                double y0 = points[i].Y;
                double x1 = points[i + 1].X;
                double y1 = points[i + 1].Y;

                lineSeries.Points.Add(new DataPoint(x0, y0));
                lineSeries.Points.Add(new DataPoint(x1, y1));
            }

            plotModel.Series.Add(lineSeries);
            plotModel.InvalidatePlot(true);
        }
    }
    
}
