using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

public class ICommand
{
    public virtual bool Run(List<string> arguments, List<string> optionalArguments, Jarvis.JApplication application )
    {
        Console.Out.WriteLine("Incorrect usage");
        return false;
    }
}
