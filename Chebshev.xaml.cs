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
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics;
using OxyPlot.Wpf;

namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для Chebshev.xaml
    /// </summary>
    public partial class Chebshev : System.Windows.Window
    {
        public PlotModel PlotModel { get; private set; }

        public Func<double, double> Func { get; set; } = Math.Sin;

        public double IntervalStart { get; set; } = -1;
        public double IntervalEnd { get; set; } = 1;
        public int Degree { get; set; } = 5;

        public double[] Coefficients { get; private set; }


        public string NodesAndValuesText { get; private set; } = "";
        public Chebshev()
        {
            InitializeComponent();

            PlotModel = new PlotModel
            {
                Title = "Аппроксимация многочленом Чебышева",
                Subtitle = "Синим - исходная функция, Красным - аппроксимация"
            };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x" });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "f(x)" });

            PlotView.Model = PlotModel;
        }
        public double ChebyshevPolynomial(int n, double x)
        {
            if (n == 0) return 1;
            if (n == 1) return x;
            return 2 * x * ChebyshevPolynomial(n - 1, x) - ChebyshevPolynomial(n - 2, x);
        }

        // Узлы Чебышева на [-1, 1]
        public double[] ComputeChebyshevNodes(int n)
        {
            double[] nodes = new double[n];
            for (int k = 1; k <= n; k++)
            {
                nodes[k - 1] = Math.Cos((2 * k - 1) * Math.PI / (2.0 * n));
            }
            return nodes;
        }

        // Узлы на произвольном отрезке [a,b]
        public double[] ComputeChebyshevNodesOnInterval(int n, double a, double b)
        {
            var nodes = ComputeChebyshevNodes(n);
            return nodes.Select(t => 0.5 * (a + b) + 0.5 * (b - a) * t).ToArray();
        }

        // Вычисление коэффициентов аппроксимации
        public double[] ComputeCoefficients(int degree, double a, double b)
        {
            int n = degree + 1;
            var nodes = ComputeChebyshevNodesOnInterval(n, a, b);
            var values = nodes.Select(x => Func(x)).ToArray();

            NodesAndValuesText = string.Join(Environment.NewLine, nodes.Zip(values, (x, y) => $"x={x:F4}, f(x)={y:F4}"));
            TxtNodesValues.Text = NodesAndValuesText; // Отобразим сразу

            double[] c = new double[n];
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int k = 0; k < n; k++)
                {
                    // Перевод x_k в [-1,1]
                    double t_k = (2 * nodes[k] - a - b) / (b - a);
                    sum += values[k] * ChebyshevPolynomial(j, t_k);
                }
                c[j] = (2.0 / n) * sum;
                if (j == 0) c[j] /= 2.0;
            }

            Coefficients = c;
            return c;
        }

        // Вычисление значения аппроксимации в точке x
        public double EvaluateApproximation(double x, double a, double b, double[] coeffs)
        {
            double t = (2 * x - a - b) / (b - a);
            double sum = 0;
            for (int j = 0; j < coeffs.Length; j++)
            {
                sum += coeffs[j] * ChebyshevPolynomial(j, t);
            }
            return sum;
        }

        // Построение графика исходной функции и аппроксимации
        public void BuildPlot()
        {
            if (Degree < 1) throw new ArgumentException("Степень аппроксимации должна быть ≥ 1.");

            var c = ComputeCoefficients(Degree, IntervalStart, IntervalEnd);

            var originalSeries = new LineSeries { Title = "Исходная функция", Color = OxyPlot.OxyColors.Blue };
            var approxSeries = new LineSeries { Title = $"Аппроксимация, степень {Degree}", Color = OxyPlot.OxyColors.Red, StrokeThickness = 2 };

            int points = 500;
            double step = (IntervalEnd - IntervalStart) / points;

            for (double x = IntervalStart; x <= IntervalEnd; x += step)
            {
                originalSeries.Points.Add(new DataPoint(x, Func(x)));
                approxSeries.Points.Add(new DataPoint(x, EvaluateApproximation(x, IntervalStart, IntervalEnd, c)));
            }

            PlotModel.Series.Clear();
            PlotModel.Series.Add(originalSeries);
            PlotModel.Series.Add(approxSeries);
            PlotModel.InvalidatePlot(true);
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            // Чтение и проверка параметров из UI
            if (!double.TryParse(TxtXMin.Text, out double xmin))
            {
                MessageBox.Show("Некорректное значение X min");
                return;
            }

            if (!double.TryParse(TxtXMax.Text, out double xmax))
            {
                MessageBox.Show("Некорректное значение X max");
                return;
            }

            if (xmax <= xmin)
            {
                MessageBox.Show("X max должен быть больше X min");
                return;
            }

            if (!int.TryParse(TxtDegree.Text, out int degree) || degree < 1)
            {
                MessageBox.Show("Степень должна быть целым числом ≥ 1");
                return;
            }

            IntervalStart = xmin;
            IntervalEnd = xmax;
            Degree = degree;

            try
            {
                BuildPlot();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
