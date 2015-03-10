using System;
using System.IO;
using System.Text;

public class FileProcess
{
    public FileProcess()
	{
        
	}

    public String read(String fileName)
    {
        String dir = fileName;
        FileStream fis = null;
        StreamReader sr = null;
        StringBuilder buf = new StringBuilder("");
        try
        {
            fis = new FileStream(dir, FileMode.Open);
            sr = new StreamReader(fis, System.Text.Encoding.UTF8);
            
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            while (sr.Peek() > -1)
            {
                buf.Append(sr.ReadLine());
            }
        }
        catch (IOException e)
        {
            
        }
        finally
        {
            FileInfo file = new FileInfo(dir);
            if (file.Exists)
            {
                fis.Close();
                sr.Close();
            }
        }

        return buf.ToString();
    }

    public void write(String str, String fileName, String dir)
    {
        //String dir = Directory.GetCurrentDirectory() + "/userInfo";
        FileStream ois = null;
        Console.WriteLine("TEST1 : " + str);
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            FileInfo fileInfo = new FileInfo(dir + fileName);
            if (!fileInfo.Exists)
            {
                fileInfo.Create();
            }
            
            ois = new FileStream(dir + fileName, FileMode.Open);

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ois.Write(bytes, 0, bytes.Length);
        }
        catch(IOException ie)
        {
            Console.WriteLine("FAIL");
        }
        finally
        {
            ois.Close();
        }
    }

    public void write(String str, String fileName)
    {
        //String dir = Directory.GetCurrentDirectory() + "/userInfo";
        FileStream ois = null;
        Console.WriteLine("TEST2 : " + str);
        try
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                fileInfo.Create();
            }
            Console.WriteLine("FILE : " + fileInfo.FullName);
            ois = new FileStream(fileName, FileMode.Open);

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            ois.Write(bytes, 0, bytes.Length);
        }
        catch (IOException ie)
        {
            Console.WriteLine("FAIL");
            Console.WriteLine(ie);
        }
        finally
        {
            ois.Close();
        }
    }

    public void deleteFile(String dir, String fileName)
    {
        FileInfo fi = new FileInfo(Directory.GetCurrentDirectory() + dir + fileName);
        if (fi.Exists)
        {
            fi.Delete();
        }
    }
}
