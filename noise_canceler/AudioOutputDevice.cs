using LiveCharts;
using LiveCharts.Wpf;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace noise_canceler
{
    class AudioOutputDevice : AudioDevice
    {
        private WasapiLoopbackCapture source_stream;
        private WaveDataViewModel data_model;
        private List<double> left_val;
        private List<double> right_val;

        public static List<String> getOutputDevices ()
        {
            return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(x => x.FriendlyName).ToList();
        }

        public int get_sample_rate ()
        {
            return source_stream.WaveFormat.SampleRate;
        }

        public List<double> get_left_val ()
        {
            List<double> ret;
            lock (left_val) {
                ret = new List<double>(left_val);
                left_val.Clear();
            }

            return ret;
        }

        public List<double> get_right_val ()
        {
            List<double> ret;
            lock (right_val) {
                ret = new List<double>(right_val);
                right_val.Clear();
            }

            return ret;
        }

        public AudioOutputDevice ( WaveDataViewModel data_model, int device_index )
        {
            this.data_model = data_model;

            left_val = new List<double>();
            right_val = new List<double>();

            source_stream = new WasapiLoopbackCapture(new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[device_index]);
            source_stream.DataAvailable += (obj, e) => {
                (List<Point>, List<Point>) points = convert_from_bytes(source_stream.WaveFormat.BitsPerSample, source_stream.WaveFormat.Channels, e.Buffer, e.BytesRecorded);
                lock ( left_val ) {
                    left_val.AddRange(points.Item1.Select(x => x.Y).ToList());
                }
                lock ( right_val ) {
                    right_val.AddRange(points.Item2.Select(x => x.Y).ToList());
                }

                /*
                var truncated_buffer_L = truncate_list(points.Item1.Select(x => x.Y).ToList(), 100);
                List<Point> truncated_points_L = new List<Point>();
                for (int i = 0; i < truncated_buffer_L.Count; ++i) {
                    truncated_points_L.Add(new Point() { Y = truncated_buffer_L[i], X = (double)i / truncated_buffer_L.Count });
                }
                data_model.DataL.Clear();
                data_model.DataL.AddRange(truncated_points_L);

                var freq_point_L = getFFT_points(source_stream.WaveFormat.SampleRate, points.Item1);
                data_model.FreqL.Clear();
                data_model.FreqL.AddRange(freq_point_L.GetRange(0, 200).Select(x => new Point(x.Item1, x.Item2.Magnitude)));
                //*/
                /*
                var truncated_buffer_R = truncate_list(points.Item2.Select(x => x.Y).ToList(), 100);
                List<Point> truncated_points_R = new List<Point>();
                for (int i = 0; i < truncated_buffer_R.Count; ++i) {
                    truncated_points_R.Add(new Point() { Y = truncated_buffer_R[i], X = (double)i / truncated_buffer_R.Count });
                }
                data_model.DataR.Clear();
                data_model.DataR.AddRange(truncated_points_R);

                var freq_point_R = getFFT_points(source_stream.WaveFormat.SampleRate, points.Item2);
                data_model.FreqR.Clear();
                data_model.FreqR.AddRange(freq_point_R.GetRange(0, 200).Select(x => new Point(x.Item1, x.Item2.Magnitude)));
/*
                NoiseCanceler nc = new NoiseCanceler();
                var tmp = nc.apply_freq_weight(get_sample_rate(), right_val);
                var truncated_tmp = truncate_list(tmp, 100);
                List<Point> truncated_points_tmp = new List<Point>();
                for (int i = 0; i < truncated_tmp.Count; ++i) {
                    truncated_points_tmp.Add(new Point() { Y = truncated_tmp[i], X = (double)i / truncated_tmp.Count });
                }
                data_model.DataOutput.Clear();
                data_model.DataOutput.AddRange(truncated_points_tmp);

                var freq_tmp = getFFT_points(source_stream.WaveFormat.SampleRate, tmp.Select(x => new Point(0, x)).ToList());
                data_model.FreqOutput.Clear();
                data_model.FreqOutput.AddRange(freq_tmp.GetRange(0, 200).Select(x => new Point(x.Item1, x.Item2.Magnitude)));
//*/
            };
        }

        ~AudioOutputDevice ()
        {
            stop();
        }

        public void start ()
        {
            source_stream.StartRecording();
        }

        public void stop ()
        {
            source_stream.StopRecording();
        }

    }
}
