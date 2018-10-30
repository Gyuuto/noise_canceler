using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace noise_canceler
{
    class MainViewModel
    {
        public List<string> AudioInputDevices { get; private set; }
        public List<string> AudioEnvironmentDevices { get; private set; }
        public List<string> AudioOutputDevices { get; private set; }

        public MainViewModel ()
        {
            init_audio_device();
        }

        private void init_audio_device ()
        {
            AudioInputDevices = AudioInputDevice.getInputDevices();
            AudioEnvironmentDevices = AudioOutputDevice.getOutputDevices();
            AudioOutputDevices = AudioInputDevice.getOutputDevices();
        }
    }
}
