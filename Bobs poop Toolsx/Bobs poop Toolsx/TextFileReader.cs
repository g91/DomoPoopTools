using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Bobs_poop_Toolsx
{
    internal class TextFileReader
    {
        private string filename;
        private long length;
        private bool running;
        private Thread thread;
        private SpeechSynthesizer synth;

        public TextFileReader(string filename)
        {
            this.filename = filename;
            this.length = new FileInfo(filename).Length;
            this.running = false;
            this.synth = new SpeechSynthesizer();
            this.synth.Rate = 0;
            this.synth.Volume = 100;
        }

        public void Start()
        {
            if (this.running)
            {
                return;
            }

            this.running = true;

            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.Start();
        }

        public void Stop()
        {
            if (!this.running)
            {
                return;
            }

            this.running = false;

            if (this.thread != null)
            {
                this.thread.Join();
                this.thread = null;
            }
        }

        private void Run()
        {
            while (this.running)
            {
                long newLength = new FileInfo(this.filename).Length;

                if (newLength != this.length)
                {
                    this.length = newLength;

                    using (var reader = new StreamReader(this.filename))
                    {
                        string line = null;

                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                        }

                        int index = line.IndexOf("] ") + 2;
                        line = line.Substring(index);

                        this.synth.Speak(line);
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
