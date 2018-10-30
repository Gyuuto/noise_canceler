using LiveCharts;
using LiveCharts.Wpf;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace noise_canceler
{
    class AudioInputDevice : AudioDevice
    {
        private AudioOutputDevice environment;
        private WaveIn source_stream;
        private WaveOut dest;
        private BufferedWaveProvider wave_provider;
        private WaveDataViewModel data_model;
        private NoiseCanceler noise_canceler;
        public List<double> val { get; private set; }

        public static List<string> getInputDevices ()
        {
            List<string> ret = new List<string>();

            List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; ++i)
                sources.Add(WaveIn.GetCapabilities(i));

            foreach (var source in sources) {
                ret.Add(source.ProductName);
            }

            return ret;
        }

        public static List<string> getOutputDevices ()
        {
            List<string> ret = new List<string>();

            List<WaveOutCapabilities> sources = new List<WaveOutCapabilities>();
            for (int i = 0; i < WaveOut.DeviceCount; ++i)
                sources.Add(WaveOut.GetCapabilities(i));

            foreach (var source in sources) {
                ret.Add(source.ProductName);
            }

            return ret;
        }

        public AudioInputDevice ( WaveDataViewModel data_model, AudioOutputDevice environment, int input_device_index, int output_device_index )
        {
            this.data_model = data_model;

            noise_canceler = new NoiseCanceler();

            source_stream = new NAudio.Wave.WaveIn();
            source_stream.DeviceNumber = input_device_index;
            source_stream.WaveFormat = new WaveFormat(44100, 24, 1);

            dest = new NAudio.Wave.WaveOut();
            dest.DeviceNumber = output_device_index;
            dest.DesiredLatency = 100;
            wave_provider = new BufferedWaveProvider(source_stream.WaveFormat);
            wave_provider.DiscardOnBufferOverflow = true;
            dest.Init(wave_provider);

            int cnt = 0;
            List<double> buffers = new List<double>();
            source_stream.DataAvailable += (obj, e) => {
/*
                var truncated_points = get_point_from_bytes(e.Buffer, e.BytesRecorded);
                data_model.DataL.Clear();
                data_model.DataL.AddRange(truncated_points);

                var freq_point = getFFT_points(source_stream.WaveFormat.SampleRate, 
                    convert_from_bytes(source_stream.WaveFormat.BitsPerSample, source_stream.WaveFormat.Channels, e.Buffer, e.BytesRecorded).Item1);
                data_model.FreqL.Clear();
                data_model.FreqL.AddRange(freq_point.GetRange(0, 200).Select(x => new Point(x.Item1, x.Item2.Magnitude)));
//*/

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                List<double> source_point = convert_from_bytes(source_stream.WaveFormat.BitsPerSample, source_stream.WaveFormat.Channels, e.Buffer, e.BytesRecorded).Item1
                    .Select(x => x.Y)
                    .ToList();
                List<double> canceled_buf = noise_canceler.get_noise_canceled_buffer(environment.get_sample_rate(), environment.get_left_val(), environment.get_right_val(),
                    source_stream.WaveFormat.SampleRate, source_point);

                byte[] result = convert_from_list(dest.OutputWaveFormat.BitsPerSample, dest.OutputWaveFormat.Channels, canceled_buf).Item1;
                wave_provider.AddSamples(result, 0, result.Length);
                /*
                var truncated_points = get_point_from_bytes(result, result.Length);
                data_model.DataOutput.Clear();
                data_model.DataOutput.AddRange(truncated_points);

                var freq_point = getFFT_points(dest.OutputWaveFormat.SampleRate, 
                    convert_from_bytes(dest.OutputWaveFormat.BitsPerSample, dest.OutputWaveFormat.Channels, result, result.Length).Item1);
                data_model.FreqOutput.Clear();
                data_model.FreqOutput.AddRange(freq_point.GetRange(0, 200).Select(x => new Point(x.Item1, x.Item2.Magnitude)));
                //*/
            };
        }

        ~AudioInputDevice ()
        {
            stop();
        }

        private List<Point> get_point_from_bytes ( byte[] buffer, int length )
        {
            (List<Point>, List<Point>) points = convert_from_bytes(source_stream.WaveFormat.BitsPerSample, source_stream.WaveFormat.Channels, buffer, length);
            val = points.Item1.Select(x => x.Y).ToList();

            var truncated_buffer = truncate_list(points.Item1.Select(x => x.Y).ToList(), 100);
            List<Point> truncated_points = new List<Point>();
            for (int i = 0; i < truncated_buffer.Count; ++i) {
                truncated_points.Add(new Point() { Y = truncated_buffer[i], X = (double)i / truncated_buffer.Count });
            }
            return truncated_points;
        }

        public void start ()
        {
            source_stream.StartRecording();
            dest.Play();
        }

        public void stop ()
        {
            source_stream.StopRecording();
            dest.Stop();
        }
    }
}
