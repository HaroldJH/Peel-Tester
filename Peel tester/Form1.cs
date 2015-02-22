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
        private SerialPort sp;
        private SerialCommProcess sc;
        private Queue<String> queue;
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
        private String flag = "NR";
        private List<String> reiceivedString;
        private Timer timer;
        private Timer timer2;
        private String reicevedData = "";
        private double CAL00 = 140;
        private double CAL50 = 650;
        private Stopwatch sw;
        private int fl = 1;
        private int hb = 0;
        private List<int> seq;
        private List<double> x;
        private List<double> y;
        private PointPairList list;
        private delegate void draw();
        private int state = 0;
        public Form1()
        {
            reiceivedString = new List<string>();
            
            queue = new Queue<String>();
            seq = new List<int>();
            x = new List<double>();
            y = new List<double>();
            list = new PointPairList();

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

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);
            curve.Line.Width = 1.0f;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(sendHeartBeat);

            timer2 = new Timer();
            timer2.Interval = 1000;
            timer2.Tick += new EventHandler(stateCheck);
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
                    
                    Console.WriteLine("CONNECTED");
                    state = 1;

                    // Send heart beat
                    reicevedData = "OK";
                    //timer.Start();
                    //timer2.Start();
                    startCommSend();
                }
                catch(SystemException exception)
                {
                    // 시리얼포트 open실패시 예외처리
                    //throw new se
                    
                    MessageBox.Show("연결 상태를 확인하십시오");
                }

                // 연결성공시 데이터를 수신한다.
                //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            }
        }

        private void stateCheck(object sender, EventArgs e)
        {
            if(state == 0)
            {
                MessageBox.Show("응답이 없습니다. 단말기를 확인하십시오.");
                sp.Close();
                timer.Stop();
                timer2.Stop();
            }
        }
        /*
        public void startDataSendReiceve()
        {
            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
        }
        */
        public void startCommSend()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATZ\n");
            sp.Write(bytes, 0, bytes.Length);

            // Handler
            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            wait(1);
            Console.WriteLine("version : " + reiceivedString[0]);
            bytes = Encoding.UTF8.GetBytes("ATR CAL SEL\n");
            sp.Write(bytes, 0, bytes.Length);

            wait(2);
            bytes = Encoding.UTF8.GetBytes("OK\n");
            sp.Write(bytes, 0, bytes.Length);

            if (reiceivedString[1].Equals("CAL SEL 000\n"))
            {
                // Message "Need Calibration value"
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes("ATR CAL 000\n");
                sp.Write(bytes, 0, bytes.Length);

                wait(3);

                bytes = Encoding.UTF8.GetBytes("OK\n");
                sp.Write(bytes, 0, bytes.Length);
                Console.WriteLine("CALI " + reiceivedString[1]);
                if (reiceivedString[1].Equals("CAL SEL 1"))
                {
                    bytes = Encoding.UTF8.GetBytes("ATR CAL 020\n");
                }
                else if (reiceivedString[1].Equals("CAL SEL 2"))
                {
                    bytes = Encoding.UTF8.GetBytes("ATR CAL 050\n");
                }
                else if (reiceivedString[1].Equals("CAL SEL 3"))
                {
                    bytes = Encoding.UTF8.GetBytes("ATR CAL 100\n");
                }
                sp.Write(bytes, 0, bytes.Length);
                wait(4);
                bytes = Encoding.UTF8.GetBytes("OK\n");
                sp.Write(bytes, 0, bytes.Length);
            }
        }

        public void startMeasure()
        {
            fl = 2;
         }

        public void reqtHost()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATS X01 10\n");
            sp.Write(bytes, 0, bytes.Length);

            reiceivedString.RemoveRange(0, reiceivedString.Count);
            wait(1);

            bytes = Encoding.UTF8.GetBytes("ATS X02 200\n");
            sp.Write(bytes, 0, bytes.Length);

            wait(2);
            
            // Min. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MIN {0}\n", textBox2.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(3);
           
            // Max. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MAX {0}\n", textBox1.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(4);
           
            // Speed
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS SPD {0}\n", textBox8.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(5);
           
            // Start
            bytes = Encoding.UTF8.GetBytes("ATC SRT\n");
            sp.Write(bytes, 0, bytes.Length);
            wait(6);

            queue.Clear();
            timer.Stop();
            fl = 2;
        }

        public void sendHeartBeat(object sender, EventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATA");
            int tempFl = fl;
     
            fl = 3;
            switch(hb)
            {
                // If hb is 0, send "ATA"
                case 0:
                    {
                        if(reicevedData.Equals("OK"))
                        {
                            sp.Write(bytes, 0, bytes.Length);
                            reicevedData = "";
                        }
                        break;
                    }

                // If hb isn't 0, Not work
                default :
                    break;
            }
            
            while (true)
            {
                sw.Start();
                Delay(100);

                if (sw.Elapsed.TotalSeconds >= 5)
                {
                    // No Anwser
                    sw.Stop();
                    sp.Close();
                    //MessageBox.Show("응답이 없습니다. 단말기 상태를 확인하여주십시오.");
                    //timer.Stop();
                    state = 0;
                    break;
                }
                if (reicevedData.Equals("OK"))
                {
                    sw.Stop();
                    break;
                }
            }
            fl = tempFl;
        }

        public void reqtClient()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATS X01 10\n");
            sp.Write(bytes, 0, bytes.Length);

            wait(1);

            bytes = Encoding.UTF8.GetBytes("ATS X02 200\n");
            sp.Write(bytes, 0, bytes.Length);

            wait(2);

            bytes = Encoding.UTF8.GetBytes("ATS X01 10\n");
            sp.Write(bytes, 0, bytes.Length);

            wait(3);

            // Min. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MIN {0}\n", textBox2.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(4);

            // Max. Value
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS MAX {0}\n", textBox1.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(5);

            // Speed
            bytes = Encoding.UTF8.GetBytes(String.Format("ATS SPD {0}\n", textBox8.Text));
            sp.Write(bytes, 0, bytes.Length);

            wait(6);

            bytes = Encoding.UTF8.GetBytes("ATC SRT\n");
            sp.Write(bytes, 0, bytes.Length);

            fl = 2;
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("fl = " + fl);
            if(fl == 1)
            {
               // start data reiceive
                Console.WriteLine("수신");
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);
                
                String recStr = Encoding.Default.GetString(bytes);
                String tempStr = orgData(recStr);
                /* 설정값.... */
                Console.WriteLine("DATA IS " + tempStr);
                if(!tempStr.Equals(""))
                {
                    reiceivedString.Add(tempStr);
                }
                if (tempStr.Equals("BTN START"))
                {
                    reqtClient();
                }
            }
            else if(fl == 2)
            {
                // start measure
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                String recStr = Encoding.Default.GetString(bytes);
                String tempStr = orgData(recStr);
                Console.WriteLine("값 : " + tempStr);
                if (!tempStr.Equals("BTN PAUSE") && !tempStr.Equals("") && !tempStr.Equals("BTN RESUME") && !tempStr.Equals("BTN RESET") && !tempStr.Equals("END"))
                {
                    String[] data = tempStr.Split(new String[] { " " }, StringSplitOptions.None);

                    
                    double calcValue = ((CAL50 - CAL00) / 500) * (double.Parse(data[2]) - CAL00);
                    seq.Add(int.Parse(data[0]));
                    x.Add(int.Parse(data[1])/10.0f);
                    y.Add(calcValue);
                    //drawing();
                    this.Invoke(new draw(drawing), null);
                }

                else if(tempStr.Equals("END"))
                {
                    bytes = Encoding.UTF8.GetBytes("OK");
                    sp.Write(bytes, 0, bytes.Length);

                    bytes = Encoding.UTF8.GetBytes("ATD MIN 155");
                    sp.Write(bytes, 0, bytes.Length);

                    bytes = Encoding.UTF8.GetBytes("ATD MAX 2051");
                    sp.Write(bytes, 0, bytes.Length);

                    bytes = Encoding.UTF8.GetBytes("ATD AVG 1052");
                    sp.Write(bytes, 0, bytes.Length);

                    bytes = Encoding.UTF8.GetBytes("ATD STD 101");
                    sp.Write(bytes, 0, bytes.Length);
                }
            }
            else if(fl == 3)
            {
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                String recStr = Encoding.Default.GetString(bytes);
                reicevedData = orgData(recStr);
            }

            if(fl == 4)
            {
                // Request Host
                int size = sp.BytesToRead;
                byte[] bytes = new byte[size];
                sp.Read(bytes, 0, size);

                String recStr = Encoding.Default.GetString(bytes);
                String tempStr = orgData(recStr);

                reiceivedString.Add(Encoding.Default.GetString(bytes));
            }
        }

        private String orgData(String recStr)
        {
            Console.WriteLine("PARAMETER : "+recStr);
            String tempStr = "";
            if (recStr.IndexOf("\n") == -1)
            {
                // '\n'이 수신될 때까지 큐에 누적한다.
                Console.WriteLine("index : " +recStr.IndexOf("\n"));
                queue.Enqueue(recStr);
            }
            else
            {
                // '\n'이 수신된경우 \n이전 이전문자열은 큐에서 꺼낸 기존문자열과 조합. 이후문자열은 다시 큐에 누적.
                String[] tempStrArr = recStr.Split(new String[] { "\n" }, StringSplitOptions.None);
               
                for (int i = 0; i < queue.Count; i++)
                {
                    Console.WriteLine("CNT : " + queue.Count);
                    String test = queue.Dequeue();
                    tempStr += test;
                    Console.WriteLine("test value : " + test);
                }
                tempStr += tempStrArr[0];
                if (tempStrArr[1].IndexOf("\n") != tempStrArr[1].Count() - 1 && tempStrArr[1].IndexOf("\n") == -1 && tempStrArr[1].Count() != 14)
                {
                    queue.Enqueue(tempStrArr[1]);
                }
                else if(tempStrArr[1].IndexOf("\n") != -1)
                {
                    //tempStr += "\n"+tempStrArr[1];
                }
            }
            Console.WriteLine("return Value : " + tempStr);
            return tempStr;
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
        private DateTime Delay(int MS)
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
            }
        }

        private void drawing()
        {
            list.Clear();
            //zedGraphControl1.GraphPane.CurveList.Clear();
            
            // Draw Graph
            double x, y = 0.0f;
            graph = zedGraphControl1.GraphPane;
      
            int pass = 0, fail = 0;
            double sum = 0f, CP = 0f, CPK = 0f, USL = 0f, LSL = 0f, SD = 0, k = 0f, avg = 0f;
            
            for (int i = 0; i < seq.Count; i++)
            {
                x = this.x[i];
                y = this.y[i];

                sum += y;
                list.Add(x, y);
                
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
            LineItem curveG = graph.AddCurve("", list, Color.Red, SymbolType.None);
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
            CPK = (1 - k) * CP;
            label29.Text = String.Format("{0}", CP);

            // Available Save file
            //zedGraphControl1.MasterPane.GetImage().Save("graph.png", System.Drawing.Imaging.ImageFormat.Png);
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
            textBox1.Text = String.Format("{0}", maxV);
            zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();
            
            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }
            
            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            maxV--;
            textBox1.Text = String.Format("{0}", maxV);
            zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            minV++;
            textBox2.Text = String.Format("{0}", minV);
            zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;
            curve1.Line.Width = 1.0f;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            minV--;
            textBox2.Text = String.Format("{0}", minV);
            zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

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
            queue.Clear();
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
