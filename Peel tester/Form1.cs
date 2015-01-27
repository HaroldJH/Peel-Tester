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
        private int[] weightView = new int[5];
        private int[] pullSpeed = new int[3];
        private int i = 0;
        private PointPairList max = null;
        private PointPairList min = null;
        private LineItem curve = null;
        private LineItem curve1 = null;
        private int minV = 0;
        private int maxV = 95;
        private GraphPane graph = null;

        public Form1()
        {
            InitializeComponent();
            weightView[0] = 50;
            weightView[1] = 80;
            weightView[2] = 100;
            weightView[3] = 120;
            weightView[4] = 150;

            pullSpeed[0] = 200;
            pullSpeed[1] = 300;
            pullSpeed[2] = 600;

            graph = zedGraphControl1.GraphPane;
            
            // set title
            graph.Title.Text = "";

            // X Coordinate
            graph.XAxis.Title.Text = "";
            //graph.XAxis.Scale.
            graph.XAxis.Scale.MinorStep = 1.0f;
            graph.XAxis.Scale.MajorStep = 5.0f; // x-axis interval
            graph.XAxis.Scale.Min = 0.0f;
            graph.XAxis.Scale.Max = 200.0f;

            // Y Coordinate
            graph.YAxis.Title.Text = "";
            graph.YAxis.Scale.MinorStep = 1.0f;
            graph.YAxis.Scale.MajorStep = 5.0f; // y-axis interval
            graph.YAxis.Scale.Min = -5.0;
            graph.YAxis.Scale.Max = 100.0f;

            max = new PointPairList();
            min = new PointPairList();
            for (int i = 0; i < 200;i++ )
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("sin", max, Color.Red, SymbolType.None);
            curve.Line.Width = 1.0f;

            curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);
            curve1.Line.Width = 1.0f;
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
            sc.cumulativeData(bt);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Draw Graph
            double x, y = 0.0f, sign = 1;
            graph = zedGraphControl1.GraphPane;
            PointPairList list = new PointPairList();

            int pass = 0, fail = 0;
            // Set Coordinate(X,Y) - Temp
            //queue = sc.getQueue();
            for (int i = 0; i < 60; i++)
            {
                x = (double)i;
                //x = double.Parse(queue.Dequeue().ToString());
                y = Math.Sin((double)i * 0.5) * 8;
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
            if (fail > 0)
            {
                label21.Text = String.Format("{0}", "FAIL");
            }
            // Add Line To graph
            LineItem curveG = graph.AddCurve("sin", list, Color.Red, SymbolType.Circle);
            curveG.Line.Width = 1.0f;

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

        private void button4_Click(object sender, EventArgs e)
        {
            i++;
            if (i < weightView.Length)
            {
                textBox9.Text = String.Format("{0}", weightView[i]);
            }
            else
            {
                i = weightView.Length;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            maxV++;
            zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();
            
            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }
            
            curve = graph.AddCurve("sin", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            maxV--;
            zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("sin", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            minV++;
            zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("sin", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            minV--;
            zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("sin", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            i--;
            if (i >= 0)
            {
                textBox9.Text = String.Format("{0}", weightView[i]);
            }
            else
            {
                i = 0;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            i++;
            if (i < pullSpeed.Length)
            {
                textBox8.Text = String.Format("{0}", pullSpeed[i]);
            }
            else
            {
                i = pullSpeed.Length;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            i--;
            if (i >= 0)
            {
                textBox8.Text = String.Format("{0}", pullSpeed[i]);
            }
            else
            {
                i = 0;
            }
        }
    }
}
