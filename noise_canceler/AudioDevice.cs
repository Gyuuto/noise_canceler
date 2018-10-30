using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace noise_canceler
{
    class AudioDevice
    {
        protected (List<Point>, List<Point>) convert_from_bytes(int bits_per_sample, int channel, byte[] buffer, int length)
        {
            var left_points = new List<Point>();
            var right_points = new List<Point>();

            int bytes = bits_per_sample / 8;
            double max_x = length / bytes;
            double min_y = -(1L << (bytes * 8 - 1)), max_y = (1L << (bytes * 8 - 1)) - 1;
            for (int i = 0; i < length / bytes; ++i) {
                if (bytes != 4) {
                    int val = 0;
                    if (bytes == 2)
                        val = BitConverter.ToInt16(buffer, bytes * i);
                    else if (bytes == 3) {
                        bool is_negative = (buffer[bytes * i + 2] & 0x80) != 0;
                        val = (is_negative ? ~buffer[bytes * i + 2] & 0xFF : buffer[bytes * i + 2]);
                        val <<= 8;
                        val += (is_negative ? ~buffer[bytes * i + 1] & 0xFF : buffer[bytes * i + 1]);
                        val <<= 8;
                        val += (is_negative ? (~buffer[bytes * i] & 0xFF) + 1 : buffer[bytes * i]);

                        if (is_negative)
                            val = -val;
                    }
                    if (i % channel == 0)
                        left_points.Add(new Point(i / max_x, Math.Max(0.0, Math.Min(1.0, (val - min_y) / (max_y - min_y))) * 2 - 1));
                    else
                        right_points.Add(new Point(i / max_x, Math.Max(0.0, Math.Min(1.0, (val - min_y) / (max_y - min_y))) * 2 - 1));
                } else {
                    float val = BitConverter.ToSingle(buffer, bytes * i);

                    if (i % channel == 0)
                        left_points.Add(new Point(i / max_x, val));
                    else
                        right_points.Add(new Point(i / max_x, val));
                }
            }

            return (left_points, right_points);
        }

        protected (byte[], byte[]) convert_from_list(int bits_per_sample, int channel, List<double> v_l, List<double> v_r = null )
        {
            int bytes = bits_per_sample / 8;
            var left_ret = new byte[bytes*v_l.Count];
            var right_ret = new byte[bytes * (v_r?.Count ?? 0)];
            int min = -(1 << (bits_per_sample - 1)), max = (1 << (bits_per_sample - 1)) - 1;

            for (int i = 0; i < v_l.Count; ++i) {
                int val = (int)((v_l[i] + 1)/2.0 * (max - min) - min);

                for (int j = 0; j < bytes; ++j)
                    left_ret[bytes * i + j] = (byte)((val & (0xFF << (j * 8))) >> (j * 8));
            }

            for (int i = 0; i < (v_r?.Count ?? 0); ++i) {
                int val = (int)((v_r[i] + 1) / 2.0 * max - min);

                for (int j = 0; j < bytes; ++j)
                    right_ret[bytes * i + j] = (byte)((val & (0xFF << (j * 8))) >> (j * 8));
            }

            return (left_ret, right_ret);
        }

        protected List<double> truncate_list(List<double> buffer, int length)
        {
            List<double> ret = new List<double>();

            for (int i = 0; i < buffer.Count / length; ++i) {
                double sum = 0;

                for (int j = 0; j < length; ++j) {
                    sum += buffer[i * length + j];
                }

                ret.Add(sum / length);
            }

            return ret;
        }

        protected List<(double, Complex)> getFFT_points ( int sample_rate, List<Point> points )
        {
            var freq = new Complex[points.Count];
            for (int i = 0; i < points.Count; ++i)
                freq[i] = new Complex((float)points[i].Y, 0);
            Fourier.Forward(freq, FourierOptions.Matlab);

            var freq_point = new List<(double, Complex)>();
            for (int i = 0; i < freq.Length; ++i) {
                freq_point.Add(((double)i * sample_rate / freq.Length, freq[i]));
            }

            return freq_point;
        }
    }
}
