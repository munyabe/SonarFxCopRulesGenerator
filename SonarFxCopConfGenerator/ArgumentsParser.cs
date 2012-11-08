using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SonarFxCopConfGenerator
{
    class ArgumentsParser
    {
        internal ArgumentsParser(IEnumerable<string> arguments)
        {
            int argumentSequenceNumber = 0;
            foreach (string argument in arguments)
            {
                switch (argument)
                {
                    case "-isMicrosoft":
                    case "-m":
                        IsMicrosoft = true;
                        break;
                    default:
                        if (argumentSequenceNumber == 0)
                        {
                            SelectedAssemblies = GetAssemblyNames(argument);
                            argumentSequenceNumber++;
                        }
                        else if (argumentSequenceNumber == 1)
                        {
                            OutputPath = argument;
                        }
                        else
                        {
                            ShowHelp();
                        }
                        break;
                }
            }
            if (string.IsNullOrEmpty(OutputPath) || !SelectedAssemblies.Any())
            {
                ShowHelp();
                return;
            }
            AreArgumentsValid = true;
        }

        private static IEnumerable<string> GetAssemblyNames(string path)
        {
            if (File.Exists(path))
            {
                return new[] { path };
            }
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            }
            return Enumerable.Empty<string>();
        }


        internal void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} <input file/folder> <output file> [-isMicrosoft|-m]", AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine("  -isMicrosoft|-m:      Use this flag if you generate the configuration file from Microsoft's assemblies. Leave it if you generate from your custom assemblies");
        }

        internal bool IsMicrosoft { get; private set; }
        internal IEnumerable<string> SelectedAssemblies { get; private set; }
        internal string OutputPath { get; private set; }
        internal bool AreArgumentsValid { get; private set; }
    }
}
