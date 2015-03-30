using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using ZedGraph;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

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
        private PointPairList x1 = null;
        private PointPairList x2 = null;
        private PointPairList xv = null;
        private PointPairList yv = null;
        private LineItem curve = null;
        private LineItem curve1 = null;
        private LineItem curve2 = null;
        private LineItem curve3 = null;
        private int minV = 0;
        private int maxV = 95;
        private int x1V = 0;
        private int x2V = 95;
        private GraphPane graph = null;
        private String flag = "NR";
        private List<String> reiceivedString;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.Timer timer5;
        private System.Windows.Forms.Timer timer6;
        private String reicevedData = "";
        private double CAL00 = 140;
        private double CAL50 = 650;
        private Stopwatch sw;
        private int fl = 1;
        private int hb = 0;
        private List<String> seq;
        private List<double> x;
        private List<double> y;
        private PointPairList list;
        private delegate void draw();
        private delegate void drawR(double avg, double std, double min, double max, String resultF);
        private int state = 0;
        private double avg = 0f;
        private double sum = 0f;
        private double std = 0f;
        private int pass = 0, fail = 0;
        private String resultF = "";
        private double rMin = 0f, rMax = 0f;
        private String tempStr = "";
        private String fileFlag = "";
        private Thread thread;
        private Thread thread2;
        private int state1 = 0;
        private double calVal = 0f;
        private float percent = 0f;
        private int fl1 = 1;
        private int width = 0;
        private int height = 0;
        private int sts = 0;
        private double cali = 0f;
        private double cali00 = 0f;
        private double calib = 0f;
        private int imsi = 0;
        private int thStat = 0;
        private int tStat = 0;
        private List<int> inclination;
        private List<double> maxY;
        private List<double> minY;
        private List<double> maxX;
        private List<double> minX;
        private int f = 0;

        public Form1()
        {
            reiceivedString = new List<string>();

            queue = new Queue<String>();
            seq = new List<String>();
            x = new List<double>();
            y = new List<double>();
            list = new PointPairList();
            inclination = new List<int>();

            maxY = new List<double>();
            minY = new List<double>();
            maxX = new List<double>();
            minX = new List<double>();

            max = new PointPairList();
            min = new PointPairList();

            x1 = new PointPairList();
            x2 = new PointPairList();

            xv = new PointPairList();
            yv = new PointPairList();

            InitializeComponent();

            graph = zedGraphControl1.GraphPane;
            //zedGraphControl1.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(zedGraphControlPointEvent);
            zedGraphControl1.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(zedGraphControlPointEvent);
            //zedGraphControl1.MouseUp += new ZedGraph.ZedGraphControl.PointValueHandler(zedGraphControlPointEvent);
            zedGraphControl1.IsShowPointValues = true;

            FileProcess fp = new FileProcess();
            String readStr = fp.read(Directory.GetCurrentDirectory() + "/userInfo/userInfo.csv");

            comboBox1.Items.Add("50");
            comboBox1.Items.Add("80");
            comboBox1.Items.Add("100");
            comboBox1.Items.Add("120");
            comboBox1.Items.Add("150");
            comboBox1.Items.Add("200");

            comboBox2.Items.Add("200");
            comboBox2.Items.Add("300");
            comboBox2.Items.Add("600");

            Console.WriteLine(SerialPort.GetPortNames().Length);
            foreach (string port in SerialPort.GetPortNames())
            {
                comboBox3.Items.Add(port);
            }

            if (SerialPort.GetPortNames().Length != 0)
            {
                comboBox3.SelectedIndex = 0;
            }

            if (!readStr.Equals(""))
            {
                String[] str = readStr.Split(new String[] { "," }, StringSplitOptions.None);
                if (str[0].Equals("CHECKED"))
                {
                    checkBox1.Checked = true;

                    textBox3.Text = str[1];
                    textBox4.Text = str[2];
                    textBox5.Text = str[3];
                    textBox6.Text = str[4];
                    textBox7.Text = str[5];
                    textBox16.Text = str[6];
                    textBox10.Text = str[7];
                    textBox11.Text = str[8];
                    textBox12.Text = str[9];
                    textBox13.Text = str[10];
                    textBox14.Text = str[11];
                    textBox15.Text = str[12];
                    numericUpDown1.Text = str[13];
                    numericUpDown2.Text = str[14];
                    numericUpDown3.Text = str[15];
                    numericUpDown4.Text = str[16];

                    comboBox2.Text = str[17];
                    comboBox1.Text = str[18];
                }
            }

            weightView[0] = 50;
            weightView[1] = 80;
            weightView[2] = 100;
            weightView[3] = 120;
            weightView[4] = 150;

            pullSpeed[0] = 200;
            pullSpeed[1] = 300;
            pullSpeed[2] = 600;

            zedGraphControl1.IsEnableZoom = false;
            sw = new Stopwatch();

            Width = ClientSize.Width;
            height = ClientSize.Height;

            //zedGraphControl1.Location();
            // set title
            graph.Title.Text = "";

            // X Coordinate
            graph.XAxis.Title.Text = "Distance(mm)";
            //graph.XAxis.Scale.
            graph.XAxis.Scale.MinorStep = 1.0f;
            graph.XAxis.Scale.MajorStep = 5.0; // x-axis interval
            graph.XAxis.Scale.Min = 0.0f;
            graph.XAxis.Scale.Max = 200.0f;
            graph.XAxis.MajorGrid.IsVisible = true;
            graph.XAxis.MajorGrid.DashOn = 3;
            graph.XAxis.MajorGrid.DashOff = 5;
            graph.XAxis.MajorGrid.Color = Color.LightGray;
            graph.XAxis.MajorGrid.PenWidth = 0.3f;

            // Y Coordinate
            graph.YAxis.Title.Text = "Weight View(g)";
            graph.YAxis.Scale.MinorStep = 1.0f;
            graph.YAxis.Scale.MajorStep = 5.0f; // y-axis interval
            graph.YAxis.Scale.Min = -5.0;
            graph.YAxis.Scale.Max = 100.0f;
            graph.YAxis.MajorGrid.IsVisible = true;
            graph.YAxis.MajorGrid.DashOn = 3;
            graph.YAxis.MajorGrid.DashOff = 5;
            graph.YAxis.MajorGrid.Color = Color.LightGray;

            if(!comboBox1.Text.Equals(""))
                graph.YAxis.Scale.Max = double.Parse(comboBox1.Text);

            zedGraphControl1.AxisChange();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(sendHeartBeat);
            
            timer2 = new System.Windows.Forms.Timer();
            timer2.Interval = 100;
            timer2.Tick += new EventHandler(stateCheck);

            timer3 = new System.Windows.Forms.Timer();
            timer3.Interval = 100;
            timer3.Tick += new EventHandler(startCommSend);

            timer4 = new System.Windows.Forms.Timer();
            timer4.Interval = 100;
            timer4.Tick += new EventHandler(checkTimer);
            timer4.Start();

            timer5 = new System.Windows.Forms.Timer();
            timer5.Interval = 100;
            timer5.Tick += new EventHandler(reqtHost);

            timer6 = new System.Windows.Forms.Timer();
            timer6.Interval = 100;
            timer6.Tick += new EventHandler(reqtClient);
        }

        private void reiceiveCheck(object sender, EventArgs e)
        {
            try
            {
                if (sp.IsOpen)
                {
                    int size = sp.BytesToRead;
                    if (size != 0)
                    {
                        byte[] buffer = new byte[size];
                        sp.Read(buffer, 0, buffer.Length);

                        String recStr = Encoding.Default.GetString(buffer);
                        Console.WriteLine("수신1 : " + recStr);

                        String temp = orgData(recStr);

                        if (!temp.Equals(""))
                        {
                            reiceivedString.Add(temp);
                            //break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {

            }
        }

        //private string zedGraphControlPointEvent(ZedGraph.ZedGraphControl sender, ZedGraph.GraphPane pane, ZedGraph.CurveItem curve, int iPt)
        private bool zedGraphControlPointEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            //Thread.Sleep(100);
            xv.Clear();
            yv.Clear();
            //Console.WriteLine("XXX : "+ pane.po);
            double x, y;
            sender.GraphPane.ReverseTransform(e.Location, out x, out y);
            //Console.WriteLine(x);
            for (int i = 0; i < 200; i++)
            {
                //xv.Add(iPt, i);
                xv.Add(x, i);
            }

            for (int i = 0; i < 200; i++)
            {
                //xv.Add(iPt, i);
                yv.Add(i,y);
            }
            
            //zedGraphControl1.Move(0, 0);
            curve = graph.AddCurve("", xv, Color.Red, SymbolType.None);
            curve1 = graph.AddCurve("", yv, Color.Red, SymbolType.None);

            //curve.Line.Width = 1.0f;
            zedGraphControl1.Refresh();
            //zedGraphControl1.AxisChange();
            //zedGraphControl1.Invalidate();
            //Console.WriteLine("Point" + iPt);
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!comboBox3.Text.Equals(""))
            {
                sp = new SerialPort();
                sp.PortName = comboBox3.Text;
                sp.BaudRate = (int)19200;
                sp.DataBits = (int)8;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
                sp.ReadTimeout = (int)500;
                sp.WriteTimeout = (int)500;
                sp.ReceivedBytesThreshold = 1;
                f = 0;
                if (sp.IsOpen)
                {
                    //sp.Close();
                    MessageBox.Show("이미 연결되어있습니다.");
                }
                else
                {
                    try
                    {
                        sp.Open();

                        label39.Text = "Connecting....";
                        Console.WriteLine("CONNECTED");
                        state = 1;

                        // Send heart beat
                        reicevedData = "OK";
                    
                        //timer2.Start();
                        //startCommSend();
                        byte[] bytes = Encoding.UTF8.GetBytes("ATZ\n");
                        sp.Write(bytes, 0, bytes.Length);
                        fl1 = 0;
                        tStat = 3;
                       // timer.Start();
                        //thread = new Thread(new ThreadStart(sendHeartBeat));
                        //thread.Start();
                        
                        //timer2.Start();
                        
                    }
                    catch (SystemException exception)
                    {
                        // 시리얼포트 open실패시 예외처리
                        //throw new se
                        Console.WriteLine(exception);
                        MessageBox.Show("연결 상태를 확인하십시오");
                    }

                    // 연결성공시 데이터를 수신한다.
                    //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
                }
            }
            else
            {
                MessageBox.Show("포트를 선택하세요");
                comboBox3.Focus();
            }
        }

        private void checkTimer(object sender, EventArgs e)
        {
            Console.WriteLine("TS : {0}", tStat);
            switch (tStat)
            {
                case 1:
                    {
                        timer.Start();
                        timer2.Stop();
                        timer3.Stop();
                        timer5.Stop();
                        timer6.Stop();
                        break;
                    }
                case 2:
                    {
                        timer.Stop();
                        timer2.Start();
                        timer3.Stop();
                        timer5.Stop();
                        timer5.Stop();
                        timer6.Stop();
                        break;
                    }
                case 3:
                    {
                        timer.Stop();
                        timer2.Stop();
                        timer3.Start();
                        timer5.Stop();
                        timer5.Stop();
                        timer6.Stop();
                        break;
                    }
                case 5:
                    {
                        timer.Stop();
                        timer2.Stop();
                        timer3.Stop();
                        timer5.Start();
                        timer6.Stop();
                        break;
                    }
                case 6:
                    {
                        timer.Stop();
                        timer2.Stop();
                        timer3.Stop();
                        timer5.Stop();
                        timer6.Start();
                        break;
                    }
                default :
                    {
                        timer.Stop();
                        timer2.Stop();
                        timer3.Stop();
                        timer5.Stop();
                        timer6.Stop();
                        break ;
                    }

            }
        }

        private void stateCheck(object sender, EventArgs e)
        {
            Console.WriteLine("STS : {0}", sts);
            if (sts == 1)
            {
                sts = 0;
                tStat = 6;
            }
        }
        /*
        public void startDataSendReiceve()
        {
            sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
        }
        */
        public void startCommSend(object sender, EventArgs e)
        {
            byte[] bytes;
            
            // Handler
            //sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            Console.WriteLine("STST : {0}", thStat);
            if (thStat != 1)
            {
                thread = new Thread(new ThreadStart(dataReceivedThread));
                thread.Start();
                switch (fl1)
                {
                    case 1:
                        {
                            if(thread.IsAlive)
                            { 
                                thread.Abort();
                            }
                            thread = new Thread(new ThreadStart(dataReceivedThread));
                            thread.Start();
                            Console.WriteLine("version : " + reiceivedString[0]);
                            bytes = Encoding.UTF8.GetBytes("ATR CAL SEL\n");
                            sp.Write(bytes, 0, bytes.Length);
                            break;
                        }
                    case 2:
                        {
                            Console.WriteLine("B");
                            if (thread.IsAlive)
                            {
                                thread.Abort();
                            }
                            thread = new Thread(new ThreadStart(dataReceivedThread));
                            thread.Start();
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

                                Console.WriteLine("C");
                                thread.Abort();
                                thread = new Thread(new ThreadStart(dataReceivedThread));
                                thread.Start();
                            }
                            break;
                        }
                    case 3:
                        {
                            if (thread.IsAlive)
                            {
                                thread.Abort();
                            }
                            thread = new Thread(new ThreadStart(dataReceivedThread));
                            thread.Start();
                            bytes = Encoding.UTF8.GetBytes("OK\n");
                            sp.Write(bytes, 0, bytes.Length);
                            Console.WriteLine("CALI " + reiceivedString[1]);

                            if (reiceivedString[1].Equals("CAL SEL 1"))
                            {
                                bytes = Encoding.UTF8.GetBytes("ATR CAL 020\n");
                                cali = 20f;
                            }
                            else if (reiceivedString[1].Equals("CAL SEL 2"))
                            {
                                bytes = Encoding.UTF8.GetBytes("ATR CAL 050\n");
                                cali = 50f;
                            }
                            else if (reiceivedString[1].Equals("CAL SEL 3"))
                            {
                                bytes = Encoding.UTF8.GetBytes("ATR CAL 100\n");
                                cali = 100f;
                            }

                            sp.Write(bytes, 0, bytes.Length);
                            thread = new Thread(new ThreadStart(dataReceivedThread));
                            thread.Start();
                            
                            break;
                        }
                    case 4:
                        {
                            Console.WriteLine("CALIB {0}", reiceivedString[3].Split(new String[] { " " }, StringSplitOptions.None)[2]);
                            CAL00 = double.Parse(reiceivedString[2].Split(new String[] { " " }, StringSplitOptions.None)[2]);
                            calib = double.Parse(reiceivedString[3].Split(new String[] { " " }, StringSplitOptions.None)[2]);
                            tStat = 1;
                            label39.Text = "Connected";
                            f = 1;
                            break ;
                        }
                    default :
                        break;
                }
            }
        }

        public void startMeasure()
        {
            fl = 2;
        }

        public void reqtHost(object sender, EventArgs e)
        {
            Console.WriteLine("시작");
            tStat = 5;
            byte[] bytes;
            Console.WriteLine("ㅠㅠ : {0}", fl1);
            switch (fl1)
            {
                case 1:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS X01 {0}\n", int.Parse(numericUpDown3.Text) * 10));
                        sp.Write(bytes, 0, bytes.Length);
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 2:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS X02 {0}\n", int.Parse(numericUpDown4.Text) * 10));
                        sp.Write(bytes, 0, bytes.Length);
                        Thread.Sleep(500);
                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 3:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS MIN {0}\n", int.Parse(numericUpDown2.Text) * 10));
                        sp.Write(bytes, 0, bytes.Length);
                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 4:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS MAX {0}\n", int.Parse(numericUpDown1.Text) * 10));
                        sp.Write(bytes, 0, bytes.Length);
                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 5:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS SPD {0}\n", comboBox2.Text));
                        sp.Write(bytes, 0, bytes.Length);
                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 6:
                    {
                        bytes = Encoding.UTF8.GetBytes("ATC SRT\n");
                        sp.Write(bytes, 0, bytes.Length);
                        Console.WriteLine("1");
                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }

                case 7:
                    {
                        queue.Clear();
                        //timer.Stop();
                        thread.Abort();
                        //timer4.Stop();
                        fl1++;
                        tStat = 0;
                        sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
                        break;
                    }

            }
        }

        public void sendHeartBeat(object sender, EventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("ATA\n");
            //state1 = 0;
            if (sp.IsOpen)
            {
                sp.Write(bytes, 0, bytes.Length);
                reicevedData = "";

                Thread.Sleep(500);

                int size = sp.BytesToRead;
                    
                byte[] buffer = new byte[size];
                sp.Read(buffer, 0, buffer.Length);
                /*
                sp.DiscardInBuffer();
                queue.Clear();
                 * */
                String temp = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("test");
                Console.WriteLine(temp);
                Console.WriteLine(temp.IndexOf("BTN START"));
                if (!temp.Equals("") && temp.Length != 14)
                { 
                    if (temp.IndexOf("OK") == -1 && temp.IndexOf("BTN START") == -1)
                    {
                        label39.Text = "DISCONNECTED";
                        sp.Close();
                    }
                    else if (temp.IndexOf("BTN START") != -1)
                    {
                        sts = 1;
                        tStat = 2;
                        fl1 = 1;
                    }
                    else if (temp.IndexOf("OK\n") != -1)
                    {
                        
                    }
                }
            }
        }

        public void reqtClient(object sender, EventArgs e)
        {
            byte[] bytes;
            switch (fl1)
            {
                case 1:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS MIN {0}\n", numericUpDown2.Text));
                        sp.Write(bytes, 0, bytes.Length);
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 2:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS MAX {0}\n", numericUpDown2.Text));
                        sp.Write(bytes, 0, bytes.Length);
                       
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 3:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS X01 {0}\n", numericUpDown3.Text));
                        sp.Write(bytes, 0, bytes.Length);

                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 4:
                    {
                        // Min. Value
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS X02 {0}\n", numericUpDown4.Text));
                        sp.Write(bytes, 0, bytes.Length);

                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 5:
                    {
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATS SPD {0}\n", comboBox2.Text));
                        sp.Write(bytes, 0, bytes.Length);

                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 6:
                    {
                        bytes = Encoding.UTF8.GetBytes("ATC SRT\n");
                        sp.Write(bytes, 0, bytes.Length);

                        thread.Abort();
                        thread = new Thread(new ThreadStart(dataReceivedThread));
                        thread.Start();
                        break;
                    }
                case 7:
                    {
                        fl1++;
                        tStat = 0;
                        sp.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
                        break;
                    }
            }
        }

        private void dataReceivedThread()
        {
            thStat = 1;
            try
            {
                while (sp.IsOpen)
                {
                    int size = sp.BytesToRead;
                    if (size != 0)
                    {
                        byte[] buffer = new byte[size];
                        sp.Read(buffer, 0, buffer.Length);
                        
                        String recStr = Encoding.Default.GetString(buffer);
                        Console.WriteLine("수신1 : " + recStr);

                        String temp = orgData(recStr);

                        if (!temp.Equals(""))
                        {
                            reiceivedString.Add(temp);
                            fl1++;
                            thStat = 0;
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {

            }
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //sp.DiscardInBuffer();
            queue.Clear();
            // start measure
            int size = sp.BytesToRead;
            byte[] bytes = new byte[size];
            sp.Read(bytes, 0, size);

            String recStr = Encoding.Default.GetString(bytes);
            String tempStr = orgData(recStr);
            //this.tempStr += tempStr + "\r\n";
            Console.WriteLine("값 : " + tempStr);
            if (tempStr.Length == 14)
            {
                String[] data = tempStr.Split(new String[] { " " }, StringSplitOptions.None);
                if (data.Count() == 3)
                {
                    //double calcValue = double.Parse(data[2]) * calVal;//((CAL50 - CAL00) / 500) * (double.Parse(data[2]) - CAL00);
                    //double calcValue = (double.Parse(data[2]) * cali)/40 - calVal;
                    //if ((calib - cali00) == 0) { MessageBox.Show("ZERO...."); }
                    double calcValue = cali * (double.Parse(data[2]) - cali00) / (calib - cali00);
                    if (data[0].Equals("0000"))
                    {
                        MessageBox.Show("CAL 00 : " + cali00 + "\nCALIB : " + calib + "\nADC : " + data[2]);
                    }
                    Console.WriteLine("SEQ : " + data[0]);
                    
                        seq.Add(data[0]);
                    
                        percent = float.Parse(data[0]) / 406 * 100;
                        Console.WriteLine("X : " + data[1]);
                        Console.WriteLine("Y : " + calcValue);
                        x.Add(int.Parse(data[1]) / 10f);
                        y.Add(Math.Round(calcValue, 2));
                    try { 
                    if (int.Parse(data[0]) > 2 && y.Count-2>0&&x.Count-1>0)
                    {
                        inclination.Add((int)(y[y.Count-1] - y[y.Count-2]));
                        if (inclination.Count > 1)
                        {
                            if (inclination[inclination.Count-1] == 0 && inclination[inclination.Count - 2] < 0)
                            {
                                // 최솟값
                                minY.Add((int)y[y.Count-1]);
                                minX.Add((int)x[x.Count - 1]);
                            }
                            else if (inclination[inclination.Count-1] == 0 && inclination[inclination.Count - 2] > 0)
                            {
                                // 최댓값
                                maxY.Add((int)y[y.Count-1]);
                                maxX.Add((int)x[x.Count - 1]);
                            }
                        }
                    }
                        
                    sum += calcValue;
                    //this.write(tempStr, "/log.txt");
                    //drawing();
                    this.Invoke(new draw(drawing), null);
                        }
                    catch (Exception e1)
                    {
                        //MessageBox.Show("테스트용\n{0}", e1.Message);
                    }
                }     
            }
            //else if (tempStr.IndexOf("END") != -1)
            else if (tempStr.Equals("END"))
            {
                MessageBox.Show("완료되었습니다.....");
                percent = 100;
                //progressBar1.Style = ProgressBarStyle.Marquee;
                
                avg = sum / double.Parse(seq[seq.Count - 1]);
                double[] numberY = new double[seq.Count];

                for (int i = 0; i < seq.Count; i++)
                {
                    numberY[i] = x[i];
                }
                std = Math.Sqrt(numberY.Average(n => { double dif = n - avg; return dif * dif; }));
                Invoke(new drawR(result), avg, std, rMin, rMax, resultF);
                
                //if(thread.ThreadState)
                    
                thread = new Thread(new ThreadStart(sendStatics));
                thread.Start();
                /*
                label31.Text = String.Format("{0}", max);
                label32.Text = String.Format("{0}", min);
                label35.Text = String.Format("{0}", avg);
                    * */

            }
            else if(tempStr.IndexOf("PAUSE") != -1)
            {

            }
            else if (tempStr.IndexOf("RESET") != -1)
            {
                //tStat = 1;
                fl1 = 1;
                flag = "RST";
                state1 = 0;
                if (sp != null)
                {
                    if (sp.IsOpen)
                    {
                        sp.DataReceived -= new SerialDataReceivedEventHandler(dataReceived);
                        bytes = Encoding.UTF8.GetBytes(String.Format("ATC RST\n", std));
                        //thread = new Thread(new ThreadStart(resetFunc));
                        //thread.Start();
                        x.Clear();
                        y.Clear();
                        seq.Clear();
                        list.Clear();
                        sp.Write(bytes, 0, bytes.Length);
                    }
                }
                zedGraphControl1.Refresh();
                zedGraphControl1.AxisChange();
            }
            else
            {
                if (!tempStr.Equals(""))
                {
                    reiceivedString.Add(tempStr);
                    Console.WriteLine("크기 : {0}", reiceivedString.Count);
                }
            }
            sp.DiscardInBuffer();
        }

        private void sendStatics()
        {
            //reiceivedString.RemoveRange(0, reiceivedString.Count-1);
            reiceivedString.Clear();
            fl1 = 0;

            byte[] bytes = Encoding.UTF8.GetBytes("OK\n");
            sp.Write(bytes, 0, bytes.Length);

            bytes = Encoding.UTF8.GetBytes(String.Format("ATD MIN {0}\n", (int)(minY.Min() * 10)));
            sp.Write(bytes, 0, bytes.Length);
            Console.WriteLine("size : {0}", reiceivedString.Count);
            wait(1);
            fl1 = 0;
            bytes = Encoding.UTF8.GetBytes(String.Format("ATD MAX {0}\n", (int)(maxY.Max() * 10)));
            sp.Write(bytes, 0, bytes.Length);

            wait(2);
            fl1 = 0;
            bytes = Encoding.UTF8.GetBytes(String.Format("ATD AVG {0}\n", (int)(avg * 10)));
            sp.Write(bytes, 0, bytes.Length);

            wait(3);
            fl1 = 0;
            bytes = Encoding.UTF8.GetBytes(String.Format("ATD STD {0}\n", (int)(std * 10)));
            sp.Write(bytes, 0, bytes.Length);

            wait(4);
            fl1 = 0;

            if (numericUpDown1.Value.CompareTo((int)maxY.Max()) < 0)
            {
                bytes = Encoding.UTF8.GetBytes(String.Format("ATD RES {0}\n", "PAS"));
                fl = 0;
                resultF = "PASS";
            }
            if (numericUpDown2.Value.CompareTo((int)minY.Min()) > 0)
            {
                bytes = Encoding.UTF8.GetBytes(String.Format("ATD RES {0}\n", "FAI"));
                fl = 1;
                resultF = "Fail";
            }
            sp.Write(bytes, 0, bytes.Length);
            
            MessageBox.Show("통계값 전송완료.");
        }

        private static bool EndsWithSaurus(String s)
        {
            if ((s.Length > 5) &&
                (s.Substring(s.Length - 6).ToLower() == "saurus"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void result(double avg1, double std1, double min1, double max1, String result)
        {
            double CP, CPK;

            minY.Reverse();
            maxY.Reverse();

            PointPairList pp = new PointPairList();
            try
            {
                pp.Add(x[y.IndexOf(maxY[0]) - 1], maxY[0]);
                pp.Add(x[y.IndexOf(maxY[1]) - 1], maxY[1]);
                pp.Add(x[y.IndexOf(maxY[2]) - 1], maxY[2]);

            }
            catch (Exception e)
            {

            }

            LineItem myCurve = graph.AddCurve("", pp, Color.Red,
                SymbolType.Diamond);
            myCurve.Symbol.Size = 12;
            // Set up a red-blue color gradient to be used for the fill
            myCurve.Symbol.Fill = new Fill(Color.Red, Color.Blue);
            // Turn off the symbol borders
            myCurve.Symbol.Border.IsVisible = false;
            // Instruct ZedGraph to fill the symbols by selecting a color out of the
            // red-blue gradient based on the Z value.  A value of 19 or less will be red,
            // a value of 34 or more will be blue, and values in between will be a
            // linearly apportioned color between red and blue.
            myCurve.Symbol.Fill.Type = FillType.GradientByZ;
            myCurve.Symbol.Fill.RangeMin = 19;
            myCurve.Symbol.Fill.RangeMax = 34;
            // Turn off the line, so the curve will by symbols only
            myCurve.Line.IsVisible = false;
                

            if (fl == 1)
            {
                label22.ForeColor = Color.Green;
            }
            else if (fl == 0)
            {
                label22.ForeColor = Color.Red;
            }

            CP = (double.Parse(this.numericUpDown1.Value.ToString()) / double.Parse(this.numericUpDown2.Value.ToString())) / std1;
            label29.Text = String.Format("{0:F2}", CP);

            double k = (((double.Parse(this.numericUpDown1.Value.ToString()) + double.Parse(this.numericUpDown2.Value.ToString()))) / (2 - this.avg)) / (((((double.Parse(this.numericUpDown1.Value.ToString()) - double.Parse(this.numericUpDown2.Value.ToString()))) / 2)));
            CPK = (1 - k) * CP;
            label30.Text = String.Format("{0:F2}", CPK);

            label22.Text = String.Format("{0}", result);
            label31.Text = String.Format("{0:F2}", maxY.Max());
            label32.Text = String.Format("{0:F2}", minY.Min());
            label35.Text = String.Format("{0:F2}", avg1);
            label28.Text = String.Format("{0:F2}", std1);
            label21.Text = String.Format("{0:F2}", minY.Average());
            label33.Text = String.Format("{0:F2}", maxY.Average());
            //label21.Text = String.Format("{0:F2}", minY[0]+mi);
            //label33.Text = String.Format("{0:F2}", maxY.Average());
        }

        private String orgData(String recStr)
        {
            Console.WriteLine("PARAMETER : " + recStr);
            String tempStr = "";
            if (recStr.IndexOf("\n") == -1)
            {
                // '\n'이 수신될 때까지 큐에 누적한다.
                Console.WriteLine("index : " + recStr.IndexOf("\n"));
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
                else if (tempStrArr[1].IndexOf("\n") != -1)
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
                Delay(10);
                if (reiceivedString.Count == i && fl1 == 0)
                {
                    fl1 = 1;
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
            try
            {
                if ((int)percent >= 100)
                {
                    progressBar1.Value = 100;
                }
                else
                    progressBar1.Value = (int)percent;
            }
            catch (Exception e)
            {
                MessageBox.Show(""+e+"\nPERCENT : " +percent);
            }
            //zedGraphControl1.GraphPane.CurveList.Clear();

            // Draw Graph
            double x, y = 0.0f;
            graph = zedGraphControl1.GraphPane;

            double sum = 0f, CP = 0f, CPK = 0f, USL = 0f, LSL = 0f, SD = 0, k = 0f, avg = 0f;
            Console.WriteLine("CNT : " + seq.Count);
            for (int i = 0; i < seq.Count; i++)
            {
                x = this.x[i];
                y = this.y[i];
                sum += y;
                list.Add(x, y);

                if (numericUpDown1.Text != "" && numericUpDown2.Text != "")
                {
                    // Max Val. Check.
                    if (y > double.Parse(numericUpDown1.Text))
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
                    if (y > double.Parse(numericUpDown2.Text))
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

            // Add Line To graph
            LineItem curveG = graph.AddCurve("", list, Color.Red, SymbolType.None);
            curveG.Line.Width = 1.0f;

            // Draw
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();

            // Available Save file
            //zedGraphControl1.MasterPane.GetImage().Save("graph.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void printGraph(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(zedGraphControl1.MasterPane.GetImage(), 1000, 1000);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Disconnect
            if (sp.IsOpen)
            {
                tStat = 0;
                sp.Close();
            }
            else
            {
                // 이미 연결해제되었음.
                MessageBox.Show("연결상태가 아닙니다.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            i++;
            if (i < weightView.Length)
            {
                comboBox1.Text = String.Format("{0}", weightView[i]);
            }
            else
            {
                i = weightView.Length;
            }
        }

        private void button7_Click(object sender, MouseEventArgs e)
        {
            MessageBox.Show("1");
            maxV++;
            numericUpDown1.Text = String.Format("{0}", maxV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();

            for (int i = 0; i < 200; i++)
            {
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button8_Click(object sender, MouseEventArgs e)
        {
            MessageBox.Show("2");
            maxV--;
            numericUpDown1.Text = String.Format("{0}", maxV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            max.Clear();

            for (int i = 0; i < 200; i++)
            {
                max.Add(i, maxV);
            }

            curve = graph.AddCurve("", max, Color.Red, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            minV++;
            numericUpDown2.Text = String.Format("{0}", minV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
            }

            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

            curve1.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            minV--;
            numericUpDown2.Text = String.Format("{0}", minV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, minV);
            }

            curve1 = graph.AddCurve("", min, Color.Red, SymbolType.None);

            curve1.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            i--;
            if (i >= 0)
            {
                comboBox1.Text = String.Format("{0}", weightView[i]);
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
                comboBox2.Text = String.Format("{0}", pullSpeed[i]);
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
                comboBox2.Text = String.Format("{0}", pullSpeed[i]);
            }
            else
            {
                i = 0;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            tStat = 0;
            if (f == 1) 
            { 
                if (state1 == 0) 
                { 
                    queue.Clear();
                    if (sp != null)
                    {
                        Console.WriteLine("IS ALIVE? : " + sp.IsOpen);
                        if (sp.IsOpen)
                        {
                            //timer.Stop();
                            fl1 = 1;
                            tStat = 5;
                            //reqtHost();
                        }
                        else
                        {
                            MessageBox.Show("연결되지 않은 상태입니다.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("연결되지 않은 상태입니다.");
                    }
                }
                if (state1 == 1) 
                {
                    if(!flag.Equals("RST"))
                    { 
                        byte[] bytes = Encoding.UTF8.GetBytes(String.Format("ATC RSM\n", std));
                        sp.Write(bytes, 0, bytes.Length);
                        tStat = 0;
                    }
                }
            }
            else if (f == 0)
            {
                MessageBox.Show("미연결상태입니다.");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {

            PrintDocument print = new PrintDocument();
            PageSettings ps = new PageSettings();
            PrintDialog pd = new PrintDialog();

            print.PrintPage += new PrintPageEventHandler(printGraph);

            //인쇄 대화상자를 띄우자
            if (DialogResult.OK == pd.ShowDialog())
            {
                try
                {
                    try
                    {
                        print.Print();// 인쇄 시작
                    }
                    finally
                    {
                        //streamToPrint.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


            /*
            ps.Margins = new Margins(10, 10, 10, 10);
            ps.Landscape = true;
            print.DefaultPageSettings = ps;
            
            
            
            PrintPreviewDialog ppd = new PrintPreviewDialog();
            ppd.ClientSize = new System.Drawing.Size(500, 500);
            ppd.UseAntiAlias = true;
            print.PrintPage += new PrintPageEventHandler(printGraph);
            ppd.Document = print;
            pd.Document = print;
            */
            /*
            if (pd.ShowDialog() == DialogResult.OK)
                pd.ShowDialog();
             
            */
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //saveUserInfo();
        }

        private void saveUserInfo()
        {
            FileProcess fp = new FileProcess();
            StringBuilder sb = new StringBuilder("");

            if (checkBox1.Checked || fileFlag.Equals("CHECKED"))
            {
                // 사용자 정보 저장
                sb.Append("CHECKED");
                sb.Append(",");
                sb.Append(textBox3.Text);
                sb.Append(",");
                sb.Append(textBox4.Text);
                sb.Append(",");
                sb.Append(textBox5.Text);
                sb.Append(",");
                sb.Append(textBox6.Text);
                sb.Append(",");
                sb.Append(textBox7.Text);
                sb.Append(",");
                sb.Append(textBox16.Text);
                sb.Append(",");
                sb.Append(textBox10.Text);
                sb.Append(",");
                sb.Append(textBox11.Text);
                sb.Append(",");
                sb.Append(textBox12.Text);
                sb.Append(",");
                sb.Append(textBox13.Text);
                sb.Append(",");
                sb.Append(textBox14.Text);
                sb.Append(",");
                sb.Append(textBox15.Text);
                sb.Append(",");
                sb.Append(numericUpDown1.Text);
                sb.Append(",");
                sb.Append(numericUpDown2.Text);
                sb.Append(",");
                sb.Append(numericUpDown3.Text);
                sb.Append(",");
                sb.Append(numericUpDown4.Text);
                sb.Append(",");
                sb.Append(comboBox2.Text);
                sb.Append(",");
                sb.Append(comboBox1.Text);
            }
            else
            {
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
                sb.Append(";");
                sb.Append("");
            }
            fp.write(sb.ToString(), "/userInfo.csv", Directory.GetCurrentDirectory() + "/userInfo");
        }

        private void excelSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Add();
            Excel._Worksheet ws = (Excel.Worksheet)excel.ActiveSheet;

            excel.Visible = false;

            for (int inx = 1; inx <= this.x.Count; inx++)
            {
                ws.Cells[inx, "A"] = this.x[inx-1].ToString();
                ws.Cells[inx, "B"] = this.y[inx-1].ToString();
            }

            SaveFileDialog sd = new SaveFileDialog();
            sd.DefaultExt = "*.xls";
            sd.Filter = "Excel Files (*.xls)|*.xls";
            if (sd.ShowDialog() == DialogResult.OK)
            {
                wb.SaveAs(sd.FileName, Excel.XlFileFormat.xlWorkbookNormal);

                excel.Quit();
                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);

                excel = null;
                wb = null;
                ws = null;
            }

        }

        private void inputMaxValue(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                maxV = int.Parse(numericUpDown1.Text);
                //zedGraphControl1.GraphPane.CurveList.Clear();
                max.Clear();

                for (int i = 0; i < 200; i++)
                {
                    max.Add(i, maxV);
                }

                curve = graph.AddCurve("", max, Color.Red, SymbolType.None);

                curve.Line.Width = 1.0f;

                //zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
            }
        }

        private void inputMinValue(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                minV = int.Parse(numericUpDown2.Text);
                //zedGraphControl1.GraphPane.CurveList.Clear();
                min.Clear();

                for (int i = 0; i < 200; i++)
                {
                    min.Add(i, minV);
                }

                curve = graph.AddCurve("", max, Color.Red, SymbolType.None); ;

                curve.Line.Width = 1.0f; ;

                //zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void setupToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void calibration50gToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sp != null) 
            { 
                if(sp.IsOpen)
                { 
                    byte[] bytes = Encoding.UTF8.GetBytes("ATS CAL 050 940\n");
                    sp.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private void inputX1Value(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                x1V = int.Parse(numericUpDown3.Text);
                //zedGraphControl1.GraphPane.CurveList.Clear();
                x1.Clear();

                for (int i = 0; i < 200; i++)
                {
                    x1.Add(x1V, i);
                }

                curve2 = graph.AddCurve("", x1, Color.Red, SymbolType.None);

                curve2.Line.Width = 1.0f;

                //zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
            }
        }

        private void inputX2Value(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                x2V = int.Parse(numericUpDown4.Text);
                //zedGraphControl1.GraphPane.CurveList.Clear();
                x2.Clear();

                for (int i = 0; i < 200; i++)
                {
                    x2.Add(x2V, i);
                }

                curve3 = graph.AddCurve("", x2, Color.Blue, SymbolType.None);

                curve3.Line.Width = 1.0f;

                //zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
            }
        }

        private void maxValueChange(object sender, EventArgs e)
        {
            numericUpDown1.Minimum = numericUpDown2.Value;
            
            max.Clear();

            for (int i = 0; i < 200; i++)
            {
                max.Add(i, double.Parse(numericUpDown1.Value.ToString()));
            }

            curve = graph.AddCurve("", max, Color.Purple, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void minValueChange(object sender, EventArgs e)
        {
            numericUpDown2.Maximum = numericUpDown1.Value;
            min.Clear();

            for (int i = 0; i < 200; i++)
            {
                min.Add(i, double.Parse(numericUpDown2.Value.ToString()));
            }

            curve = graph.AddCurve("", min, Color.Purple, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void x1ValueChange(object sender, EventArgs e)
        {
            //maxV++;
            //numericUpDown1.Text = String.Format("{0}", maxV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            x1.Clear();

            for (int i = 0; i < 200; i++)
            {
                x1.Add(double.Parse(numericUpDown3.Value.ToString()), i);
            }

            curve = graph.AddCurve("", x1, Color.Blue, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void x2ValueChange(object sender, EventArgs e)
        {
            //maxV++;
            //numericUpDown1.Text = String.Format("{0}", maxV);
            //zedGraphControl1.GraphPane.CurveList.Clear();
            x2.Clear();

            for (int i = 0; i < 200; i++)
            {
                x2.Add(double.Parse(numericUpDown4.Value.ToString()), i);
            }

            curve = graph.AddCurve("", x2, Color.Blue, SymbolType.None);

            curve.Line.Width = 1.0f;

            //zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileProcess fp = new FileProcess();
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                String fileName = fd.FileName;

                String readStr = fp.read(fileName);
                if (!readStr.Equals(""))
                {
                    String[] str = readStr.Split(new String[] { "," }, StringSplitOptions.None);
                    if (str[0].Equals("CHECKED"))
                    {
                        checkBox1.Checked = true;

                        textBox3.Text = str[1];
                        textBox4.Text = str[2];
                        textBox5.Text = str[3];
                        textBox6.Text = str[4];
                        textBox7.Text = str[5];
                        textBox16.Text = str[6];
                        textBox10.Text = str[7];
                        textBox11.Text = str[8];
                        textBox12.Text = str[9];
                        textBox13.Text = str[10];
                        textBox14.Text = str[11];
                        textBox15.Text = str[12];
                        numericUpDown1.Text = str[13];
                        numericUpDown2.Text = str[14];
                        numericUpDown3.Text = str[15];
                        numericUpDown4.Text = str[16];
                        comboBox2.Text = str[17];
                        comboBox1.Text = str[18];
                        label31.Text = str[20];
                        label21.Text = str[21];
                        label32.Text = str[22];
                        label33.Text = str[23];
                        label22.Text = str[24];
                        label27.Text = str[25];
                        label29.Text = str[26];
                        label30.Text = str[27];
                        if (!str[19].Equals(""))
                        {
                            Console.WriteLine("테스트문자열 : " + str[19]);
                            String[] temp = str[19].Split(new String[] { "|" }, StringSplitOptions.None);
                            for (int i = 0; i < temp.Count(); i++)
                            {
                                String[] temp2 = temp[i].Split(new String[] { ";" }, StringSplitOptions.None);
                                
                                seq.Add(temp2[0]);
                                x.Add(double.Parse(temp2[1]));
                                Console.WriteLine("x is {0}", temp2[1]);
                                y.Add(double.Parse(temp2[2]));
                            }
                            
                            drawing();
                        }
                    }

                    String[] str2 = str[17].Split(new String[] { "|" }, StringSplitOptions.None);
                    Console.WriteLine("STR2 : " + str2.Count());
                    /*
                    for (int i = 0; i < str2.Count(); i++)
                    {
                        String[] str3 = str2[i].Split(new String[] { "," }, StringSplitOptions.None);
                        seq.Add(int.Parse(str3[0]));
                        x.Add(double.Parse(str3[1]));
                        y.Add(double.Parse(str3[2]));
                    }
                     * */
                }
            }
        }

        private void resizeWindow(object sender, EventArgs e)
        {
            //zedGraphControl1.Size = new System.Drawing.Size(ClientSize.Width, ClientSize.Height / 2+height / ClientSize.Height*50);
            //zedGraphControl1.Location = new Point(0, ClientSize.Height / 2 - height / ClientSize.Height * 50);
            
            //progressBar1.Location = new Point(0, ClientSize.Height / 2-26);
            progressBar1.Size = new Size(ClientSize.Width, 30);

            if (Width > ClientSize.Width)
            {
                graph.XAxis.Scale.MinorStep = (float)width / ClientSize.Width;
                graph.XAxis.Scale.MajorStep = (float)width / ClientSize.Width * 5.0;
            }
            else if (Width < ClientSize.Width)
            {
                graph.XAxis.Scale.MinorStep = (float)width / ClientSize.Width;
                graph.XAxis.Scale.MajorStep = (float)width / ClientSize.Width * 5.0;
            }

            if (height > ClientSize.Height)
            {
                graph.YAxis.Scale.MinorStep = (float)height / ClientSize.Height * 1.0f;
                graph.YAxis.Scale.MajorStep = (float)height / ClientSize.Height * 5.0;
            }
            else if (height < ClientSize.Height)
            {
                graph.YAxis.Scale.MinorStep = (float)height / ClientSize.Height * 1.0f;
                graph.YAxis.Scale.MajorStep = (float)height / ClientSize.Height * 5.0;
            }

            //ClientSize.
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileProcess fp = new FileProcess();
            StringBuilder sb = new StringBuilder("");
            SaveFileDialog sd = new SaveFileDialog();

            sd.DefaultExt = "*.csv";
            sd.Filter = "CSV Files (*.csv)|*.csv";

            if (sd.ShowDialog() == DialogResult.OK)
            {
                // 사용자 정보 저장
                sb.Append("CHECKED");
                sb.Append(",");
                sb.Append(textBox3.Text);
                sb.Append(",");
                sb.Append(textBox4.Text);
                sb.Append(",");
                sb.Append(textBox5.Text);
                sb.Append(",");
                sb.Append(textBox6.Text);
                sb.Append(",");
                sb.Append(textBox7.Text);
                sb.Append(",");
                sb.Append(textBox16.Text);
                sb.Append(",");
                sb.Append(textBox10.Text);
                sb.Append(",");
                sb.Append(textBox11.Text);
                sb.Append(",");
                sb.Append(textBox12.Text);
                sb.Append(",");
                sb.Append(textBox13.Text);
                sb.Append(",");
                sb.Append(textBox14.Text);
                sb.Append(",");
                sb.Append(textBox15.Text);
                sb.Append(",");
                sb.Append(numericUpDown1.Text);
                sb.Append(",");
                sb.Append(numericUpDown2.Text);
                sb.Append(",");
                sb.Append(numericUpDown3.Text);
                sb.Append(",");
                sb.Append(numericUpDown4.Text);
                sb.Append(",");
                sb.Append(comboBox2.Text);
                sb.Append(",");
                sb.Append(comboBox1.Text);
                sb.Append(",");
                for (int i = 0; i < seq.Count; i++)
                {
                    sb.Append(seq[i]);
                    sb.Append(";");
                    sb.Append(x[i]);
                    sb.Append(";");
                    sb.Append(y[i]);
                    if(i < seq.Count-1)
                    {
                        sb.Append("|");
                    }
                }
                sb.Append(",");
                sb.Append(label31.Text);
                sb.Append(",");
                sb.Append(label21.Text);
                sb.Append(",");
                sb.Append(label32.Text);
                sb.Append(",");
                sb.Append(label33.Text);
                sb.Append(",");
                sb.Append(label22.Text);
                sb.Append(",");
                sb.Append(label27.Text);
                sb.Append(",");
                sb.Append(label28.Text);
                sb.Append(",");
                sb.Append(label29.Text);
                sb.Append(",");
                sb.Append(label30.Text);
                Console.WriteLine("출력문자열 : " + sb.ToString());
                Console.WriteLine("파일출력 테스트 : " + sd.FileName);
                fp.write(sb.ToString(), sd.FileName);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            graph.YAxis.Scale.Max = double.Parse(comboBox1.Text);
            zedGraphControl1.Refresh();
            zedGraphControl1.AxisChange();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (sp != null)
            {
                if (sp.IsOpen)
                {
                    sp.DiscardInBuffer();
                    byte[] bytes = Encoding.UTF8.GetBytes(String.Format("ATC PAU\n", std));
                    state1 = 1;
                    sp.Write(bytes, 0, bytes.Length);
                    tStat = 1;
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //tStat = 1;
            fl1 = 1;
            flag = "RST";
            state1 = 0;
            if (sp != null)
            {
                if (sp.IsOpen)
                {
                    sp.DataReceived -= new SerialDataReceivedEventHandler(dataReceived);
                    byte[] bytes = Encoding.UTF8.GetBytes(String.Format("ATC RST\n", std));
                    sp.Write(bytes, 0, bytes.Length);
                    thread = new Thread(new ThreadStart(resetFunc));
                    thread.Start();
                    x.Clear();
                    y.Clear();
                    seq.Clear();
                    list.Clear();
                }
            }
            zedGraphControl1.Refresh();
            zedGraphControl1.AxisChange();
        }

        private void resetFunc()
        {
            try
            {
                while (sp.IsOpen)
                {
                    int size = sp.BytesToRead;
                    if (size != 0)
                    {
                        byte[] buffer = new byte[size];
                        sp.Read(buffer, 0, buffer.Length);

                        String recStr = Encoding.Default.GetString(buffer);
                        Console.WriteLine("수신1 : " + recStr);

                        String temp = orgData(recStr);

                        if (temp.IndexOf("RESET") != -1)
                        {
                            reiceivedString.Add(temp);
                            
                            thStat = 1;
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {

            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";

            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox16.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";

            numericUpDown1.Value = 95;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            numericUpDown4.Value = 0;

            comboBox2.Text = "";
            comboBox1.Text = "";

            label31.Text = "";
            label21.Text = "";
            label32.Text = "";
            label33.Text = "";
            label27.Text = "";
            label35.Text = "";
            label29.Text = "";
            label30.Text = "";
        }

        private void closingEvent(object sender, FormClosingEventArgs e)
        {
            saveUserInfo();
            if (sp != null)
            {
                sp.DataReceived -= new SerialDataReceivedEventHandler(dataReceived);
                sp.Dispose();
                sp.Close();
                //sp = null;
                thread.Abort();
                Thread.Sleep(100);
                
                Console.WriteLine(thread.ThreadState);
                //System.Runtime.InteropServices.Marshal.
                GC.Collect();
            }
        }

        private void label29_Click(object sender, EventArgs e)
        {

        }
    }
}