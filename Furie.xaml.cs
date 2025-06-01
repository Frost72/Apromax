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

namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для Furie.xaml
    /// </summary>
    public partial class Furie : Window
    {
        public PlotModel PlotModel { get; private set; }
        private List<MyDataPoint> originalFunctionPoints = new List<MyDataPoint>();
        private List<MyDataPoint> fourierApproximationPoints = new List<MyDataPoint>();
        private int numberOfTerms = 5;
        private double a0 = 0;
        private List<double> anCoefficients = new List<double>();
        private List<double> bnCoefficients = new List<double>();

        public Furie()
        {
            InitializeComponent();
            InitializePlot();
            this.DataContext = this;
        }
        private void InitializePlot()
        {
            PlotModel = new PlotModel
            {
                Title = "Аппроксимация рядом Фурье",
                Subtitle = "Синим - исходная функция, Красным - аппроксимация"
            };

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "x",
                Minimum = -Math.PI - 1,
                Maximum = Math.PI + 1
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "f(x)"
            });
        }
        private bool CheckFunctionConditions(Func<double, double> func, double from, double to, int pointsCount = 1000)
        {
            // Проверка на конечное число разрывов и экстремумов
            try
            {
                double step = (to - from) / pointsCount;
                for (double x = from; x <= to; x += step)
                {
                    double y = func(x);
                    if (double.IsNaN(y) || double.IsInfinity(y))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Вычисление коэффициентов Фурье
        private void CalculateFourierCoefficients(Func<double, double> func, int nTerms)
        {
            a0 = 0;
            anCoefficients.Clear();
            bnCoefficients.Clear();

            int integrationSteps = 1000;
            double step = Math.PI / integrationSteps;

            // Вычисление a0
            for (double x = -Math.PI; x < Math.PI; x += step)
            {
                a0 += func(x) * step;
            }
            a0 /= Math.PI;

            // Вычисление коэффициентов an и bn
            for (int n = 1; n <= nTerms; n++)
            {
                double an = 0;
                double bn = 0;

                for (double x = -Math.PI; x < Math.PI; x += step)
                {
                    an += func(x) * Math.Cos(n * x) * step;
                    bn += func(x) * Math.Sin(n * x) * step;
                }

                an /= Math.PI;
                bn /= Math.PI;

                anCoefficients.Add(an);
                bnCoefficients.Add(bn);
            }
        }

        // Аппроксимация функции рядом Фурье
        private double FourierApproximation(double x, int nTerms)
        {
            double sum = a0 / 2;

            for (int n = 1; n <= nTerms; n++)
            {
                sum += anCoefficients[n - 1] * Math.Cos(n * x) + bnCoefficients[n - 1] * Math.Sin(n * x);
            }

            return sum;
        }

        // Построение графиков
        private void PlotFunctions(Func<double, double> func, int nTerms, double from, double to)
        {
            PlotModel.Series.Clear();
            originalFunctionPoints.Clear();
            fourierApproximationPoints.Clear();

            // Генерация точек для исходной функции
            var originalSeries = new LineSeries
            {
                Title = "Исходная функция",
                Color = OxyColors.Blue,
                StrokeThickness = 1
            };

            int pointsCount = 500;
            double step = (to - from) / pointsCount;

            for (double x = from; x <= to; x += step)
            {
                double y = func(x);
                originalSeries.Points.Add(new MyDataPoint(x, y));
                originalFunctionPoints.Add(new MyDataPoint(x, y));
            }

            PlotModel.Series.Add(originalSeries);

            // Генерация точек для аппроксимации Фурье
            var fourierSeries = new LineSeries
            {
                Title = $"Аппроксимация ({nTerms} членов)",
                Color = OxyColors.Red,
                StrokeThickness = 1.5
            };

            for (double x = from; x <= to; x += step)
            {
                double y = FourierApproximation(x, nTerms);
                fourierSeries.Points.Add(new MyDataPoint(x, y));
                fourierApproximationPoints.Add(new MyDataPoint(x, y));
            }

            PlotModel.Series.Add(fourierSeries);

            // Обновление графика
            PlotModel.InvalidatePlot(true);
        }

        // Пример функции для аппроксимации (прямоугольный импульс)
        private double SquareWave(double x)
        {
            // Периодическое продолжение
            x = x % (2 * Math.PI);
            if (x < -Math.PI) x += 2 * Math.PI;
            if (x > Math.PI) x -= 2 * Math.PI;

            return (x > -Math.PI / 2 && x < Math.PI / 2) ? 1 : 0;
        }

        // Пример функции (треугольный импульс)
        private double TriangleWave(double x)
        {
            // Периодическое продолжение
            x = x % (2 * Math.PI);
            if (x < -Math.PI) x += 2 * Math.PI;
            if (x > Math.PI) x -= 2 * Math.PI;

            return Math.PI / 2 - Math.Abs(x);
        }

        // Обработчик кнопки "Построить"
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Выбор функции
                Func<double, double> selectedFunction = null;
                if (RbSquareWave.IsChecked == true)
                    selectedFunction = SquareWave;
                else if (RbTriangleWave.IsChecked == true)
                    selectedFunction = TriangleWave;
                else
                {
                    MessageBox.Show("Выберите функцию для аппроксимации");
                    return;
                }

                // Проверка условий
                if (!CheckFunctionConditions(selectedFunction, -Math.PI, Math.PI))
                {
                    MessageBox.Show("Выбранная функция не удовлетворяет условиям Дирихле");
                    return;
                }

                // Получение количества членов ряда
                if (!int.TryParse(TxtTerms.Text, out numberOfTerms) || numberOfTerms < 1)
                {
                    MessageBox.Show("Введите корректное число членов ряда (≥1)");
                    return;
                }

                // Вычисление коэффициентов
                CalculateFourierCoefficients(selectedFunction, numberOfTerms);

                // Построение графиков
                double from = -Math.PI;
                double to = Math.PI;
                PlotFunctions(selectedFunction, numberOfTerms, from, to);

                // Вывод коэффициентов
                DisplayCoefficients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Вывод коэффициентов
        private void DisplayCoefficients()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Коэффициенты ряда Фурье (n = {numberOfTerms}):");
            sb.AppendLine();
            sb.AppendLine($"a₀ = {a0:F6}");
            sb.AppendLine();

            sb.AppendLine("Коэффициенты aₙ:");
            for (int n = 0; n < anCoefficients.Count; n++)
            {
                sb.AppendLine($"a{n + 1} = {anCoefficients[n]:F6}");
            }
            sb.AppendLine();

            sb.AppendLine("Коэффициенты bₙ:");
            for (int n = 0; n < bnCoefficients.Count; n++)
            {
                sb.AppendLine($"b{n + 1} = {bnCoefficients[n]:F6}");
            }

            TxtCoefficients.Text = sb.ToString();
        }
    }

    
