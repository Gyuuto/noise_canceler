using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace noise_canceler
{
    class WaveDataViewModel
    {
        public ChartValues<Point> DataL { get; set; }
        public ChartValues<Point> DataR { get; set; }
        public ChartValues<Point> FreqL { get; set; }
        public ChartValues<Point> FreqR { get; set; }

        public ChartValues<Point> DataOutput { get; set; }
        public ChartValues<Point> FreqOutput { get; set; }

        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public WaveDataViewModel()
        {
            Charting.For<Point>(Mappers.Xy<Point>().X(model => model.X).Y(model => model.Y));

            DataL = new ChartValues<Point>();
            DataR = new ChartValues<Point>();
            FreqL = new ChartValues<Point>();
            FreqR = new ChartValues<Point>();

            DataOutput = new ChartValues<Point>();
            FreqOutput = new ChartValues<Point>();

            XFormatter = x => x.ToString();
            YFormatter = y => y.ToString();
        }
    }
}
