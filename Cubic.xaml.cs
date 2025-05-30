using MathNet.Numerics.Interpolation;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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

namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для Cubic.xaml
    /// </summary>
    public partial class Cubic : Window
    {
        private List<MyDataPoint> points = new List<MyDataPoint>();
        private PlotModel plotModel;
        public Cubic()
        {
            InitializeComponent();
            plotModel = new PlotModel { Title = "Кубический сплайн" };
            plotView.Model = plotModel;
        }

        private void UpdatePointsList()
        {
            lstPoints.ItemsSource = null;
            lstPoints.ItemsSource = points;
        }

        private void PlotPoints()
        {
            plotModel.Series.Clear();

            // Добавляем точки
            var pointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.Blue,
                Title = "Исходные точки"
            };

            foreach (var point in points.OrderBy(p => p.X))
            {
                pointSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            plotModel.Series.Add(pointSeries);
            plotModel.InvalidatePlot(true);
        }

        private void ClearPlot()
        {
            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                points.Add(new MyDataPoint (x, y));
                UpdatePointsList();
                PlotPoints();
            }
            else
            {
                MessageBox.Show("Введите корректные числовые значения для X и Y");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            UpdatePointsList();
            ClearPlot();
            txtCoefficients.Text = string.Empty;
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count < 3)
            {
                MessageBox.Show("Для построения сплайна нужно минимум 3 точки");
                return;
            }

            CalculateAndPlotSpline();
        }
        private void CalculateAndPlotSpline()
        {
            var sortedPoints = points.OrderBy(p => p.X).ToList();
            double[] x = sortedPoints.Select(p => p.X).ToArray();
            double[] y = sortedPoints.Select(p => p.Y).ToArray();
             
            // Используем библиотеку MathNet.Numerics для расчета кубического сплайна
            var spline = CubicSpline.InterpolateNatural(x, y);

            // Выводим коэффициенты
          
            // Строим график сплайна
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                Title = "Кубический сплайн"
            };

            double step = (x.Max() - x.Min()) / 100;
            for (double xi = x.Min(); xi <= x.Max(); xi += step)
            {
                lineSeries.Points.Add(new DataPoint(xi, spline.Interpolate(xi)));
            }

            plotModel.Series.Add(lineSeries);

            // Добавляем точки снова, чтобы они были поверх сплайна
            var pointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.Blue,
                Title = "Исходные точки"
            };

            foreach (var point in sortedPoints)
            {
                pointSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            plotModel.Series.Add(pointSeries);

            // Обновляем границы графика
            plotModel.Axes.Clear();
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = x.Min() - 1,
                Maximum = x.Max() + 1,
                Title = "X"
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = y.Min() - 1,
                Maximum = y.Max() + 1,
                Title = "Y"
            });

            plotModel.InvalidatePlot(true);
        }
        public void Coefficients()
        {

        }
    }
}
