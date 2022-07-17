using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{

    static void Main(string[] args)
    {
        JApplication application = new JApplication();
        application.Init(args);
        application.UpdateLoop(); // Kind of a fake update loop
        application.DeInit();
    }


}
