using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace noise_canceler
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioInputDevice input_device;
        private AudioOutputDevice env_device;
        private WaveDataViewModel wave_data_model_input;
        private WaveDataViewModel wave_data_model_env;

        public MainWindow()
        {
            InitializeComponent();

            wave_data_model_env = new WaveDataViewModel();
            env_device = new AudioOutputDevice(wave_data_model_env, 0);
            audio_env_grid.DataContext = wave_data_model_env;

            wave_data_model_input = new WaveDataViewModel();
            input_device = new AudioInputDevice(wave_data_model_input, env_device, 0, 0);
            audo_input_grid.DataContext = wave_data_model_input;

            init_event();
        }

        private void init_event()
        {
            this.comboBox_input.SelectionChanged += (sender, e) => {
                ComboBox cur_cb = (ComboBox)sender;
                input_device = new AudioInputDevice(wave_data_model_input, env_device, cur_cb.SelectedIndex, comboBox_output.SelectedIndex);
            };

            this.comboBox_speaker.SelectionChanged += (sender, e) => {
                ComboBox cur_cb = (ComboBox)sender;
                env_device = new AudioOutputDevice(wave_data_model_env, cur_cb.SelectedIndex);
            };

            this.comboBox_output.SelectionChanged += (sender, e) => {
                ComboBox cur_cb = (ComboBox)sender;
                input_device = new AudioInputDevice(wave_data_model_input, env_device, comboBox_input.SelectedIndex, cur_cb.SelectedIndex);
            };

            this.button_start.Click += (sender, e) => {
                input_device.start();
                env_device.start();
            };

            this.button_stop.Click += (sender, e) => {
                input_device.stop();
                env_device.stop();
            };
        }
    }
}
