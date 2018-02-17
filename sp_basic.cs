* File info & Directory info 
<File>
1. Create
 - FileStream fs = File.Create("a.dat");
 - FileInfo file = new FileInfo("a.dat");
   FileStream fs = file.Create();
2. Copy
 - File.Copy("a.dat", "b.bat");
 - FileInfo src = new FileInfo("a.dat");
   FileInfo dst = src.CopyTo("b.dat");
3. Delete
 - File.Delete("a.dat");
 - FileInfo file = new FileInfo("a.dat");
   file.Delete();
4. Move
 - File.Move("a.dat", "b.dat");
 - FileInfo file = new FileInfo("a.dat");
   file.MoveTo("b.dat");
5. Check Exist 
 - if (File.Exists("a.dat"))
 - FileInfo file = new FileInfo("a.dat");
   if (file.Exists)
	   
<Directory>
1. Create
 - DirectoryInfo dir = Directory.CreateDirectory("a");
 - DirectoryInfo dir = new Directory("a");
   dir.Create();
2. Delete
 - Directory.Delete("a");
 - DirectoryInfo dir = new DirectoryInfo("a");
   dir.Delete();
3. Move
 - Directory.Move("a", "b");
 - DirectoryInfo dir = new DirectoryInfo("a");
   dir.MoveTo("b");
4. Check Exist
 - if (Directory.Exists("a.dat"))
 - DirectoryInfo dir = new DirectoryInfo("a");
   if (dir.Exists)
5. Search Attribute
 - Console.WriteLine(Directory.GetAttributes("a");
 - DirectoryInfo dir = new DirectoryInfo("a")
   Console.WriteLine(dir.Attributes);
6. Search subdirectories
 - string[] dirs = Directory.GetDirectories("a");
 - DirectoryInfo dir = new DirectoryInfo("a");
   DirectoryInfo[] dirs = dir.GetDirectories();
7. Search subfiles
 - string[] files = Directory.GetFiles("a");
 - DirectoryInfo dir = new DirectoryInfo("a");
   FileInfo[] files = dir.GetFiles();		     

FileClient.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace PrjFileClient
{
    class FileClient
    {
        private static void SendToServer(String strFilename)
        {
            try
            {
                // Establish the remote endpoint for the socket.
                //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 27015);
                const int BUF_SIZE = 4096;
                int nFReadLen;
                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    // Send Data
                    // Process the list of files found in the directory.
                    string strPath = "..\\..\\ClientFiles\\" + strFilename;
                    byte[] buffer = new byte[BUF_SIZE];
                    FileStream fileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read);
                        while ((nFReadLen = fileStream.Read(buffer, 0, BUF_SIZE)) > 0)
                            sender.Send(buffer, nFReadLen, SocketFlags.None);
                        fileStream.Close();

                    // Release the socket.
                    //..sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    Console.WriteLine(strFilename + " is sent.");
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            SendToServer("test.exe");
        }
    }
}

FileServer.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PrjFileServer
{
    class FileServer
    {
        static Socket listener;
        public class Worker
        {
            // This method will be called when the thread is started. 
            public void DoWork()
            {
                const int DEFAULT_BUFLEN = 4096;
                const int DEFAULT_PORT = 27015;
                const int FILENAME_LEN = 27;
                const int HEADER_LEN = (1 + FILENAME_LEN + 4);

                // Data buffer for incoming data.
                byte[] recvbuf = new byte[DEFAULT_BUFLEN + HEADER_LEN];
                int recvLen;
                string filename = "test.exe";

                // Establish the local endpoint for the socket.
                //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, DEFAULT_PORT);

                // Create a TCP/IP socket.
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and 
                // listen for incoming connections.
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);

                    // Start listening for connections.
                    while (true)
                    {
                        // Program is suspended while waiting for an incoming connection.
                        Socket handler = listener.Accept();

                        while ((recvLen = handler.Receive(recvbuf)) > 0)
                        {
                            string szFullPath = "..\\..\\ServerFiles\\" + filename;
                            SaveReceiveFile(szFullPath, recvbuf, recvLen);
                        }

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();

                        Console.WriteLine(filename + " is received.");
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.ToString());
                }

                listener.Close();
            }
        }

        static void Main(string[] args)
        {
            Worker workerObject = new Worker();
            Thread workerThread = new Thread(workerObject.DoWork);
            workerThread.Start();

            string strLine;
            while (true)
            {
                strLine = Console.ReadLine();

                if (strLine.Equals("QUIT"))
                {
                    listener.Close();
                    break;
                }
            }

            workerThread.Join();
        }

        private static void SaveReceiveFile(string fullPath, byte[] buf, int length)
        {
            FileStream fs = new FileStream(fullPath, FileMode.Append);
            fs.Write(buf, 0, length);
            fs.Close();
        }
    }
}

MutexSample.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 

namespace ThreadTest
{
    class ThreadSample
    {
        private static Mutex mut = new Mutex();

        static void PrintNums(string str)
        {
            Console.WriteLine("[" + str + "]");

            for (int i = 1; i <= 30; i++)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();
        }

        public class Worker
        {
            // This method will be called when the thread is started. 
            public void DoWork()
            {
                mut.WaitOne();
                PrintNums(name);
                mut.ReleaseMutex();
            }
            public string name;
        }

        static void Main(string[] args)
        {
            // Create the thread object. This does not start the thread. 
            Worker workerObject1 = new Worker();
            workerObject1.name = "Thread1";
            Thread workerThread1 = new Thread(workerObject1.DoWork);

            Worker workerObject2 = new Worker();
            workerObject2.name = "Thread2";
            Thread workerThread2 = new Thread(workerObject2.DoWork);


            // Start the worker thread. 
            workerThread1.Start();
            workerThread2.Start();

            mut.WaitOne();
            PrintNums("Main");
            mut.ReleaseMutex();

            // Use the Join method to block the current thread  
            // until the object's thread terminates. 
            workerThread1.Join();
            workerThread2.Join();
        }
    }
}

ThreadSample.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 

namespace ThreadTest
{
    class ThreadSample
    {
        public class Worker
        {
            // This method will be called when the thread is started. 
            public void DoWork()
            {
                for (int i=0; i<10; i++)
                {
                    Console.WriteLine("[Thread"+num+"] "+i);

                    Thread.Sleep(1);
                }
            }
            public int num;
        }

        static void Main(string[] args)
        {
            // Create the thread object. This does not start the thread. 
            Worker workerObject1 = new Worker();
            workerObject1.num = 1;
            Thread workerThread1 = new Thread(workerObject1.DoWork);

            Worker workerObject2 = new Worker();
            workerObject2.num = 2;
            Thread workerThread2 = new Thread(workerObject2.DoWork);


            // Start the worker thread. 
            workerThread1.Start();
            workerThread2.Start();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("[Main] "+i);
                // Put the main thread to sleep for 1 millisecond to 
                // allow the worker thread to do some work: 
                Thread.Sleep(1);
            }

            // Use the Join method to block the current thread  
            // until the object's thread terminates. 
            workerThread1.Join();
            workerThread2.Join();
        }
    }
}

PracFile.cs
using System;
using System.IO;

namespace PrjFileSearch
{
    class PracFile
    {
        static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo("./INPUT");
            FileInfo[] fiArr = di.GetFiles();

            foreach (FileInfo f in fiArr)
            {
                Console.WriteLine("{0}: {1}bytes.", f.Name, f.Length);
                if (f.Length > 2048)
                {
                    MyCopyFile(f.Name);
                }
            }
        }

        static void MyCopyFile(string filename)
        {
            System.IO.Directory.CreateDirectory(".\\OUTPUT");

            const int BUF_SIZE = 512;
            byte[] buffer = new byte[BUF_SIZE];
            int nFReadLen;

            FileStream fs_in = new FileStream("./INPUT/"+filename, FileMode.Open, FileAccess.Read);
            FileStream fs_out = new FileStream("./OUTPUT/"+filename, FileMode.Create, FileAccess.Write);
            while ((nFReadLen = fs_in.Read(buffer, 0, BUF_SIZE)) > 0)
            {
                fs_out.Write(buffer, 0, nFReadLen);
            }
            fs_in.Close();
            fs_out.Close();
        }
    }
}

EncDec.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PrjEncDec
{
    class EncDec
    {
        static void Base64Sample(string str)
        {
            //string str = "This is a Base64 test.";
            byte[] byteStr = System.Text.Encoding.UTF8.GetBytes(str);
            string encodedStr;
            byte[] decodedBytes;

            Console.WriteLine(str);

            encodedStr = Convert.ToBase64String(byteStr);
            Console.WriteLine(encodedStr);

            decodedBytes = Convert.FromBase64String(encodedStr);
            Console.WriteLine(Encoding.Default.GetString(decodedBytes));
        }

        static void SHA256Sample(string strInput)
        {
            byte[] hashValue;
            //string strInput = "1234";
            byte[] byteInput = System.Text.Encoding.UTF8.GetBytes(strInput);

            SHA256 mySHA256 = SHA256Managed.Create();
            hashValue = mySHA256.ComputeHash(byteInput);

            for (int i=0; i<hashValue.Length; i++)
                Console.Write(String.Format("{0:X2}",hashValue[i]));
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            while (true)
            {
                string strLine = Console.ReadLine();
                if (strLine.Equals("QUIT"))
                    break;

                Base64Sample(strLine);

                SHA256Sample(strLine);
            }
        }
    }
}

링버퍼
C#
https://circularbuffer.codeplex.com/

JAVA
http://tutorials.jenkov.com/java-performance/ring-buffer.html

c언어
https://github.com/dhess/c-ringbuf

C++ 
http://www.asawicki.info/news_1468_circular_buffer_of_raw_binary_data_in_c.html

List
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrjDS1_List
{
    class DS1_ArrayList
    {
        static void Main(string[] args)
        {
            List<Grade> myAL = new List<Grade>(); 
            
            string line;
            StreamReader file = new StreamReader("DS_Sample1.txt");
            while ((line = file.ReadLine()) != null)
            {
                char[] delimiterChars = { ' ' };
                string[] words = line.Split(delimiterChars);
                Grade userGrade = new Grade(words[0], Convert.ToInt32(words[1]), Convert.ToInt32(words[2]), Convert.ToInt32(words[3]));

                myAL.Add(userGrade);
            }
            file.Close();

            while (true)
            {
                string strLine = Console.ReadLine();

                if (strLine.Equals("PRINT"))    // 이름 순 출력
                {
                    myAL.Sort((Grade x, Grade y) => x.getName().CompareTo(y.getName()));
                }
                else if (strLine.Equals("KOREAN"))    // 국어 성적 순 출력
                {
                    myAL.Sort((Grade x, Grade y) => y.getKorean().CompareTo(x.getKorean()));
                }
                else if (strLine.Equals("ENGLISH"))    // 영어 성적 순 출력
                {
                    myAL.Sort((Grade x, Grade y) => y.getEnglish().CompareTo(x.getEnglish()));
                }
                else if (strLine.Equals("MATH"))    // 수학 성적 순 출력
                {
                    myAL.Sort((Grade x, Grade y) => y.getMath().CompareTo(x.getMath()));
                }
                else if (strLine.Equals("QUIT"))    // 종료
                {
                    break;
                }

                foreach (Grade obj in myAL)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", obj.getName(), obj.getKorean(), obj.getEnglish(), obj.getMath());
                }
            }
        }
    }

    public class Grade
    {
        private String strName;
        private int Korean;
        private int English;
        private int Math; 

        public Grade(string str, int k, int e, int m)
        {
            strName = str;
            Korean = k;
            English = e;
            Math = m;
        }

        public String getName()
        {
            return strName;
        }
        public void setName(String strName)
        {
            this.strName = strName;
        }
        public int getKorean()
        {
            return Korean;
        }
        public void setProjectA(int n)
        {
            Korean = n;
        }
        public int getEnglish()
        {
            return English;
        }
        public void setProjectB(int n)
        {
            English = n;
        }
        public int getMath()
        {
            return Math;
        }
        public void setMath(int n)
        {
            Math = n;
        }
    }
}

Map
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrjDS2_Dictionary
{
    class DS2_Dictionary
    {
        static void Main(string[] args)
        {
            Dictionary<string, Effort> m = new Dictionary<string, Effort>();

            string line;
            StreamReader file = new StreamReader("DS_Sample2.csv");
            while ((line = file.ReadLine()) != null)
            {
                Effort userEffort = new Effort();
                char[] delimiterChars = { ',' };
                string[] words = line.Split(delimiterChars);

                string key = words[1];

                if (!m.ContainsKey(key))
                {
                    userEffort.setName(words[2]);
                    userEffort.setProjectA(Convert.ToDouble(words[3]));
                    userEffort.setProjectB(Convert.ToDouble(words[4]));
                    userEffort.setProjectC(Convert.ToDouble(words[5]));

                    m.Add(key, userEffort);
                }
                else
                {
                    m[key].addProjectA(Convert.ToDouble(words[3]));
                    m[key].addProjectB(Convert.ToDouble(words[4]));
                    m[key].addProjectC(Convert.ToDouble(words[5]));
                }
            }
            file.Close(); 

            //foreach (KeyValuePair<string, Effort> items in m.OrderBy(x => x.Key))
            foreach (KeyValuePair<string, Effort> items in m)
            {
                double total = items.Value.getProjectA() + items.Value.getProjectB() + items.Value.getProjectC();
                string s = items.Key + "\t" + items.Value.getName() + "\t" + items.Value.getProjectA() + "\t" + items.Value.getProjectB() + "\t" + items.Value.getProjectC() + "\t=>\t" + total;
                Console.WriteLine(s);
            }
        }
    }

    public class Effort
    {
        private String strName;
        private double ProjectA;
        private double ProjectB;
        private double ProjectC;

        public String getName()
        {
            return strName;
        }
        public void setName(String strName)
        {
            this.strName = strName;
        }
        public double getProjectA()
        {
            return ProjectA;
        }
        public void setProjectA(double n)
        {
            ProjectA = n;
        }
        public double getProjectB()
        {
            return ProjectB;
        }
        public void setProjectB(double n)
        {
            ProjectB = n;
        }
        public double getProjectC()
        {
            return ProjectC;
        }
        public void setProjectC(double n)
        {
            ProjectC = n;
        }
        public void addProjectA(double n)
        {
            ProjectA += n;
        }
        public void addProjectB(double n)
        {
            ProjectB += n;
        }
        public void addProjectC(double n)
        {
            ProjectC += n;
        }
    }
}

FilesSearch.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace PrjFileSearch
{
    class FileSearch
    {
        static void Main(string[] args)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(".");
            foreach (string subdirectory in subdirectoryEntries)
                Console.WriteLine("[{0}]", subdirectory);   // Directory

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(".");
            foreach (string fileName in fileEntries)
                Console.WriteLine(fileName);   // file
        }
    }


Q1.
CardUtility.cs
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

class CardUtility
{
    public static string passwordEncryption_SHA256(string strInput)
    {
        byte[] hashValue;
        byte[] byteInput = System.Text.Encoding.UTF8.GetBytes(strInput);
        string strOutput = "";

        SHA256 mySHA256 = SHA256Managed.Create();
        hashValue = mySHA256.ComputeHash(byteInput);

        for (int i = 0; i < hashValue.Length; i++) {
            //Console.Write(String.Format("{0:X2}", hashValue[i]));
            strOutput += String.Format("{0:X2}", hashValue[i]);
        }

        return strOutput;
    }
}

Validator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Question1
{
    class Validator
    {
        public bool CheckIdPsw(string id, string psw)
        {
            bool bRet = false;
            String encPsw = CardUtility.passwordEncryption_SHA256(psw);

            string[] lines = System.IO.File.ReadAllLines(@"..\\..\\..\\CLIENT\\INSPECTOR.TXT");
            foreach (string line in lines)
            {
                String fileId = line.Substring(0, 8);
                String filePsw = line.Substring(9);

                if (id.Equals(fileId) && encPsw.Equals(filePsw))
                {
                    bRet = true;
                    break;
                }
            }

            return bRet;
        }
    }
}



Q2.
CardUtility.cs
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

class CardUtility
{
    public static string passwordEncryption_SHA256(string strInput)
    {
        byte[] hashValue;
        byte[] byteInput = System.Text.Encoding.UTF8.GetBytes(strInput);
        string strOutput = "";

        SHA256 mySHA256 = SHA256Managed.Create();
        hashValue = mySHA256.ComputeHash(byteInput);

        for (int i = 0; i < hashValue.Length; i++) {
            //Console.Write(String.Format("{0:X2}", hashValue[i]));
            strOutput += String.Format("{0:X2}", hashValue[i]);
        }

        return strOutput;
    }

    // yyyyMMddHHmmss형태의 문자열로 된 시간값 두 개를 입력받아서 시간 차이를 반환 
    // strTime2 - strTime1
    public static long HourDiff(string strTime2, string strTime1) 
    {
        DateTime dt1 = DateTime.ParseExact(strTime1, "yyyyMMddHHmmss", null);
        DateTime dt2 = DateTime.ParseExact(strTime2, "yyyyMMddHHmmss", null);

        TimeSpan ts = dt2 - dt1;

        return (long)ts.TotalHours;
	}
}

Validator.cs
using System;
using System.IO;
using System.Text;

namespace Question2
{
    class Validator
    {
        public bool CheckIdPsw(string id, string psw)
        {
            bool bRet = false;
            String encPsw = CardUtility.passwordEncryption_SHA256(psw);

            string[] lines = System.IO.File.ReadAllLines(@"..\\..\\..\\CLIENT\\INSPECTOR.TXT");
            foreach (string line in lines)
            {
                String fileId = line.Substring(0, 8);
                String filePsw = line.Substring(9);

                if (id.Equals(fileId) && encPsw.Equals(filePsw))
                {
                    bRet = true;
                    break;
                }
            }

            return bRet;
        }

        // cardInfo : [카드ID(8)][버스번호(7)][승차/하차 코드(1)][최근 승차시각(14)]
        // sample : CARD_001BUS_001N20171019143610
        public void InspectCard(string startTime, string id, string busID, string cardInfo)
        {
            string strValidateCode;

            // cardInfo parsing 
            //string cardID = cardInfo.Substring(0, 8); 
            string cardBusID = cardInfo.Substring(8, 7);
            string code = cardInfo.Substring(15, 1);
            string rideTime = cardInfo.Substring(16);

            // Get Inspect Time
            string strInspectTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Validation
            // 1. Bus ID Match
            if (busID.Equals(cardBusID))
            {
                // 2. Check Aboard Code
                if (code.Equals("N"))
                {
                    // 3. Time Difference
                    if (CardUtility.HourDiff(strInspectTime, rideTime) < 3)
                    {
                        strValidateCode = "R1";
                    }
                    else
                    {
                        strValidateCode = "R4";
                    }
                }
                else
                {
                    strValidateCode = "R3";
                }
            }
            else
            {
                strValidateCode = "R2";
            }

            // Create Folder
            string destFolder = "..\\..\\..\\" + id;
            System.IO.Directory.CreateDirectory(destFolder);

            // File Writing
            string strFilename = destFolder + "\\" + id + "_" + startTime + ".TXT";
            string strWrite = id + "#" + busID + "#" + cardInfo + "#" + strValidateCode + "#" + strInspectTime + "\n";
            FileStream fs = new System.IO.FileStream(strFilename, FileMode.Append);
            fs.Write(Encoding.UTF8.GetBytes(strWrite), 0, strWrite.Length);
            fs.Close();
        }
    }
}


Q3
Client
1.Validator
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Question3_Client
{
    class Validator
    {
        public bool CheckIdPsw(string id, string psw)
        {
            bool bRet = false;
            String encPsw = CardUtility.passwordEncryption_SHA256(psw);

            string[] lines = System.IO.File.ReadAllLines(@"..\\..\\..\\CLIENT\\INSPECTOR.TXT");
            foreach (string line in lines)
            {
                String fileId = line.Substring(0, 8);
                String filePsw = line.Substring(9);

                if (id.Equals(fileId) && encPsw.Equals(filePsw))
                {
                    bRet = true;
                    break;
                }
            }

            return bRet;
        }

        // cardInfo : [카드ID(8)][버스번호(7)][승차/하차 코드(1)][최근 승차시각(14)]
        // sample : CARD_001BUS_001N20171019143610
        public void InspectCard(string startTime, string id, string busID, string cardInfo)
        {
            string strValidateCode;

            // cardInfo parsing 
            //string cardID = cardInfo.Substring(0, 8); 
            string cardBusID = cardInfo.Substring(8, 7);
            string code = cardInfo.Substring(15, 1);
            string rideTime = cardInfo.Substring(16);

            // Get Inspect Time
            string strInspectTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Validation
            // 1. Bus ID Match
            if (busID.Equals(cardBusID))
            {
                // 2. Check Aboard Code
                if (code.Equals("N"))
                {
                    // 3. Time Difference
                    if (CardUtility.HourDiff(strInspectTime, rideTime) < 3)
                    {
                        strValidateCode = "R1";
                    }
                    else
                    {
                        strValidateCode = "R4";
                    }
                }
                else
                {
                    strValidateCode = "R3";
                }
            }
            else
            {
                strValidateCode = "R2";
            }

            // Create Folder
            string destFolder = "..\\..\\..\\" + id;
            System.IO.Directory.CreateDirectory(destFolder);

            // File Writing
            string strFilename = destFolder + "\\" + id + "_" + startTime + ".TXT";
            string strWrite = id + "#" + busID + "#" + cardInfo + "#" + strValidateCode + "#" + strInspectTime + "\n";
            FileStream fs = new System.IO.FileStream(strFilename, FileMode.Append);
            fs.Write(Encoding.UTF8.GetBytes(strWrite), 0, strWrite.Length);
            fs.Close();
        }

        public void SendToServer(String inspectorID)
        {
            try
            {
                // Establish the remote endpoint for the socket.
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 27015);
                const int BUF_SIZE = 4096;

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    // Send Data
                    // Process the list of files found in the directory.
                    string strPath = "..\\..\\..\\" + inspectorID;
                    string[] fileEntries = Directory.GetFiles(strPath);
                    byte[] buffer = new byte[BUF_SIZE];
                    int nFReadLen;
                    foreach (string fileName in fileEntries)
                    {
                        string onlyFileName = fileName.Substring(strPath.Length + 1);
                        FileInfo fInfo = new FileInfo(fileName);
                        int cnt = 0;

                        buffer[cnt] = (byte)'*';
                        cnt++;

                        byte[] byte_fname = Encoding.ASCII.GetBytes(onlyFileName);
                        Buffer.BlockCopy(byte_fname, 0, buffer, 1, byte_fname.Length);
                        cnt += byte_fname.Length;

                        CardUtility.WriteMessageInt32(ref buffer, cnt, (int)fInfo.Length);
                        cnt += 4;

                        // Header 전송
                        sender.Send(buffer, cnt, SocketFlags.None);

                        // Body 전송
                        FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        while ((nFReadLen = fileStream.Read(buffer, 0, BUF_SIZE)) > 0)
                            sender.Send(buffer, nFReadLen, SocketFlags.None);
                        fileStream.Close();

                        // file move 
                        string strPathDest = "..\\..\\..\\BACKUP\\" + onlyFileName;
                        if (File.Exists(strPathDest))
                            File.Delete(strPathDest);
                        File.Move(fileName, strPathDest);
                    }

                    // Release the socket.
                    //..sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

2. CardUtility
using System;
using System.Security.Cryptography;

class CardUtility
{
    public static string passwordEncryption_SHA256(string strInput)
    {
        byte[] hashValue;
        byte[] byteInput = System.Text.Encoding.UTF8.GetBytes(strInput);
        string strOutput = "";

        SHA256 mySHA256 = SHA256Managed.Create();
        hashValue = mySHA256.ComputeHash(byteInput);

        for (int i = 0; i < hashValue.Length; i++) {
            //Console.Write(String.Format("{0:X2}", hashValue[i]));
            strOutput += String.Format("{0:X2}", hashValue[i]);
        }

        return strOutput;
    }

    // yyyyMMddHHmmss형태의 문자열로 된 시간값 두 개를 입력받아서 시간 차이를 반환 
    // strTime2 - strTime1
    public static long HourDiff(string strTime2, string strTime1) 
    {
        DateTime dt1 = DateTime.ParseExact(strTime1, "yyyyMMddHHmmss", null);
        DateTime dt2 = DateTime.ParseExact(strTime2, "yyyyMMddHHmmss", null);

        TimeSpan ts = dt2 - dt1;

        return (long)ts.TotalHours;
	}

    public static int WriteMessageInt32(ref byte[] output, int index, int input)
    {
        byte[] ByteValue = BitConverter.GetBytes(input);

        output[index + 0] = ByteValue[0];
        output[index + 1] = ByteValue[1];
        output[index + 2] = ByteValue[2];
        output[index + 3] = ByteValue[3];

        return sizeof(Int32);
    }

    public static byte[] INT2LE(int data)
    {
        byte[] b = new byte[4];
        b[0] = (byte)data;
        b[1] = (byte)(((uint)data >> 8) & 0xFF);
        b[2] = (byte)(((uint)data >> 16) & 0xFF);
        b[3] = (byte)(((uint)data >> 24) & 0xFF);
        return b;
    }

}

Server
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Question3_Server
{
    class CardServer
    {
        public Socket listener;

        // This method will be called when the thread is started. 
        public void DoWork()
        {
            const int DEFAULT_BUFLEN = 4096;
            const int DEFAULT_PORT = 27015;
            const int FILENAME_LEN = 27;
            const int HEADER_LEN = (1 + FILENAME_LEN + 4);

            // Data buffer for incoming data.
            byte[] recvbuf = new byte[DEFAULT_BUFLEN + HEADER_LEN];
            int recvLen;
            string filename = "";

            // Establish the local endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, DEFAULT_PORT);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();

                    int nBufIndex = 0;  // 메시지를 담을 위치 인덱스
                    int cnt = 0;        // 메시지 처리 인덱스
                    int nDataSize = 0;	// 파일 데이터 크기
                    while ((recvLen = handler.Receive(recvbuf, nBufIndex, DEFAULT_BUFLEN, SocketFlags.None)) > 0)
                    {
                        recvLen += nBufIndex;

                        while (cnt < recvLen)
                        {
                            if (recvbuf[cnt] == '*')    // 메시지 시작 문자
                            {
                                // 처리하고 남은 버퍼의 크기가 헤더 사이즈보다 작으면 receive를 다시 해서 버퍼를 채운다 
                                if (recvLen - cnt < HEADER_LEN)
                                {
                                    Buffer.BlockCopy(recvbuf, cnt, recvbuf, 0, recvLen - cnt);  // 맨 끝에 남아있는 내용을 앞으로 이동.
                                    nBufIndex = recvLen - cnt;                                  // 새로 받을 버퍼의 위치 표시 
                                    cnt = 0;                                                    // 버퍼 처리는 맨 처음부터 
                                    break;
                                }
                                cnt++;

                                filename = Encoding.Default.GetString(recvbuf, cnt, FILENAME_LEN);
                                cnt += FILENAME_LEN;

                                nDataSize = BitConverter.ToInt32(recvbuf, cnt);
                                cnt += sizeof(int);

                                if (cnt == recvLen)
                                { // 처리한 데이터가 받은 메시지의 크기에 도달하면 -> 헤더만 받은 경우
                                    cnt = 0;
                                    nBufIndex = 0;
                                    break;
                                }
                            }

                            // 파일 데이터가 남아있는 버퍼보다 클 경우. 버퍼에 있는 것만 일단 쓰고 나머지는 다시receive하여 처리한다.
                            // 파일에 이미 쓴 크기를 nSize(파일크기)에서 빼서, 앞으로 쓸 크기만큼만 계산해 놓는다. 
                            if (recvLen - cnt <= nDataSize)
                            {
                                string szFullPath = "..\\..\\..\\SERVER\\" + filename;
                                SaveReceiveFile(szFullPath, recvbuf, cnt, recvLen - cnt);
                                nDataSize -= (recvLen - cnt);   // 처리한 (파일에 쓴) 크기만큼 줄인다.  
                                cnt = 0;
                                nBufIndex = 0;
                                break;
                            }
                            // 파일 데이터가 남아있는 버퍼에 모두들어있는 경우. 
                            // 데이터 다음에 *부터 다시 시작하는 데이터가 들어있는 것임, while(cnt < recvLen)문을 돌며 계속 처리.
                            else
                            {
                                string szFullPath = "..\\..\\..\\SERVER\\" + filename;
                                SaveReceiveFile(szFullPath, recvbuf, cnt, nDataSize);
                                cnt += nDataSize;
                                nDataSize = 0;
                            }
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception)
            {
                //Console.WriteLine(e.ToString());
            }

            listener.Close();
        }

        private static void SaveReceiveFile(string fullPath, byte[] buf, int index, int length)
        {
            FileStream fs = new FileStream(fullPath, FileMode.Append);
            fs.Write(buf, index, length);
            fs.Close();
        }
    }
}

Q4.
CardServer.cs
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Question4_Server
{
    class CardServer
    {
        public Socket listener;

        // This method will be called when the thread is started. 
        public void DoWork()
        {
            const int DEFAULT_BUFLEN = 4096;
            const int DEFAULT_PORT = 27015;
            const int FILENAME_LEN = 27;
            const int HEADER_LEN = (1 + FILENAME_LEN + 4);

            // Data buffer for incoming data.
            byte[] recvbuf = new byte[DEFAULT_BUFLEN + HEADER_LEN];
            int recvLen;
            string filename = "";

            // Establish the local endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, DEFAULT_PORT);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();

                    int nBufIndex = 0;  // 메시지를 담을 위치 인덱스
                    int cnt = 0;        // 메시지 처리 인덱스
                    int nDataSize = 0;	// 파일 데이터 크기
                    while ((recvLen = handler.Receive(recvbuf, nBufIndex, DEFAULT_BUFLEN, SocketFlags.None)) > 0)
                    {
                        recvLen += nBufIndex;

                        while (cnt < recvLen)
                        {
                            if (recvbuf[cnt] == '*')    // 메시지 시작 문자
                            {
                                // 처리하고 남은 버퍼의 크기가 헤더 사이즈보다 작으면 receive를 다시 해서 버퍼를 채운다 
                                if (recvLen - cnt < HEADER_LEN)
                                {
                                    Buffer.BlockCopy(recvbuf, cnt, recvbuf, 0, recvLen - cnt);  // 맨 끝에 남아있는 내용을 앞으로 이동.
                                    nBufIndex = recvLen - cnt;                                  // 새로 받을 버퍼의 위치 표시 
                                    cnt = 0;                                                    // 버퍼 처리는 맨 처음부터 
                                    break;
                                }
                                cnt++;

                                filename = Encoding.Default.GetString(recvbuf, cnt, FILENAME_LEN);
                                cnt += FILENAME_LEN;

                                nDataSize = BitConverter.ToInt32(recvbuf, cnt);
                                cnt += sizeof(int);

                                if (cnt == recvLen)
                                { // 처리한 데이터가 받은 메시지의 크기에 도달하면 -> 헤더만 받은 경우
                                    cnt = 0;
                                    nBufIndex = 0;
                                    break;
                                }
                            }

                            // 파일 데이터가 남아있는 버퍼보다 클 경우. 버퍼에 있는 것만 일단 쓰고 나머지는 다시receive하여 처리한다.
                            // 파일에 이미 쓴 크기를 nSize(파일크기)에서 빼서, 앞으로 쓸 크기만큼만 계산해 놓는다. 
                            if (recvLen - cnt <= nDataSize)
                            {
                                string szFullPath = "..\\..\\..\\SERVER\\" + filename;
                                SaveReceiveFile(szFullPath, recvbuf, cnt, recvLen - cnt);
                                nDataSize -= (recvLen - cnt);   // 처리한 (파일에 쓴) 크기만큼 줄인다.  
                                cnt = 0;
                                nBufIndex = 0;
                                break;
                            }
                            // 파일 데이터가 남아있는 버퍼에 모두들어있는 경우. 
                            // 데이터 다음에 *부터 다시 시작하는 데이터가 들어있는 것임, while(cnt < recvLen)문을 돌며 계속 처리.
                            else
                            {
                                string szFullPath = "..\\..\\..\\SERVER\\" + filename;
                                SaveReceiveFile(szFullPath, recvbuf, cnt, nDataSize);
                                cnt += nDataSize;
                                nDataSize = 0;
                            }
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception)
            {
                //Console.WriteLine(e.ToString());
            }

            listener.Close();
        }

        private static void SaveReceiveFile(string fullPath, byte[] buf, int index, int length)
        {
            FileStream fs = new FileStream(fullPath, FileMode.Append);
            fs.Write(buf, index, length);
            fs.Close();
        }
    }
}

Report.cs
using System;

namespace Question4_Server
{
    public class Report
    {
        private String strID;
        private int CheckCard;
        private int FailCard;

        public String getStrID()
        {
            return strID;
        }
        public void setStrID(String strID)
        {
            this.strID = strID;
        }
        public int getCheckCard()
        {
            return CheckCard;
        }
        public void setCheckCard(int nCheckCard)
        {
            this.CheckCard = nCheckCard;
        }
        public int getFailCard()
        {
            return FailCard;
        }
        public void setFailCard(int nFailCard)
        {
            this.FailCard = nFailCard;
        }
        public void increaseCheckCard()
        {
            this.CheckCard++;
        }
        public void increaseFailCard()
        {
            this.FailCard++;
        }
    }
}

ValidatorReport.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Question4_Server
{
    class ValidatorReport
    {
        public bool ReportValidator()
        {
            Dictionary<int, Report> m;
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            m = new Dictionary<int, Report>();

            string strPath = "..\\..\\..\\SERVER";
            string[] fileEntries = Directory.GetFiles(strPath);

            foreach (string fileName in fileEntries)
            {
                string onlyFileName = fileName.Substring(strPath.Length + 1);
                if (onlyFileName.Length == 27 && onlyFileName.Substring(9, 8).Equals(strToday))
                {
                    AnalysisData(m, fileName);
                }
            }
            // Save Report File
            SaveReportFile(m, strToday);
            return true;
        }

        private void AnalysisData(Dictionary<int, Report> m, string path)
        {
            string line;
            StreamReader file = new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                Report userReport = new Report();

                int key = Convert.ToInt32(line.Substring(5, 3));
                if (!m.ContainsKey(key))
                {
                    userReport.setStrID(line.Substring(0, 8));
                    userReport.setCheckCard(1);
                    if (line.Substring(49, 1).Equals("1"))
                    {
                        userReport.setFailCard(0);
                    }
                    else
                    {
                        userReport.setFailCard(1);
                    }
                    m.Add(key, userReport);
                }
                else
                {
                    m[key].increaseCheckCard();
                    if (!line.Substring(49, 1).Equals("1"))
                    {
                        m[key].increaseFailCard();
                    }
                }
            }
            file.Close();
        }

        private void SaveReportFile(Dictionary<int, Report> m, string strToday)
        {
            string filename = "..\\..\\..\\SERVER\\REPORT_" + strToday + ".TXT";

            FileStream fs = new FileStream(filename, FileMode.Create);

            foreach (KeyValuePair<int, Report> items in m)
            {
                string s = items.Value.getStrID() + " " + items.Value.getCheckCard() + " " + items.Value.getFailCard() + "\n";
                fs.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            }

            fs.Close();
        }

        public void PrintReport(string strDate, string option)
        {
            List<Report> reportList = new List<Report>();
            string strPath = "..\\..\\..\\SERVER\\REPORT_" + strDate + ".TXT";
            char[] delimiterChars = { ' ' };
            try
            {
                string[] lines = File.ReadAllLines(strPath);
                foreach (string line in lines)
                {
                    string[] words = line.Split(delimiterChars);
                    Report cReport = new Report();

                    cReport.setStrID(words[0]);
                    cReport.setCheckCard(Convert.ToInt32(words[1]));
                    cReport.setFailCard(Convert.ToInt32(words[2]));
                    reportList.Add(cReport);
                }

                if (option.Equals("C"))
                {
                    reportList.Sort(delegate (Report x, Report y)
                    {
                        return y.getCheckCard().CompareTo(x.getCheckCard());
                    });
                }

                for (int i = 0; i < reportList.Count; i++)
                {
                    Console.WriteLine(reportList[i].getStrID() + " " + reportList[i].getCheckCard() + " " + reportList[i].getFailCard());
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}


















