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
            sr = new StreamReader(fis, System.Text.Encoding.Default);
            
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
                ois = new FileStream(fileName, FileMode.OpenOrCreate);
            }
            else
            { 
                ois = new FileStream(dir + fileName, FileMode.Open);
            }
            byte[] bytes = Encoding.Default.GetBytes(str);
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
                ois = new FileStream(fileName, FileMode.OpenOrCreate);
            }
            //Console.WriteLine("FILE : " + fileInfo.FullName.Substring(fileInfo.Directory.ToString().Length+1, fileInfo.FullName.Length - fileInfo.Directory.ToString().Length-1));
            else
            { 
                ois = new FileStream(fileName, FileMode.Open);
            }
            byte[] bytes = Encoding.Default.GetBytes(str);
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
