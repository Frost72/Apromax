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
        public class SplineSegment
        {
            public double XStart { get; set; }
            public double XEnd { get; set; }
            public double A { get; set; } // свободный член
            public double B { get; set; } // коэффициент при x
            public double C { get; set; } // коэффициент при x²
            public double D { get; set; } // коэффициент при x³
        }

        private List<MyDataPoint> points = new List<MyDataPoint>();
        public PlotModel PlotModel { get; private set; }

        public Cubic()
        {
            InitializeComponent();
            InitializePlot();
            this.DataContext = this;
        }

        private void InitializePlot()
        {
            PlotModel = new PlotModel { Title = "Кубический сплайн" };
        }


        private void UpdatePointsList()
        {
            lstPoints.ItemsSource = null;
            lstPoints.ItemsSource = points.OrderBy(p => p.X).ToList();
        }

        private void PlotPoints()
        {
            PlotModel.Series.Clear();

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

            PlotModel.Series.Add(pointSeries);
            PlotModel.InvalidatePlot(true);
        }

        private void ClearPlot()
        {
            PlotModel.Series.Clear();
            PlotModel.InvalidatePlot(true);
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                points.Add(new MyDataPoint(x, y));
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

        private double[] SolveTridiagonalSystem(double[] a, double[] b, double[] c, double[] d)
        {
            int n = d.Length;
            double[] x = new double[n];
            double[] cp = new double[n];
            double[] dp = new double[n];

            // Прямой ход
            cp[0] = c[0] / b[0];
            dp[0] = d[0] / b[0];

            for (int i = 1; i < n; i++)
            {
                double m = 1.0 / (b[i] - a[i] * cp[i - 1]);
                cp[i] = c[i] * m;
                dp[i] = (d[i] - a[i] * dp[i - 1]) * m;
            }

            // Обратный ход
            x[n - 1] = dp[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                x[i] = dp[i] - cp[i] * x[i + 1];
            }

            return x;
        }

        private List<SplineSegment> CalculateSplineCoefficients()
        {
            var sortedPoints = points.OrderBy(p => p.X).ToList();
            int n = sortedPoints.Count - 1;
            double[] x = sortedPoints.Select(p => p.X).ToArray();
            double[] y = sortedPoints.Select(p => p.Y).ToArray();
            double[] h = new double[n];

            for (int i = 0; i < n; i++)
            {
                h[i] = x[i + 1] - x[i];
            }

            // Построение системы уравнений для моментов (естественный сплайн)
            double[] a = new double[n + 1];
            double[] b = new double[n + 1];
            double[] c = new double[n + 1];
            double[] d = new double[n + 1];

            // Естественные граничные условия (M₀=Mₙ=0)
            b[0] = 2; c[0] = 1; d[0] = 0;
            a[n] = 1; b[n] = 2; d[n] = 0;

            // Уравнения для внутренних точек
            for (int i = 1; i < n; i++)
            {
                a[i] = h[i - 1];
                b[i] = 2 * (h[i - 1] + h[i]);
                c[i] = h[i];
                d[i] = 3 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);
            }

            // Решение системы
            double[] M = SolveTridiagonalSystem(a, b, c, d);

            // Расчет коэффициентов для каждого отрезка
            List<SplineSegment> segments = new List<SplineSegment>();
            for (int i = 0; i < n; i++)
            {
                segments.Add(new SplineSegment
                {
                    XStart = x[i],
                    XEnd = x[i + 1],
                    A = y[i],
                    B = (y[i + 1] - y[i]) / h[i] - h[i] * (2 * M[i] + M[i + 1]) / 6,
                    C = M[i] / 2,
                    D = (M[i + 1] - M[i]) / (6 * h[i])
                });
            }

            return segments;
        }

        private void CalculateAndPlotSpline()
        {
            var segments = CalculateSplineCoefficients();
            PlotSpline(segments);
            DisplayCoefficients(segments);
        }

        private void PlotSpline(List<SplineSegment> segments)
        {
            PlotModel.Series.Clear();

            // Построение сплайна
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                Title = "Кубический сплайн"
            };

            foreach (var segment in segments)
            {
                double step = (segment.XEnd - segment.XStart) / 20;
                for (double xi = segment.XStart; xi <= segment.XEnd; xi += step)
                {
                    double dx = xi - segment.XStart;
                    double yi = segment.A + segment.B * dx + segment.C * dx * dx + segment.D * dx * dx * dx;
                    lineSeries.Points.Add(new OxyPlot.DataPoint(xi, yi));

                }
            }

            PlotModel.Series.Add(lineSeries);

            // Добавление исходных точек
            var pointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.Red,
                Title = "Узлы интерполяции"
            };

            foreach (var point in points.OrderBy(p => p.X))
            {
                pointSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            PlotModel.Series.Add(pointSeries);

            // Обновление границ графика
            double minX = points.Min(p => p.X) - 1;
            double maxX = points.Max(p => p.X) + 1;
            double minY = points.Min(p => p.Y) - 1;
            double maxY = points.Max(p => p.Y) + 1;

            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = minX,
                Maximum = maxX,
                Title = "X"
            });
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = minY,
                Maximum = maxY,
                Title = "Y"
            });

            PlotModel.InvalidatePlot(true);
        }

        private void DisplayCoefficients(List<SplineSegment> segments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Коэффициенты кубического сплайна (естественные граничные условия):");
            sb.AppendLine();

            foreach (var segment in segments)
            {
                sb.AppendLine($"Отрезок [{segment.XStart:F2}, {segment.XEnd:F2}]:");
                sb.AppendLine($"a = {segment.A:F6}");
                sb.AppendLine($"b = {segment.B:F6}");
                sb.AppendLine($"c = {segment.C:F6}");
                sb.AppendLine($"d = {segment.D:F6}");
                sb.AppendLine();
            }

            txtCoefficients.Text = sb.ToString();
        }
    }
}
    