using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Peel_tester
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
            Application.Run(new Form1());
            
            GraphPane graph = new GraphPane();

            // set title
            graph.Title.Text = "";

            // X Coordinate
            graph.XAxis.Title.Text = "";
            graph.XAxis.Scale.MinorStep = 1.0f;
            graph.XAxis.Scale.MajorStep = 50.0f; // x-axis interval
            graph.XAxis.Scale.Min = 0.0f;
            graph.XAxis.Scale.Max = 200.0f;

            // Y Coordinate
            graph.YAxis.Title.Text = "";
            graph.YAxis.Scale.MinorStep = 1.0f;
            graph.YAxis.Scale.MajorStep = 4.0f; // y-axis interval
            graph.YAxis.Scale.Min = 8.0f;
            graph.YAxis.Scale.Max = 108.0f;

            //Queue queue = new Queue();
        }
    }
}
