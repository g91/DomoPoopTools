using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using NLog;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace net4Tools
{
    public partial class Form1 : Form
    {
        private readonly SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();
        private bool isListening = false;
        private readonly List<string> clipboardHistory = new List<string>();
        private DateTime lastRecognized = DateTime.Now;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Thread _consoleThread;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;

        public Form1()
        {
            InitializeComponent();

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(new DictationGrammar());

            Grammar grammar = new Grammar(new GrammarBuilder(new Choices(new string[] { "start listening", "stop listening", "stop", "sent", "read" })));
            recognizer.LoadGrammar(grammar);

            // Set the InitialSilenceTimeout property before starting recognition
            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1);

            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            // Set the initial URL to navigate to
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://evilsource.net/community/");

            //Thread.Sleep(100);
           // StartConsole();
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "start listening":
                    isListening = true;
                    button1.Text = "Stop Listening";
                    textBox2.Text += "Stop Listening" + Environment.NewLine;
                    break;
                case "stop listening":
                    isListening = false;
                    button1.Text = "Start Listening";
                    textBox2.Text += "Start Listening" + Environment.NewLine;
                    break;
                case "read":
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event((byte)Keys.C, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                    keybd_event((byte)Keys.C, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                    string selectedText = Clipboard.GetText();
                    synth.Speak(selectedText);
                    textBox2.Text += "read" + Environment.NewLine;
                    break;
                case "say":
                    isListening = true;
                    button1.Text = "Stop Listening";
                    Console.Write("Stop Listening");
                    textBox2.Text += "say" + Environment.NewLine;
                    break;
                case "clear":
                    textBox1.Clear();
                    Console.Write("clear");
                    textBox2.Text += "clear" + Environment.NewLine;
                    break;
                case "sent":
                    Console.Write("sent");
                    SendKeys.Send("^v");
                    SendKeys.Send("{ENTER}");
                    textBox1.Clear();
                    break;
                case "stop":
                    Console.Write("stop");
                    isListening = false;
                    textBox1.Clear();
                    button1.Text = "Start Listening";
                    textBox2.Text += "stop" + Environment.NewLine;
                    break;
                default:
                    if (isListening)
                    {
                        textBox1.AppendText(e.Result.Text + Environment.NewLine);
                        listBox1.Items.Insert(0, e.Result.Text);
                        using (System.IO.StreamWriter writer = new System.IO.StreamWriter("log.txt", true))
                        {
                            writer.WriteLine(e.Result.Text);
                        }
                        Clipboard.SetText(textBox1.Text);
                    }
                    break;
            }
        }

        private void StartConsole()
        {
            _consoleThread = new Thread(ConsoleLoop);
            _consoleThread.Start();
        }

        private void StopConsole()
        {
            _consoleThread?.Join();
        }




        public void CreateConsole()
        {
            AllocConsole();


            // Redirect the standard input and output streams to the console
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            // Start the console input loop
          //  ConsoleInputLoop();


            Console.WriteLine("Hello, console!");
        }

        private void ConsoleLoop()
        {
            Console.Write("the1Domo's toolbox V2.1");
            Thread.Sleep(10000);
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                // Handle the input
                // ...

                if (input.ToLower() == "exit")
                {
                    break;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateConsole();
            textBox2.Text += "the1Domo's toolbox" + Environment.NewLine;
            textBox2.Text += "v2.1" + Environment.NewLine;
            Console.Write("the1Domo's toolbox V2.1");
            try
            {
                if (System.IO.File.Exists("log.txt"))
                {
                    string[] lines = System.IO.File.ReadAllLines("log.txt");
                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        listBox1.Items.Insert(0, lines[i].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isListening)
            {
                recognizer.RecognizeAsyncStop();
                isListening = false;
                button1.Text = "Start Listening";
                textBox1.Clear();
            }
            else
            {
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
                isListening = true;
                button1.Text = "Stop Listening";
                textBox1.Clear();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        { 

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedText = "";
            foreach (var item in listBox1.SelectedItems)
            {
                selectedText += item.ToString() + Environment.NewLine;
            }
            Clipboard.SetText(selectedText);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string selectedText = Clipboard.GetText();
            synth.Speak(selectedText);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }



        private void myconsoleapp(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("myconsoleapp.exe");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            Process p = new Process();
            p.StartInfo = psi;
            p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            p.Start();

            p.BeginOutputReadLine();
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }
    }
}
