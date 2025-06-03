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
        private List<MyDataPoint> points = new List<MyDataPoint>();
        private PlotModel plotModel;
        public Quadratic()
        {
            InitializeComponent();
            UpdatePlot();
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                var point = new MyDataPoint(x, y);
                points.Add(point);
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
       
        private void UpdatePlot()
        {
            var model = new PlotModel { Title = "Квадратичная интерполяция" };

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
        
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            lstPoints.ItemsSource = null;
            UpdatePlot();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count < 3)
            {
                MessageBox.Show("Для квадратичной интерполяции нужно минимум 3 точки");
                return;
            }

            // Сортируем точки по X для корректной интерполяции
            var sortedPoints = points.OrderBy(p => p.X).ToList();

            // Выполняем квадратичную интерполяцию для каждой тройки точек
            var interpolatedSeries = new LineSeries { Title = "Интерполяция" };

            for (int i = 0; i < sortedPoints.Count - 2; i++)
            {
                var p0 = sortedPoints[i];
                var p1 = sortedPoints[i + 1];
                var p2 = sortedPoints[i + 2];

                // Решаем систему уравнений для нахождения коэффициентов параболы
                double[,] matrix = {
                    { p0.X * p0.X, p0.X, 1, p0.Y },
                    { p1.X * p1.X, p1.X, 1, p1.Y },
                    { p2.X * p2.X, p2.X, 1, p2.Y }
                };

                if (!SolveSystem(matrix, out double a, out double b, out double c))
                {
                    continue;
                }

                // Добавляем точки параболы между текущей и следующей точкой
                double startX = p0.X;
                double endX = p2.X;
                int steps = 100;

                for (int j = 0; j <= steps; j++)
                {
                    double x = startX + (endX - startX) * j / steps;
                    double y = a * x * x + b * x + c;
                    interpolatedSeries.Points.Add(new DataPoint(x, y));
                }
            }

            // Обновляем график
            var model = new PlotModel { Title = "Квадратичная интерполяция" };

            // Добавляем исходные точки
            var scatterSeries = new ScatterSeries { Title = "Исходные точки" };
            foreach (var point in points)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            model.Series.Add(scatterSeries);
            model.Series.Add(interpolatedSeries);

            plotView.Model = model;
        }
        private bool SolveSystem(double[,] matrix, out double a, out double b, out double c)
        {
            a = b = c = 0;

            int n = matrix.GetLength(0);

            // Прямой ход метода Гаусса
            for (int i = 0; i < n; i++)
            {
                // Поиск максимального элемента в столбце
                double maxEl = Math.Abs(matrix[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(matrix[k, i]);
                        maxRow = k;
                    }
                }

                // Перестановка строк
                if (maxRow != i)
                {
                    for (int k = i; k < n + 1; k++)
                    {
                        double tmp = matrix[maxRow, k];
                        matrix[maxRow, k] = matrix[i, k];
                        matrix[i, k] = tmp;
                    }
                }

                // Приведение к треугольному виду
                for (int k = i + 1; k < n; k++)
                {
                    double factor = matrix[k, i] / matrix[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                    }
                }
            }

            // Обратный ход метода Гаусса
            double[] solution = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                solution[i] = matrix[i, n] / matrix[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    matrix[k, n] -= matrix[k, i] * solution[i];
                }
            }

            a = solution[0];
            b = solution[1];
            c = solution[2];

            return true;
        }
    }
}
