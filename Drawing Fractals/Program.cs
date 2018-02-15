using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;

namespace Drawing_Fractals
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

            int intWindowHeight = Convert.ToInt32(ConfigurationManager.AppSettings["window-height"]);
            const double goldenRatio = 1.61803398874989484820458683436;

            int intWindowWidth = (int)Math.Round(intWindowHeight / (1 / goldenRatio));


            Form myForm = new Form1
            {
                Size = new Size(intWindowWidth, intWindowHeight)
            };

            Application.Run(myForm);
        }
    }
}
