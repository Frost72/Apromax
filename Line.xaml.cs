using System;
using System.Collections.Generic;
using System.Globalization;
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
using OxyPlot.Axes;
using OxyPlot.Series;


namespace Apromax
{
    /// <summary>
    /// Логика взаимодействия для line.xaml
    /// </summary>
    public partial class Line : Window
    {
        private List<MyDataPoint> points = new List<MyDataPoint>();
        private PlotModel plotModel;
        public Line()
        {
            InitializeComponent();
            InitializePlotModel();
        }

        private void InitializePlotModel()
        {
            plotModel = new PlotModel { Title = "Линейная интерполяция" };

            // Настройка осей
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            plotView.Model = plotModel;
        }
        private void UpdatePointsList()
        {
            lstPoints.ItemsSource = null;
            lstPoints.ItemsSource = points;
        }
        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                points.Add(new MyDataPoint(x, y));
                lstPoints.Items.Add(new MyDataPoint(x, y));

                // Очистка полей ввода
                txtX.Clear();
                txtY.Clear();

                // Ограничиваем количество точек до 2
                if (points.Count > 2)
                {
                    MessageBox.Show("Можно ввести только 2 точки для линейной интерполяции");
                    points.RemoveAt(points.Count - 1);
                    lstPoints.Items.RemoveAt(lstPoints.Items.Count - 1);
                }
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
            txtEquation.Clear();
            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count != 2)
            {
                MessageBox.Show("Для линейной интерполяции необходимо ввести 2 точки");
                return;
            }

            // Сортируем точки по X
            points.Sort((a, b) => a.X.CompareTo(b.X));
            lstPoints.Items.Clear();
            foreach (var point in points)
            {
                lstPoints.Items.Add(point);
            }

            // Получаем координаты точек
            double x0 = points[0].X;
            double y0 = points[0].Y;
            double x1 = points[1].X;
            double y1 = points[1].Y;

            // Вычисляем уравнение прямой
            double k = (y1 - y0) / (x1 - x0);
            double b = y0 - k * x0;
            string equation = $"Уравнение прямой: y = {k:0.###}x + {b:0.###}";
            txtEquation.Text = equation;

            // Генерируем промежуточные точки (5 точек между основными)
            List<DataPoint> interpolatedPoints = new List<DataPoint>();
            int intermediatePointsCount = 5;
            double step = (x1 - x0) / (intermediatePointsCount + 1);

            for (int i = 0; i <= intermediatePointsCount + 1; i++)
            {
                double x = x0 + i * step;
                double y = k * x + b;
                interpolatedPoints.Add(new DataPoint(x, y));
            }

            // Очищаем график и добавляем новые серии
            plotModel.Series.Clear();

            // 1. Добавляем линию интерполяции (синяя)
            var lineSeries = new LineSeries
            {
                Title = "Интерполяция",
                Color = OxyColors.Blue
            };
            lineSeries.Points.AddRange(interpolatedPoints);
            plotModel.Series.Add(lineSeries);

            // 2. Добавляем промежуточные точки (зеленые)
            var intermediateSeries = new ScatterSeries
            {
                Title = "Промежуточные точки",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Green
            };
            foreach (var point in interpolatedPoints)
            {
                intermediateSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }
            plotModel.Series.Add(intermediateSeries);

            // 3. Добавляем исходные точки (красные)
            var originalSeries = new ScatterSeries
            {
                Title = "Исходные точки",
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red
            };
            originalSeries.Points.Add(new ScatterPoint(x0, y0));
            originalSeries.Points.Add(new ScatterPoint(x1, y1));
            plotModel.Series.Add(originalSeries);

            // Обновляем график
            plotModel.InvalidatePlot(true);
        }
    }
    
    
}
