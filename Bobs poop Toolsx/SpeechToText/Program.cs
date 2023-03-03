using System;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;


namespace SpeechToText
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.SetInputToDefaultAudioDevice();

            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            Console.ReadLine();
        }

        static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Speech recognized: " + e.Result.Text);
        }
    }
}
