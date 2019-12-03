using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Node");


            var process = new Process();
            process.StartInfo.FileName = "node";
            process.StartInfo.Arguments = "hello";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = new DirectoryInfo(typeof(Program).Assembly.Location).Parent.Parent.Parent.FullName + "\\Node";

            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();

            var bt = Encoding.UTF8.GetBytes("Aqui é o .Net");
            process.StandardInput.BaseStream.Write(bt, 0, bt.Length);
            process.StandardInput.WriteLine();

            process.WaitForExit();

            Console.WriteLine("Node finalizado");
            Console.ReadLine();
        }
        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            var process = (Process)sender;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Data);
            Console.ResetColor();

            if (e.Data.StartsWith("Oi"))
            {
                Console.WriteLine("Enviando primeira soma: 1 + 1");
                process.StandardInput.WriteLine("SUM 1,1");
            }
            else if (int.TryParse(e.Data, out int sumResult))
            {
                Console.WriteLine($"Resultado da soma anterior: {sumResult}");
                if (sumResult < 1000)
                {
                    Console.WriteLine($"Enviando soma: {sumResult} + {sumResult}");
                    process.StandardInput.WriteLine($"SUM {sumResult},{sumResult}");
                }
                else
                {
                    process.Kill();
                }
            }
        }
    }
}
