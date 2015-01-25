using System;
using System.IO.Ports;
using System.Collections;

public class SerialCommProcess
{

    private Queue queue;

    public SerialCommProcess()
    {

    }

    public SerialPort configureSerialPort()
    {
        SerialPort sp = new SerialPort();
        sp.PortName = "COM1";
        sp.BaudRate = (int)38400;
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
}