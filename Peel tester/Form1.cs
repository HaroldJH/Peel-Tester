using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using ZedGraph;

namespace Peel_tester
{
    public partial class Form1 : Form
    {
        private static SerialPort sp;
        private static SerialCommProcess sc;
        private Queue queue;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sc = new SerialCommProcess();
            sp = sc.configureSerialPort();

            if(sp.IsOpen)
            {
                //sp.Close();
            }
            else
            {
                try
                {
                    sp.Open();
                    queue = sc.getQueue();
                    Console.WriteLine("CONNECTED");
                }
                catch(SystemException exception)
                {
                    // 시리얼포트 open실패시 예외처리
                    //throw new se
                }

                // 연결성공시 데이터를 수신한다.
                sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            }
        }

        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte bt = (byte)sp.ReadByte();
            Console.WriteLine(bt);
            //sc.cumulativeData(bt);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Draw Graph
            double x, y = 0.0f, sign = 1;
            GraphPane graph = zedGraphControl1.GraphPane;
            PointPairList list = new PointPairList();

            int pass = 0, fail = 0;
            // Set Coordinate(X,Y) - Temp
            queue = sc.getQueue();
            for (int i = 0; i < queue.Count; i++)
            {
                x = (double)i;
               // x = double.Parse(queue.Dequeue().ToString());
                //y = Math.Sin((double)i * 0.5) * 8;
                y = i * sign;
                list.Add(x, y);
                if (i % 2 == 0)
                {
                    //sign = -1;
                }
                else
                    sign = 1;

                if (textBox1.Text != "" && textBox2.Text != "")
                {
                    // Max Val. Check.
                    if (y > double.Parse(textBox1.Text))
                    {
                        // Increase fail count
                        fail++;
                    }
                    else
                    {
                        // Increase pass count
                        pass++;
                    }
                    // Min Val. Check
                    if (y > double.Parse(textBox2.Text))
                    {
                        // Increase fail count
                        fail++;
                    }
                    else
                    {
                        // Increase pass count
                        pass++;
                    }
                }
            }
            label21.Text = String.Format("{0}", fail);

            // Add Line To graph
            LineItem curve = graph.AddCurve("sin", list, Color.Red, SymbolType.Circle);
            curve.Line.Width = 1.0f;

            // Draw
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
            
            // Available Save file
            zedGraphControl1.MasterPane.GetImage().Save("graph.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Disconnect
            if (sp.IsOpen)
            {
                sp.Close();
            }
        }
    }
}
