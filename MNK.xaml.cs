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
using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;

namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для MNK.xaml
    /// </summary>
    public partial class MNK : System.Windows.Window
    {
        public PlotModel PlotModel { get; private set; }
        public List<DataPoint> DataPoints { get; private set; } = new List<DataPoint>();
        public int Degree { get; set; } = 2;
        public double[] Coefficients { get; private set; }

        public MNK()
        {
            InitializeComponent();

            PlotModel = new PlotModel { Title = "Аппроксимация МНК" };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x" });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "y" });

            PlotView.Model = PlotModel;
        }
        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(TxtY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                DataPoints.Add(new DataPoint(x, y));
                LstPoints.Items.Add($"x = {x:F3}, y = {y:F3}");
                TxtX.Clear();
                TxtY.Clear();
            }
            else
            {
                MessageBox.Show("Введите корректные значения для X и Y");
            }
        }

        private void BtnClearPoints_Click(object sender, RoutedEventArgs e)
        {
            DataPoints.Clear();
            LstPoints.Items.Clear();
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtDegree.Text, out int degree) || degree < 1)
            {
                MessageBox.Show("Степень должна быть целым числом ≥ 1");
                return;
            }

            if (DataPoints.Count < degree + 1)
            {
                MessageBox.Show("Недостаточно точек для заданной степени");
                return;
            }

            Degree = degree;
            ComputeCoefficients();
            BuildPlot();
        }

        private void ComputeCoefficients()
        {
            int n = DataPoints.Count;
            int m = Degree + 1;

            var matrixBuilder = Matrix<double>.Build;
            var vectorBuilder = Vector<double>.Build;

            var A = matrixBuilder.Dense(n, m, (i, j) => Math.Pow(DataPoints[i].X, j));
            var y = vectorBuilder.Dense(DataPoints.Select(p => p.Y).ToArray());

            var At = A.Transpose();
            var AtA = At * A;
            var AtY = At * y;

            var coeffs = AtA.Solve(AtY);
            Coefficients = coeffs.ToArray();

            TxtCoefficients.Text = string.Join(Environment.NewLine, Coefficients.Select((c, i) => $"a{i} = {c:F4}"));
        }


        private double EvaluatePolynomial(double x)
        {
            double result = 0;
            for (int i = 0; i < Coefficients.Length; i++)
            {
                result += Coefficients[i] * Math.Pow(x, i);
            }
            return result;
        }

        private void BuildPlot()
        {
            var originalPoints = new ScatterSeries
            {
                Title = "Исходные точки",
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Blue,
                MarkerSize = 4
            };
            // ВАЖНО: конвертация DataPoint → ScatterPoint
            originalPoints.Points.AddRange(DataPoints.Select(p => new ScatterPoint(p.X, p.Y)));

            var approximation = new LineSeries
            {
                Title = $"Аппроксимация (степень {Degree})",
                Color = OxyColors.Red,
                StrokeThickness = 2
            };

            double minX = DataPoints.Min(p => p.X);
            double maxX = DataPoints.Max(p => p.X);
            int count = 500;
            double step = (maxX - minX) / count;

            for (double x = minX; x <= maxX; x += step)
            {
                approximation.Points.Add(new DataPoint(x, EvaluatePolynomial(x)));
            }

            PlotModel.Series.Clear();
            PlotModel.Series.Add(originalPoints);
            PlotModel.Series.Add(approximation);
            PlotModel.InvalidatePlot(true);

        }
    }
}
