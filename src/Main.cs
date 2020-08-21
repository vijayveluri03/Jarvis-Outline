using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

class Program {

    static void Main(string[] args) {

        Jarvis.JApplication app = new Jarvis.JApplication();
        app.Initialize();

        while (true) {
            if (app.IsExitSignalRaised)
                break;
            app.Update();
        }

    }

}
