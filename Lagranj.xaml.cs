using OxyPlot;
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
    /// Логика взаимодействия для Lagranj.xaml
    /// </summary>
    public partial class Lagranj : Window
    {
        private List<MyDataPoint> points = new List<MyDataPoint>();
        public Lagranj()
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

            // Сортируем точки по X для корректной интерполяции
            var sortedPoints = points.OrderBy(p => p.X).ToList();

            // Создаем серию для интерполированной кривой
            var interpolatedSeries = new LineSeries { Title = "Интерполяция Лагранжа" };

            // Определяем диапазон для построения графика
            double minX = sortedPoints.First().X;
            double maxX = sortedPoints.Last().X;
            int steps = 200;

            // Вычисляем значения многочлена Лагранжа для каждого X
            for (int i = 0; i <= steps; i++)
            {
                double x = minX + (maxX - minX) * i / steps;
                double y = CalculateLagrangePolynomial(x, sortedPoints);
                interpolatedSeries.Points.Add(new DataPoint(x, y));
            }

            // Обновляем график
            var model = new PlotModel { Title = "Интерполяция многочленом Лагранжа" };

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
        private double CalculateLagrangePolynomial(double x, List<MyDataPoint> points)
        {
            double result = 0;
            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                double term = points[i].Y;

                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        term *= (x - points[j].X) / (points[i].X - points[j].X);
                    }
                }

                result += term;
            }

            return result;
        }
        private void UpdatePlot()
        {
            var model = new PlotModel { Title = "Интерполяция многочленом Лагранжа" };

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
