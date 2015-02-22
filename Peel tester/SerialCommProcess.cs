using System;
using System.IO.Ports;
using System.Collections;
using System.Text;

public class SerialCommProcess
{

    private Queue queue;  
    private static SerialPort sp;

    public SerialCommProcess()
    {

    }

    public SerialPort configureSerialPort()
    {
        sp = new SerialPort();
        sp.PortName = "COM3";
        sp.BaudRate = (int)19200;
        sp.DataBits = (int)8;
        sp.Parity = Parity.None;
        sp.StopBits = StopBits.One;
        sp.ReadTimeout = (int)500;
        sp.WriteTimeout = (int)500;

        queue = new Queue();

        return sp;
    }

    public Queue getQueue()
    {
        return queue;
    }

    public void setQueue(Queue queue)
    {
        this.queue = queue;
    }

    public void cumulativeData(byte bt)
    {
        queue.Enqueue(bt);
    }

    public void startCommSend(SerialPort sp)
    {
        byte[] bytes = Encoding.UTF8.GetBytes("ATZ");
        sp.Write(bytes, 0, bytes.Length);


    }
}