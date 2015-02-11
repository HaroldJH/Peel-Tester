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
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Peel_tester
{
    public partial class Form1 : Form
    {
        private static SerialPort sp;
        private static SerialCommProcess sc;
        private static Queue queue;
        private static Queue queue1;
        private static Queue queue2;
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
        private static String flag = "NR";
        private static List<String> reiceivedString;
        private static Timer timer;
        private static String reicevedData = "";
        private static double CAL00 = 140;
        private static double CAL50 = 650;
        private static Stopwatch sw;
        private static int fl = 1;

        public Form1()
        {
            reiceivedString = new List<string>();

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
            sw = new Stopwatch();

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

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(sendHeartBeat);
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
                    queue1 = new Queue();
                    queue2 = new Queue();
                    //byte[] bytes = Rs232Utils.ByteArrayToHexString;
                    //timer.Start();
                    startCommSend();
                    
                   // startDataSendReiceve();
                    Console.WriteLine("CONNECTED");
                }
                catch(SystemException exception)
                {
                    // 시리얼포트 open실패시 예외처리
                    //throw new se
                }

                // 연결성공시 데이터를 수신한다.
                //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            }
        }

        public void startDataSendReiceve()
        {
            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived1);
        }

        public void startCommSend()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATZ");
            sp.Write(bytes, 0, bytes.Length);

            // Handler
            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            wait(1);

            bytes = Encoding.UTF8.GetBytes("ATR CAL SEL");
            sp.Write(bytes, 0, bytes.Length);

            // Calibration 기준
          //  sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            wait(2);

            bytes = Encoding.UTF8.GetBytes("OK");
            sp.Write(bytes, 0, bytes.Length);

            if (reiceivedString[1].Equals("CAL SEL 0"))
            {
                // Message "Need Calibration value"

            }
            else
            {
                bytes = Encoding.UTF8.GetBytes("ATR CAL 000");
                sp.Write(bytes, 0, bytes.Length);

                // Calibration value 
                // sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

                wait(3);

                bytes = Encoding.UTF8.GetBytes("OK");
                sp.Write(bytes, 0, bytes.Length);

                if (!reiceivedString[2].Equals("CAL 000 40"))
                {
                    // Need cali. val. 0
                }
                else
                {
                    bytes = Encoding.UTF8.GetBytes("ATR CAL 020");
                    sp.Write(bytes, 0, bytes.Length);

                    // Calibration value 
                    // sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

                    wait(4);

                    bytes = Encoding.UTF8.GetBytes("OK");
                    sp.Write(bytes, 0, bytes.Length);

                    bytes = Encoding.UTF8.GetBytes("ATR CAL 050");
                    sp.Write(bytes, 0, bytes.Length);

                    // Calibration value 
                    // sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

                    wait(5);

                    bytes = Encoding.UTF8.GetBytes("OK");
                    sp.Write(bytes, 0, bytes.Length);

                    if (!reiceivedString[4].Equals("CAL 050 500"))
                    {
                        // Need Cali. Val. 50
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes("ATR CAL 100");
                        sp.Write(bytes, 0, bytes.Length);

                        wait(6);

                        bytes = Encoding.UTF8.GetBytes("OK");
                        sp.Write(bytes, 0, bytes.Length);

                        reiceivedString.RemoveRange(0, reiceivedString.Count);
                    }
                }
            }
        }

        public void startMeasure()
        {
            fl = 2;
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
        }

        public void reqtHost()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATS X01 10");
            sp.Write(bytes, 0, bytes.Length);

            wait(1);

            // Confirm Received data
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            bytes = Encoding.UTF8.GetBytes("ATS X02 200");
            sp.Write(bytes, 0, bytes.Length);

            wait(2);
            // Confirm Received data
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            // Min. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MIN {0}", textBox2.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(3);
            // Confirm Received data
           //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            // Max. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MAX {0}", textBox1.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(4);
            // Confirm Received data
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            // Speed
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS SPD {0}", textBox8.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(5);
            // Confirm Received data
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            // Start
            bytes = Encoding.UTF8.GetBytes("ATC SRT");
            sp.Write(bytes, 0, bytes.Length);
            wait(6);

            timer.Stop();
            fl = 2;
        }

        public void sendHeartBeat(object sender, EventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATA");
            sp.Write(bytes, 0, bytes.Length);

            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            while (true)
            {
                sw.Start();
                Delay(100);

                if (sw.Elapsed.TotalSeconds >= 5)
                {
                    // No Anwser
                    sw.Stop();
                    break;
                }
                if (reicevedData.Equals("OK"))
                {
                    sw.Stop();
                    break;
                }
            }
        }

        public void reqtClient()
        {

        }

        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(fl == 1)
            {
               
                Console.WriteLine("수신");
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                reiceivedString.Add(Encoding.Default.GetString(bytes));
                Console.WriteLine("size : " + size);       
            }
            else if(fl == 2)
            {
                Console.WriteLine("수신1");
                int size = sp.BytesToRead;
                Console.Write("크기 " + size);
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                String temp = Encoding.Default.GetString(bytes);
                Console.Write("전달값 : " + temp);
                String[] data = temp.Split(new String[] { " " }, StringSplitOptions.None);

                double calcValue = ((CAL50 - CAL00) / 500) * (double.Parse(data[2]) - CAL00);
                queue.Enqueue(int.Parse(data[0]));
                queue1.Enqueue(int.Parse(data[1]));

                queue2.Enqueue(calcValue);
            }
            else if(fl == 3)
            {
                Console.WriteLine("수신reiceivedString");
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                Encoding.Default.GetString(bytes);
                Console.WriteLine("size : " + size);
                reicevedData = Encoding.Default.GetString(bytes);

                if (!reicevedData.Equals("OK"))
                {
                    // Disconnected!
                    sp.Close();
                }
            }

            if(fl == 4)
            {
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                Encoding.Default.GetString(bytes);
                Console.WriteLine("size : " + size);
                reiceivedString.Add(Encoding.Default.GetString(bytes));
            }
            //sc.cumulativeData(bt);
        }

        private static void dataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            
           
            //sc.cumulativeData(bt);
        }

        private static void dataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
           
        }

        private void wait(int i)
        {
            while (true)
            {
                Delay(100);
                if (reiceivedString.Count == i)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Delay 함수 MS
        /// </summary>
        /// <param name="MS">(단위 : MS)
        /// 
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!sp.IsOpen)
            {
                // 연결상태확인 메시지 출력
            }
            else
            {
                // Draw Graph
                double x, y = 0.0f, sign = 1;
                graph = zedGraphControl1.GraphPane;
                PointPairList list = new PointPairList();

                int pass = 0, fail = 0;
                double sum = 0f, CP = 0f, CPK = 0f, USL = 0f, LSL = 0f, SD = 0, k = 0f, avg = 0f;
                // Set Coordinate(X,Y) - Temp
                //queue = sc.getQueue();
                for (int i = 0; i < queue.Count; i++)
                {
                    //x = (double)i;
                    x = double.Parse(queue1.Dequeue().ToString());
                    //y = Math.Sin((double)i * 0.5) * 8;
                    y = double.Parse(queue2.Dequeue().ToString());

                    Console.Write("X : " + x);
                    Console.Write("Y : " + y);
                    sum += y;
                    //y = i * sign;
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

                // calc. Avg.
                label21.Text = String.Format("{0}", avg);

                CP = (USL / LSL) / SD;
                label29.Text = String.Format("{0}", CP);

                k = ((USL + LSL) / (2 - avg)) / ((USL - LSL) / 2);
                CPK = (1-k) * CP;
                label29.Text = String.Format("{0}", CP);

                // Available Save file
                //zedGraphControl1.MasterPane.GetImage().Save("graph.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void printGraph(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(zedGraphControl1.MasterPane.GetImage(), 400, 400);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Disconnect
            if (sp.IsOpen)
            {
                timer.Stop();
                sp.Close();
            }
            else
            {
                // 이미 연결해제되었음.
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
            //curve1 = graph.AddCurve("sin", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            //curve1.Line.Width = 1.0f;

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

        private void button14_Click(object sender, EventArgs e)
        {
            fl = 4;
            reiceivedString.RemoveRange(0, reiceivedString.Count);
            reqtHost();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument print = new PrintDocument();
            PageSettings ps = new PageSettings();

            ps.Margins = new Margins(10, 10, 10, 10);
            print.DefaultPageSettings = ps;

            PrintPreviewDialog pd = new PrintPreviewDialog();
            pd.ClientSize = new System.Drawing.Size(500, 500);
            pd.UseAntiAlias = true;
            print.PrintPage += new PrintPageEventHandler(printGraph);
            pd.Document = print;
            pd.Show();
        }

    }
}
