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
        private List<MyDataPoint> originalPoints = new List<MyDataPoint>();
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
        
        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtX.Text, out double x) && double.TryParse(txtY.Text, out double y))
            {
                var point = new MyDataPoint(x, y);
                originalPoints.Add(point);
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            originalPoints.Clear();
            lstPoints.Items.Clear();
            UpdatePlot();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
           
            if (originalPoints.Count < 2)
            {
                MessageBox.Show("Добавьте как минимум две точки.");
                return;
            }

            UpdatePlot(); // Обновляем график, если нужно
        }
        private void UpdatePlot()
        {
            plotModel.Series.Clear();

            // Оригинальные точки
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red
            };

            foreach (var point in originalPoints)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }
            plotModel.Series.Add(scatterSeries);

            // Интерполированные точки
            if (originalPoints.Count > 1)
            {
                var interpolatedPoints = MethodLinearInterpolation(originalPoints);
                var lineSeries = new LineSeries
                {
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Blue
                };
                foreach (var point in interpolatedPoints)
                {
                    lineSeries.Points.Add(new DataPoint(point.X, point.Y));
                }

                plotModel.Series.Add(lineSeries);
            }

            plotModel.InvalidatePlot(true);
        }
        List<MyDataPoint> MethodLinearInterpolation(List<MyDataPoint> points)
        {
            List<MyDataPoint> newPoints = new List<MyDataPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1) break;
                else
                {
                    double xStart = points[i].X;
                    double xEnd = points[i + 1].X;

                    for (double x = xStart; x <= xEnd; x += 0.01)
                    {
                        double slope = (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X);
                        double newY = points[i].Y + slope * (x - points[i].X);
                        newPoints.Add(new MyDataPoint(x, newY));
                    }
                }
            }
            return newPoints;
        }

    }



}
    
    

