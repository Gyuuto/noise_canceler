using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace noise_canceler
{
    class NoiseCanceler
    {
        private const int UPPER_FREQUENCY = 22000;
        private int delay;
        private double left_weight, right_weight;
        private double[] freq_weight;

        public NoiseCanceler()
        {
            delay = 0;
            left_weight = 0.5;
            right_weight = 0.5;

            freq_weight = new double[10]; // divide 10 parts
                                          /*
                                                      freq_weight[0] = 0.1; // 0 - 2200
                                                      freq_weight[1] = 0.1; // 2200 - 4400
                                                      freq_weight[2] = 0.3; // 4400 - 6600
                                                      freq_weight[3] = 0.5; // 6600 - 8800
                                                      freq_weight[4] = 0.5; // 8800 - 11000
                                                      freq_weight[5] = 0.6; // 11000 - 13200
                                                      freq_weight[6] = 0.6; // 13200 - 15400
                                                      freq_weight[7] = 0.6; // 15400 - 17600
                                                      freq_weight[8] = 0.8; // 17600 - 19800
                                                      freq_weight[9] = 0.8; // 19800 - 22000
                                          */
            freq_weight[0] = 1.0; // 0 - 2200
            freq_weight[1] = 1.0; // 2200 - 4400
            freq_weight[2] = 1.0; // 4400 - 6600
            freq_weight[3] = 1.0; // 6600 - 8800
            freq_weight[4] = 1.0; // 8800 - 11000
            freq_weight[5] = 1.0; // 11000 - 13200
            freq_weight[6] = 1.0; // 13200 - 15400
            freq_weight[7] = 1.0; // 15400 - 17600
            freq_weight[8] = 1.0; // 17600 - 19800
            freq_weight[9] = 1.0; // 19800 - 22000
        }

        private List<double> convert_sample_rate(int before_length, int after_length, List<double> v)
        {
            List<double> ret = new List<double>();

            for (int i = 0; i < after_length; ++i) {
                double idx = (double)(before_length - 1) / after_length * i;
                int a = (int)Math.Floor(idx), b = (int)Math.Ceiling(idx);
                double c = v[b] - v[a];

                ret.Add(v[a] + c * (idx - a));
            }

            return ret;
        }

        public List<double> get_noise_canceled_buffer ( int sampling_rate_env, List<double> left_env, List<double> right_env,
            int sampling_rate_source, List<double> source )
        {
            using ( var writer = new StreamWriter("output.txt", true) ) {
                for ( int i = 0; i < Math.Max(left_env.Count, Math.Max(right_env.Count, source.Count)); ++i ) {
                    writer.WriteLine(String.Format("{0} {1} {2}", (i < left_env.Count ? left_env[i] : 0.0),
                        (i < right_env.Count ? right_env[i] : 0.0),
                        (i < source.Count ? source[i] : 0.0)));
                }
                writer.WriteLine("");
            }

            List<double> ret = new List<double>();
            List<double> calced_left = apply_freq_weight(sampling_rate_env, left_env);
            List<double> calced_right = apply_freq_weight(sampling_rate_env, right_env);

            for ( int i = 0; i < source.Count; ++i ) {
                ret.Add(source[i]);
            }

            for ( int i = 0; i < left_env.Count; ++i ) {
                if (i - delay < 0 || ret.Count <= i )
                    continue;

                ret[i - delay] -= left_weight * calced_left[i];
            }

            for ( int i = 0; i < right_env.Count; ++i ) {
                if (i - delay < 0 || ret.Count <= i)
                    continue;

                ret[i - delay] -= right_weight * calced_right[i];

            }

            /*
            for ( int i = 0; i < ret.Count; ++i ) {
                ret[i] = Math.Min(1.0, Math.Max(-1.0, ret[i]));
            }
            */

            return ret;
        }

        public List<double> apply_freq_weight ( int sampling_rate, List<double> v )
        {
            Complex[] buf = prepare_complex_array(v);
            Fourier.Forward(buf, FourierOptions.Matlab);

            for ( int i = 0; i < buf.Length/2; ++i ) {
                double freq = (double)i * sampling_rate / buf.Length;
                int freq_idx = Math.Min(freq_weight.Length - 1, (int)(freq / (UPPER_FREQUENCY / freq_weight.Length)));

                buf[i] *= (float)freq_weight[freq_idx];
                buf[buf.Length-i-1] *= (float)freq_weight[freq_idx];
            }

            Fourier.Inverse(buf, FourierOptions.Matlab);
            return buf.Select(x => (double)x.Real).ToList();
        }

        private Complex[] prepare_complex_array ( List<double> v )
        {
            Complex[] ret = new Complex[v.Count];
            for (int i = 0; i < v.Count; ++i)
                ret[i] = new Complex(v[i], 0.0);
            return ret;
        }
    }
}
