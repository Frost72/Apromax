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
    /// Логика взаимодействия для Quadratic.xaml
    /// </summary>
    public partial class Quadratic : Window
    {
        private List<MyDataPoint> originalPoints = new List<MyDataPoint>();
        private PlotModel plotModel;
        public Quadratic()
        {
            InitializeComponent();
            InitializePlot();
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                var point = new MyDataPoint(x, y);
                originalPoints.Add(point);
                lstPoints.Items.Add(point);
                txtX.Clear();
                txtY.Clear();
                UpdatePlot();
            }
            else
            {
                MessageBox.Show("Введите корректные числа для X и Y.");
            }

        }
        private void InitializePlot()
        {
            plotModel = new PlotModel { Title = "Квадратичная интерполяция" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            plotView.Model = plotModel;
        }
        private void UpdatePlot()
        {
            plotModel.Series.Clear();

            // Оригинальные точки
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red
            };

            foreach (var point in originalPoints)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            plotModel.Series.Add(scatterSeries);

            // Интерполированные точки
            if (originalPoints.Count > 2)
            {
                var interpolatedPoints = MethodQuadraticInterpolation(originalPoints);
                var lineSeries = new LineSeries
                {
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Blue
                };

                foreach (var point in interpolatedPoints)
                {
                    lineSeries.Points.Add(new DataPoint(point.X, point.Y));
                }

                plotModel.Series.Add(lineSeries);
            }

            plotModel.InvalidatePlot(true);
        }
        List<MyDataPoint> MethodQuadraticInterpolation(List<MyDataPoint> points)
        {
            List<MyDataPoint> newPoints = new List<MyDataPoint>();

            if (points.Count < 3)
            {
                return new List<MyDataPoint>();
            }

            for (int i = 0; i < points.Count - 2; i++)
            {
                double x0 = points[i].X;
                double y0 = points[i].Y;
                double x1 = points[i + 1].X;
                double y1 = points[i + 1].Y;
                double x2 = points[i + 2].X;
                double y2 = points[i + 2].Y;

                double a = ((y1 - y0) * (x2 - x0) - (y2 - y0) * (x1 - x0)) /
                          ((x1 - x0) * (x2 - x0) * (x2 - x1));

                double b = (y2 - y0 - a * (x2 * x2 - x0 * x0)) / (x2 - x0);

                double c = y0 - a * x0 * x0 - b * x0;

                for (double x = x0; x <= x2; x += 0.01)
                {
                    double y = a * x * x + b * x + c;
                    newPoints.Add(new MyDataPoint(x, y));
                }
            }

            return newPoints;
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            originalPoints.Clear();
            lstPoints.Items.Clear();
            UpdatePlot();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (originalPoints.Count < 3)
            {
                MessageBox.Show("Добавьте как минимум три точки.");
                return;
            }

            UpdatePlot();
        }
    }
}
