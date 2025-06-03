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
            PlotModel = new PlotModel
            {
                Title = "Аппроксимация рядом Тейлора",
                Subtitle = "Синим - исходная функция, Красным - аппроксимация"
            };

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
            var selected = (FunctionSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            return selected switch
            {
                "sin(x)" => Math.Sin,
                "cos(x)" => Math.Cos,
                "exp(x)" => Math.Exp,
                "ln(1+x)" => x => Math.Log(1 + x),
                _ => Math.Sin
            };
        }

        // Новый метод с аналитическим вычислением производных для sin, cos, exp
        private double AnalyticalDerivative(Func<double, double> f, double x0, int order)
        {
            var selected = (FunctionSelector.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

            switch (selected)
            {
                case "sin(x)":
                    switch (order % 4)
                    {
                        case 0: return Math.Sin(x0);
                        case 1: return Math.Cos(x0);
                        case 2: return -Math.Sin(x0);
                        case 3: return -Math.Cos(x0);
                    }
                    break;

                case "cos(x)":
                    switch (order % 4)
                    {
                        case 0: return Math.Cos(x0);
                        case 1: return -Math.Sin(x0);
                        case 2: return -Math.Cos(x0);
                        case 3: return Math.Sin(x0);
                    }
                    break;

                case "exp(x)":
                    return Math.Exp(x0);

                case "ln(1+x)":
                    if (order == 0)
                        return Math.Log(1 + x0);
                    else
                        return Math.Pow(-1, order - 1) * SpecialFunctions.Factorial(order - 1) * Math.Pow(1 + x0, -order);

            }

            // Для прочих функций (например, ln(1+x)) используем численное дифференцирование
            return derivativeTool.EvaluateDerivative(f, x0, order);
        }

        private double EvaluateTaylorSeries(Func<double, double> f, double x, double x0, int n)
        {
            double sum = 0;
            for (int k = 0; k <= n; k++)
            {
                double dk = AnalyticalDerivative(f, x0, k);
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

            if (!double.TryParse(TxtXMin.Text, out double from))
            {
                MessageBox.Show("Некорректное значение X min");
                return;
            }

            if (!double.TryParse(TxtXMax.Text, out double to))
            {
                MessageBox.Show("Некорректное значение X max");
                return;
            }

            if (to <= from)
            {
                MessageBox.Show("X max должно быть больше X min");
                return;
            }

            var f = GetSelectedFunction();

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

            // Обновляем границы оси X графика под новые значения
            var xAxis = PlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                xAxis.Minimum = from;
                xAxis.Maximum = to;
            }

            PlotModel.Series.Clear();
            PlotModel.Series.Add(originalSeries);
            PlotModel.Series.Add(taylorSeries);
            PlotModel.InvalidatePlot(true);
        }

    }
}
