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
    /// Логика взаимодействия для Nuton.xaml
    /// </summary>
    public partial class Nuton : Window
    {
        private List<MyDataPoint> points = new List<MyDataPoint>();
        public Nuton()
        {
            InitializeComponent();
            UpdatePlot();
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                points.Add(new MyDataPoint(x, y));
                lstPoints.ItemsSource = null;
                lstPoints.ItemsSource = points;
                UpdatePlot();
            }
            else
            {
                MessageBox.Show("Введите корректные числовые значения для X и Y");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            lstPoints.ItemsSource = null;
            UpdatePlot();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count < 2)
            {
                MessageBox.Show("Для интерполяции нужно минимум 2 точки");
                return;
            }

            // Проверяем равноотстоящие узлы
            if (!AreNodesEquidistant(points))
            {
                MessageBox.Show("Узлы интерполяции должны быть равноотстоящими (с одинаковым шагом)");
                return;
            }

            // Сортируем точки по X
            var sortedPoints = points.OrderBy(p => p.X).ToList();

            // Вычисляем конечные разности
            List<List<double>> finiteDifferences = CalculateFiniteDifferences(sortedPoints);

            // Создаем серию для интерполированной кривой
            var interpolatedSeries = new LineSeries
            {
                Title = "Интерполяция Ньютона",
                LineStyle = LineStyle.Solid
            };

            // Определяем диапазон для построения графика
            double minX = sortedPoints.First().X;
            double maxX = sortedPoints.Last().X;
            double h = sortedPoints[1].X - sortedPoints[0].X; // шаг
            int steps = 200;

            // Вычисляем значения многочлена Ньютона
            for (int i = 0; i <= steps; i++)
            {
                double x = minX + (maxX - minX) * i / steps;
                double y = CalculateNewtonPolynomial(x, sortedPoints, finiteDifferences, h);
                interpolatedSeries.Points.Add(new DataPoint(x, y));
            }

            // Обновляем график
            var model = new PlotModel { Title = "Интерполяция многочленом Ньютона" };

            // Добавляем исходные точки
            var scatterSeries = new ScatterSeries { Title = "Исходные точки" };
            foreach (var point in points)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            model.Series.Add(scatterSeries);
            model.Series.Add(interpolatedSeries);

            // Устанавливаем границы осей для лучшего отображения
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = minX - h,
                Maximum = maxX + h
            });

            plotView.Model = model;
        }

        private bool AreNodesEquidistant(List<MyDataPoint> points)
        {
            if (points.Count < 2) return true;

            var sorted = points.OrderBy(p => p.X).ToList();
            double h = sorted[1].X - sorted[0].X;

            for (int i = 1; i < sorted.Count - 1; i++)
            {
                if (Math.Abs((sorted[i + 1].X - sorted[i].X) - h) > 1e-10)
                    return false;
            }
            return true;
        }
        private List<List<double>> CalculateFiniteDifferences(List<MyDataPoint> points)
        {
            List<List<double>> differences = new List<List<double>>();

            // Первая разность (разности первого порядка)
            List<double> firstOrder = new List<double>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                firstOrder.Add(points[i + 1].Y - points[i].Y);
            }
            differences.Add(firstOrder);

            // Последующие разности
            for (int order = 1; order < points.Count - 1; order++)
            {
                List<double> currentOrder = new List<double>();
                List<double> prevOrder = differences[order - 1];

                for (int i = 0; i < prevOrder.Count - 1; i++)
                {
                    currentOrder.Add(prevOrder[i + 1] - prevOrder[i]);
                }

                differences.Add(currentOrder);
            }

            return differences;
        }
        private double CalculateNewtonPolynomial(double x, List<MyDataPoint> points,
                                              List<List<double>> finiteDifferences, double h)
        {
            double result = points[0].Y; // y0
            double q = (x - points[0].X) / h;

            double term = 1;
            for (int i = 0; i < finiteDifferences.Count; i++)
            {
                term *= (q - i) / (i + 1);
                result += term * finiteDifferences[i][0];
            }

            return result;
        }

        private void UpdatePlot()
        {
            var model = new PlotModel { Title = "Интерполяция многочленом Ньютона" };

            if (points.Any())
            {
                var scatterSeries = new ScatterSeries { Title = "Исходные точки" };
                foreach (var point in points)
                {
                    scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                }
                model.Series.Add(scatterSeries);
            }

            plotView.Model = model;
        }
    }
}

