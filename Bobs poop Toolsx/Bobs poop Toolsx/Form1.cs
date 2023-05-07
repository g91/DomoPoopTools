using System;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using NLog;

using System.Threading;
using System.Windows.Forms;
using System.Text.Json;
using System.Text;

using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Factory = SharpDX.Direct2D1.Factory;
using WindowRenderTargetProperties = SharpDX.Direct2D1.RenderTargetProperties;
using System.Diagnostics;
using System.IO.Compression;
using Bobs_poop_Toolsx.Memory;
using Bobs_poop_Toolsx.Linux;
using Bobs_poop_Toolsx.Packet;

namespace Bobs_poop_Toolsx
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
        // Replace 'example.txt' with the path to your text file
        private TextFileReader reader = new TextFileReader(@"C:\\Users\\Domo\\AppData\\Roaming\\SecondLife\\theevildomo_resident\\chat.txt");

        public static IntPtr hookId = IntPtr.Zero;
        public static StreamWriter mapFile;
        public static Thread t;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [Flags]
        enum ProcessAccessFlags : uint
        {
            VirtualMemoryRead = 0x00000010,
        }

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private SshHelper _sshHelper;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                mapFile.WriteLine("sendkeys.KeyPress((Keys)" + vkCode + ")");
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public Form1()
        {
            mapFile = new StreamWriter("map.lua");
            InitializeComponent();

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(new DictationGrammar());

            Grammar grammar = new Grammar(new GrammarBuilder(new Choices(new string[] { "e 1", "e 2", "e 3", "execute fix 001", "execute fix 002", "start listening", "stop listening", "stop", "sent", "read" })));
            recognizer.LoadGrammar(grammar);

            // Set the InitialSilenceTimeout property before starting recognition
            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1);

            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
  
            // Set the initial URL to navigate to
            webView21.Source = new Uri("https://evilsource.net/community/", UriKind.Absolute);
            webView22.Source = new Uri("https://chat.openai.com/chat", UriKind.Absolute);
            webView23.Source = new Uri("https://godbolt.org/", UriKind.Absolute);

            //Thread thread2 = new Thread(() =>
            //{
            //    OverlayForm2 form = new OverlayForm2();
            //    Application.Run(form);
            //});
            //thread2.SetApartmentState(ApartmentState.STA);
            //thread2.Start();

            //Thread thread = new Thread(() =>
            //{
            //    OverlayForm form = new OverlayForm();
            //    Application.Run(form);
            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
        }

  

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "e 1":
                    for (int i = 0; i < 2; i++)
                    {
                        SendKeys.SendWait("1");
                    }
                    break;

                case "e 2":
                    for (int i = 0; i < 2; i++)
                    {
                        SendKeys.SendWait("2");
                    }
                    break;

                case "e 3":
                    for (int i = 0; i < 2; i++)
                    {
                        SendKeys.SendWait("3");
                    }
                    break;



                case "read game":
                    // Start reading the file in a separate thread
                    reader.Start();


                    textBox2.Text += "start Second Life" + Environment.NewLine;
                    Console.WriteLine(" start Second Life");
                    Console.Write("> ");
                    break;

                case "stop game1":
                    // Start reading the file in a separate thread
                    reader.Start();


                    textBox2.Text += "stop Second Life" + Environment.NewLine;
                    Console.WriteLine(" stop Second Life");
                    Console.Write("> ");
                    break;

                case "start listening":
                    isListening = true;
                    // button1.Text = "Stop Listening";
                    textBox2.Text += "Stop Listening" + Environment.NewLine;
                    Console.WriteLine(" !start listening");
                    Console.Write("> ");
                    break;
                case "execute fix 001":
                    SendKeys.Send("42{ENTER}");
                    Console.WriteLine(" !send fix 001");
                    Console.Write("> ");
                    break;
                case "execute fix 002":
                    SendKeys.Send("can you continue from where you left off with the same code syntax{ENTER}");
                    Console.WriteLine(" !send fix 002");
                    Console.Write("> ");
                    break;
                case "stop listening":
                    isListening = false;
                    Console.WriteLine(" !Stop Listening");
                    Console.Write("> ");
                    // button1.Text = "Start Listening";
                    textBox2.Text += "Start Listening" + Environment.NewLine;
                    break;
                case "read":
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event((byte)Keys.C, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                    keybd_event((byte)Keys.C, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                    string selectedText = Clipboard.GetText();
                    synth.Speak(selectedText);
                    Console.WriteLine(" !read");
                    Console.Write("> ");
                    Console.WriteLine(" !reading " + selectedText);
                    Console.Write("> ");
                    textBox2.Text += "read" + Environment.NewLine;
                    break;
                case "say":
                    isListening = true;
                   // button1.Text = "Stop Listening";
                    Console.WriteLine(" !say");
                    Console.Write("> ");
                    textBox2.Text += "say" + Environment.NewLine;
                    break;
                case "clear":
                    textBox1.Clear();
                    Console.WriteLine(" !clear");
                    Console.Write("> ");
                    textBox2.Text += "clear" + Environment.NewLine;
                    break;
                case "sent":
                    Console.WriteLine(" !sent");
                    Console.Write("> ");
                    SendKeys.Send("^v");
                    SendKeys.Send("{ENTER}");
                    textBox1.Clear();
                    break;
                case "stop":
                    Console.WriteLine(" !stop");
                    Console.Write("> ");
                    isListening = false;
                    textBox1.Clear();
                    //button1.Text = "Start Listening";
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
            AllocConsole();

            // Redirect the standard input and output streams to the console
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            // Create a new thread to run the console input loop
            _consoleThread = new Thread(ConsoleInputLoop);
            _consoleThread.Start();
        }

        private void StopConsole()
        {
            _consoleThread?.Join();
        }

        public void ListFoldersAndTxtFiles()
        {
            //// Get the path of the folder you want to search in
            //string folderPath = @"D:\Files\Documents";

            //// Get a list of all the subfolders in the folder
            //string[] subfolders = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

            //// Create a list to store the paths of all the txt files in the folder and its subfolders
            //var txtFiles = subfolders.SelectMany(subfolder => Directory.GetFiles(subfolder, "*.txt", SearchOption.TopDirectoryOnly));
            //foreach (string txtFile in txtFiles)
            //{
            //    listBox2.Items.Add(txtFile);
            //}

            //// Add an event handler to the listBox2 to display the contents of the selected txt file in the textBox3
            //listBox2.SelectedIndexChanged += (sender, args) =>
            //{
            //    string selectedItem = (string)listBox2.SelectedItem;
            //    if (selectedItem.EndsWith(".txt"))
            //    {
            //        string txtFileContents = File.ReadAllText(selectedItem);
            //        textBox3.Text = txtFileContents;
            //    }
            //    else
            //    {
            //        textBox3.Text = "";
            //    }
            //};
        }


        public void SendTcpPacket(string ipAddress, int port, byte[] data)
        {
            // Create a TCP client and connect to the specified host and port
            TcpClient client = new TcpClient();
            client.Connect(ipAddress, port);

            // Send the data to the remote host
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            // Close the connection
            client.Close();
        }

        public bool TestForBufferOverflow(string ipAddress, int port, int numAttempts, int bufferSize)
        {
            Socket socket = null;

            // Keep track of the number of attempts
            int attempts = 0;

            while (attempts < numAttempts)
            {
                try
                {
                    // Create a new socket
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    // Connect to the specified IP address and port
                    socket.Connect(ipAddress, port);

                    // Create a buffer of the specified size for this attempt
                    byte[] buffer = new byte[bufferSize];

                    // Send the buffer to the socket
                    socket.Send(buffer);

                    // Some other error occurred, increase the buffer size and try again
                    bufferSize += 1000; // Increase the buffer size by 1000 bytes for the next attempt
                    attempts++;

                    // If we didn't receive an exception, then there was no overflow
                    //return false;
                    socket.Close();
                }
                catch (SocketException ex)
                {
                    // Check if the exception was caused by a buffer overflow
                    if (ex.SocketErrorCode == SocketError.MessageSize)
                    {
                        Console.WriteLine(" - Buffer overflow detected!");
                        // A buffer overflow occurred
                        return true;
                    }
                    else
                    {
                        // Some other error occurred, increase the buffer size and try again
                        bufferSize += 1000; // Increase the buffer size by 1000 bytes for the next attempt
                        attempts++;
                    }
                }
                finally
                {
                    // Close the socket
                    socket.Close();
                }
            }

            Console.WriteLine(" -  Buffer overflow Not detected!");
            // If we reached here, there were no buffer overflow errors after the specified number of attempts
            return false;
        }

        public bool IsTcpSocketOnline(string ipAddress, int port, int timeout)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult ar = client.BeginConnect(ipAddress, port, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        Console.WriteLine(" - IsTcpSocketOnline: false");
                        return false;
                    }
                    client.EndConnect(ar);
                }
            }
            catch
            {
                Console.WriteLine(" - IsTcpSocketOnline: false");
                return false;
            }

            Console.WriteLine(" - IsTcpSocketOnline: true");
            return true;
        }

        public string BeautifyJsonFile(string filePath)
        {
            // Load the contents of the file as a JSON object
            string json = File.ReadAllText(filePath);
            dynamic obj = JsonConvert.DeserializeObject(json);

            // Serialize the JSON object with indentation to make it more readable
            string beautifiedJson = JsonConvert.SerializeObject(obj, Formatting.Indented);

            // Return the beautified JSON as a string
            return beautifiedJson;
        }

        private void ConsoleInputLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                string[] sinput =  input.Split(' ');


                //IsTcpSocketOnline 206.53.61.3 80
                if (sinput[0] == "IsTcpSocketOnline") {
                    string ipAddress = sinput[1];
                    int port = Int32.Parse(sinput[2]);
                    int timeout = 100;
                    IsTcpSocketOnline(ipAddress, port, timeout);
                }else if(sinput[0] == "TestForBufferOverflow") {
                    //TestForBufferOverflow 206.53.61.3 80 10 10000
                    string ipAddress = sinput[1];
                    int port = Int32.Parse(sinput[2]);
                    int numAttempts = Int32.Parse(sinput[3]);
                    int bufferSize = Int32.Parse(sinput[4]);
                    TestForBufferOverflow(ipAddress, port, numAttempts, bufferSize);
                }
                else if (sinput[0] == "SecondLife")
                {
                    // Start reading the file in a separate thread
                    reader.Start();
                    Console.WriteLine(" start Second Life");
                    Console.Write("> ");
                    break;
                }
                else if (sinput[0] == "StopSecondLife")
                {
                    // Start reading the file in a separate thread
                    reader.Stop();
                    Console.WriteLine(" Stop Second Life");
                    Console.Write("> ");
                    break;
                }

            }
        }

        static int FindPattern(byte[] buffer, byte[] pattern)
        {
            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }

        static int FindString(byte[] buffer, string searchString)
        {
            int searchLength = searchString.Length;
            for (int i = 0; i <= buffer.Length - searchLength; i++)
            {
                string candidate = Encoding.ASCII.GetString(buffer, i, searchLength);
                if (candidate == searchString)
                {
                    return i;
                }
            }
            return -1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartConsole();
            ListFoldersAndTxtFiles();
            textBox2.Text += "the1Domo's toolbox v2.4" + Environment.NewLine;
            Console.WriteLine(textBox2.Text);
            Console.Write("> ");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the process ID of the target process
            int processId = Process.GetProcessesByName("habbo.exe").FirstOrDefault()?.Id ?? -1;
            if (processId == -1)
            {
                Console.WriteLine("Could not find the target process.");
                return;
            }

            // Open the target process for reading its memory
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open the target process.");
                return;
            }

            // Define the byte pattern to search for in the memory
            byte[] pattern = { 0x68, 0x65, 0x6c, 0x6c, 0x6f }; // "hello" in ASCII

            // Search for the pattern in the memory of the target process
            IntPtr baseAddress = Process.GetProcessById(processId).MainModule.BaseAddress;
            IntPtr endAddress = new IntPtr((long)baseAddress + Process.GetProcessById(processId).MainModule.ModuleMemorySize);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while (baseAddress.ToInt64() < endAddress.ToInt64())
            {
                // Read a chunk of memory into the buffer
                if (!ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, out bytesRead))
                {
                    Console.WriteLine("Failed to read process memory.");
                    return;
                }

                // Search for the pattern in the buffer
                int index = FindPattern(buffer, pattern);
                if (index != -1)
                {
                    // Calculate the address of the pattern in the process memory
                    IntPtr address = new IntPtr(baseAddress.ToInt64() + index);

                    // Read a memory dump of the region containing the pattern
                    byte[] dump = new byte[1024];
                    if (!ReadProcessMemory(processHandle, address, dump, dump.Length, out bytesRead))
                    {
                        Console.WriteLine("Failed to read process memory.");
                        return;
                    }

                    // Convert the memory dump to a string representation
                    StringBuilder dumpBuilder = new StringBuilder();
                    foreach (byte b in dump)
                    {
                        dumpBuilder.AppendFormat("{0:x2} ", b);
                    }

                    // Display the memory address and the memory dump in a text box
                    Console.WriteLine("Memory address: 0x{0:x}", address.ToInt64());
                    Console.WriteLine("Memory dump: {0}", dumpBuilder.ToString());
                    return;
                }

                // Move the base address to the next chunk of memory
                baseAddress = new IntPtr(baseAddress.ToInt64() + buffer.Length);
            }

            Console.WriteLine("Could not find the pattern in the memory.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Get the process ID of the target process
            int processId = Process.GetProcessesByName("habbo").FirstOrDefault()?.Id ?? -1;
            if (processId == -1)
            {
                Console.WriteLine("Could not find the target process.");
                return;
            }

            // Open the target process for reading its memory
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open the target process.");
                return;
            }

            // Define the string to search for in the memory
            string searchString = "Hello";

            // Search for the string in the memory of the target process
            IntPtr baseAddress = Process.GetProcessById(processId).MainModule.BaseAddress;
            IntPtr endAddress = new IntPtr((long)baseAddress + Process.GetProcessById(processId).MainModule.ModuleMemorySize);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            MemorySearcher Tools = new MemorySearcher(processId);

            IntPtr test = Tools.SearchInt32(5, baseAddress, endAddress);
            Console.WriteLine("0x{0:x}", test);

            IntPtr test2 = Tools.SearchUnicodeString(searchString, baseAddress, endAddress);
            Console.WriteLine("0x{0:x}", test2);


            var results = Tools.Search_List_String(searchString, baseAddress, endAddress);
     
            // Print out the memory addresses where the value was found
            if (results.Count == 0)
            {
                Console.WriteLine("Value not found in the memory of the target process.");
            }
            else
            {
                Console.WriteLine($"Found {results.Count} instances of the value in the memory of the target process:");
                foreach (IntPtr address in results)
                {
                    Console.WriteLine($"  {address}");
                }
            }


            while (baseAddress.ToInt64() < endAddress.ToInt64())
            {
                // Read a chunk of memory into the buffer
                if (!ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, out bytesRead))
                {
                    Console.WriteLine("Failed to read process memory.");
                    return;
                }

                // Search for the string in the buffer
                int index = FindString(buffer, searchString);
                if (index != -1)
                {
                    // Calculate the address of the string in the process memory
                    IntPtr address = new IntPtr(baseAddress.ToInt64() + index);

                    // Display the memory address of the string
                    Console.WriteLine("String found at memory address: 0x{0:x}", address.ToInt64());
                }

                // Move the base address to the next chunk of memory
                baseAddress = new IntPtr(baseAddress.ToInt64() + buffer.Length);
            }

            Console.WriteLine("Memory search complete.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TcpProxyServer server = new TcpProxyServer("127.0.0.1", 8080, "206.53.61.2", 80);
            Console.WriteLine("Starting server...");
            server.Start();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}