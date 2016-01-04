using FileMakerXMLGeneratorApp.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileMakerXMLGeneratorApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Application.Run(new MainForm());
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            DialogResult result = MessageBox.Show(unhandledExceptionEventArgs.ExceptionObject.ToString(), @"Runtime Error", MessageBoxButtons.OK);
            if(result == DialogResult.Abort)
                Process.GetCurrentProcess().Kill();
        }
    }
}
