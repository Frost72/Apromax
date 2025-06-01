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

namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для Teilor.xaml
    /// </summary>
    public partial class Teilor : System.Windows.Window

    {
        public PlotModel PlotModel { get; private set; }
        private NumericalDerivative derivativeTool = new NumericalDerivative();
        public Teilor()
        {
            InitializeComponent();
            DataContext = this; 
            InitPlot();
        }
        private void InitPlot()
        {
            PlotModel = new PlotModel { Title = "Ряд Тейлора" };

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "x",
                Minimum = -5,
                Maximum = 5
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "f(x)"
            });
        }
        private Func<double, double> GetSelectedFunction()
        {
            var selected = (FunctionSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            return selected switch
            {
                "sin(x)" => Math.Sin,
                "cos(x)" => Math.Cos,
                "exp(x)" => Math.Exp,
                "ln(1+x)" => x => Math.Log(1 + x),
                _ => Math.Sin
            };
        }

        private double ComputeNthDerivative(Func<double, double> f, double x0, int order)
        {
            return derivativeTool.EvaluateDerivative(f, x0, order);
        }

        private double EvaluateTaylorSeries(Func<double, double> f, double x, double x0, int n)
        {
            double sum = 0;
            for (int k = 0; k <= n; k++)
            {
                double dk = ComputeNthDerivative(f, x0, k);
                sum += dk * Math.Pow(x - x0, k) / SpecialFunctions.Factorial(k);
            }
            return sum;
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtCenter.Text, out double x0))
            {
                MessageBox.Show("Некорректное значение центра x₀");
                return;
            }

            if (!int.TryParse(TxtTerms.Text, out int n) || n < 1)
            {
                MessageBox.Show("Число членов должно быть ≥ 1");
                return;
            }

            var f = GetSelectedFunction();

            double from = -5, to = 5;
            int points = 500;
            double step = (to - from) / points;

            var originalSeries = new LineSeries { Title = "Оригинальная функция", Color = OxyColors.Blue };
            var taylorSeries = new LineSeries { Title = $"Тейлор, n={n}", Color = OxyColors.Red, StrokeThickness = 2 };

            for (double x = from; x <= to; x += step)
            {
                try
                {
                    originalSeries.Points.Add(new DataPoint(x, f(x)));
                    taylorSeries.Points.Add(new DataPoint(x, EvaluateTaylorSeries(f, x, x0, n)));
                }
                catch
                {
                    originalSeries.Points.Add(DataPoint.Undefined);
                    taylorSeries.Points.Add(DataPoint.Undefined);
                }
            }
        }
    }
}
