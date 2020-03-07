using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using HtmlAgilityPack;
using System.Threading;
using System.Diagnostics;
using SpiderStandard;
namespace Spider
{
    class Program
    {

        //尽然有人看我写的代码Σ(っ °Д °;)っ
        //欢迎欢迎
        //我写的代码很乱（没有专业训练
        //注释也很少而且不太精确（或许这样接地气
        //别喷我写的代码我还是个新手
        //没有了...
        static int aisle = 0;//浏览器线程数量
        static bool notstop = true;//不结束运行
        static bool opti = true;//根据性能优化
        static int mixing = 20;//搅拌（打乱？）一下抓到的连接
        static List<IWebDriver> WebDriver = new List<IWebDriver>();//浏览器页面
        static List<XUrl> ALLUrl = new List<XUrl>();//已抓取到的所有链接
        static List<string> ReadUrl = new List<string>();//已抓取到但未读取的链接
        static PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");//获取CPU的总占用率
        static float cpumax = 70;//默认cpu的最大占用，超过限速
        //  static List<string> UnReadUrl = new List<string>();//读取到的链接 已弃用
        static void Main(string[] args)
        {
            //
            Console.ForegroundColor=ConsoleColor.Blue;
            Console.WriteLine(@"   _____       _     _           " + "\r\n" +
                          @"  / ____|     (_)   | |          " + "\r\n" +
                          @" | (___  _ __  _  __| | ___ _ __ " + "\r\n" +
                          @"  \___ \| '_ \| |/ _` |/ _ \ '__|" + "\r\n" +
                          @"  ____) | |_) | | (_| |  __/ |   " + "\r\n" +
                          @" |_____/| .__/|_|\__,_|\___|_|   " + "\r\n" +
                          @"        | |                      " + "\r\n" +
                          @"        |_|                      ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("by slacksoft");
            Console.ForegroundColor = ConsoleColor.White;
            //
            //判断并加载保存的文件
            if (File.Exists("ALLUrl.bin"))
            {
                ALLUrl = (List<XUrl>)Ser_Des.BytesToObject(File.ReadAllBytes("ALLUrl.bin"));
            }
            if (File.Exists("ReadUrl.bin"))
            {
                ReadUrl = (List<string>)Ser_Des.BytesToObject(File.ReadAllBytes("ReadUrl.bin"));
            }
            else//没有文件？自行输入起始要抓取的连接
            {
                Console.Write("你需要添加链接,输入ok停止添加");
                bool i = true;
                while (i)
                {
                    string url = Console.ReadLine();
                    if (url == "ok")
                    {
                        i = false;
                    }
                    else
                    {
                        ReadUrl.Add(url);
                    }
                }
            }
            //线程数量
            Console.Write("输入浏览器线程数(推荐2-6)");
            aisle = int.Parse(Console.ReadLine());
            //创建浏览器
            for (int i = 0; i != aisle; i++)
            {
                WebDriver.Add(CreateDriver());
                Console.WriteLine("成功创建浏览器:" + i);
            }
            //我推荐的起始链接？
            //ReadUrl.Add(@"https://news.sogou.com/");
            // ReadUrl.Add(@"https://www.csdn.net/");
            //ReadUrl.Add(@"https://www.bilibili.com/");
            //启动抓取线程的开启线程
            Task SpideTask = new Task(() => SpiderStart());
            SpideTask.Start();
            Console.Write("程序已开始运行，可输入help或?查看帮助");
            //是否运行、
            bool Run = true;
            #region 程序命令
            //十分硬核的command
            while (Run)
            {
                string cmd = Console.ReadLine();
                if (cmd == "stop")
                {
                    notstop = false;
                    Console.WriteLine("开始关闭浏览器");
                    for (int i = 0; i != WebDriver.Count; i++)
                    {
                        try
                        {
                            Console.WriteLine("正在关闭浏览器" + i);
                            WebDriver[i].Close();
                            WebDriver[i].Quit();
                            Console.WriteLine("关闭浏览器" + i);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    Run = false;
                }
                else if (cmd == "allurl")
                {
                    for (int i = 0; i != ALLUrl.Count; i++)
                    {
                        Console.WriteLine(ALLUrl[i].url + ALLUrl[i].Tile);
                    }
                }
                else if (cmd == "readurl")
                {
                    for (int i = 0; i != ReadUrl.Count; i++)
                    {
                        Console.WriteLine(ReadUrl[i]);
                        if (ReadUrl[i] != null)
                        {
                            Console.WriteLine(ReadUrl[i]);
                        }
                    }
                }
                else if (cmd == "donenum")
                {
                    Console.WriteLine(TaskDone);
                }
                else if (cmd == "echo")
                {
                    if (echo)
                    {
                        echo = false;
                        Console.WriteLine("已关闭");
                    }
                    else
                    {
                        echo = true;
                        Console.WriteLine("已开启");
                    }
                }
                else if (cmd == "runum")
                {
                    Console.WriteLine(ReadUrl.Count);
                }
                else if (cmd == "aunum")
                {
                    Console.WriteLine(ALLUrl.Count);
                }
                else if (cmd == "save")
                {

                    Console.WriteLine("save ALLUrl");
                    File.WriteAllBytes("ALLUrl.bin", Ser_Des.ObjectToBytes(ALLUrl));
                    Console.WriteLine("save ReadUrl");
                    List<string> Buff = new List<string>();
                    for (int i = aisle - 1; i != ReadUrl.Count; i++)
                    {
                        if (ReadUrl[i] != null)
                        {
                            Buff.Add(ReadUrl[i]);
                        }
                    }
                    File.WriteAllBytes("ReadUrl.bin", Ser_Des.ObjectToBytes(Buff));
                    Console.WriteLine("Done");
                }
                else if (cmd == "cpumax")
                {
                    Console.Write("输入CPU最大占用率(0-100)");
                    cpumax = float.Parse(Console.ReadLine());
                    Console.Write("设置成功");
                }
                else if (cmd == "opti")
                {
                    if (opti)
                    {
                        opti = false;
                        Console.WriteLine("已关闭");
                    }
                    else
                    {
                        opti = true;
                        Console.WriteLine("已开启");
                    }
                }
                else if (cmd == "mixing")
                {
                    Console.Write("链接混合程度(默认20)");
                    mixing = int.Parse(Console.ReadLine());
                    Console.Write("设置成功");
                }
                else if (cmd == "clear")
                {
                    Console.Clear();
                }
                else if (cmd == "help" || cmd == "?")
                {
                    Console.WriteLine("================================================================");
                    Console.WriteLine("SlackSpider Beta 1.2");
                    Console.WriteLine("save - 保存抓取到的链接");
                    Console.WriteLine("stop - 停止抓取");
                    Console.WriteLine("aunum - 抓取到的总链接数量");
                    Console.WriteLine("runum - 未抓取和正在抓取的链接数量");
                    Console.WriteLine("donenum - 完成的主线程");
                    Console.WriteLine("echo - 开启/关闭部分输出(默认关闭)");
                    Console.WriteLine("cpumax - 设置CPU最大占用率(在开启优化时，默认70)");
                    Console.WriteLine("opti - 开启/关闭线程优化(效果拔群默认启动，关掉会有飞一般的速度)");
                    Console.WriteLine("mixing - 链接混合程度(默认20)");
                    Console.WriteLine("clear - 清屏");
                    Console.WriteLine("help - 帮助");
                    Console.WriteLine("================================================================");
                }
                else
                {
                    Console.WriteLine("未知的命令:" + cmd + " 输入help或?查看帮助");
                }
                #endregion
            }
        }
        #region 抓取线程的开启线程
        /// <summary>
        /// 抓取线程的开启线程
        /// </summary>
        public static void SpiderStart()
        {
            //如果程序没有被停止
            if (notstop)
            {
                Console.WriteLine("新的抓取线程");
                TaskDone = 0;
                for (int i = 0; i != aisle; i++)//添加抓取线程
                {
                    if (ReadUrl.Count - 1 < i)//链接不够创建
                    {
                        TaskDone++;
                        if (echo)
                        {
                            Console.WriteLine("未添加的线程:" + i);
                        }
                    }
                    else//创建
                    {
                        int Buffe = i;
                        if (echo)
                        {
                            Console.WriteLine("启动线程:" + Buffe);
                        }
                        Task spidersun = new Task(() => SpiderCore(Buffe));
                        spidersun.Start();
                    }
                }
                while (TaskDone != aisle) { }//等待扔出去的线程回来
                List<string> Buff = new List<string>();//把未读取的链接往上挪
                for (int i = aisle; i != ReadUrl.Count; i++)
                {
                    Buff.Add(ReadUrl[i]);
                }
                Console.WriteLine("混淆");
                ReadUrl = Randomlist(Buff);
                SpiderStart();
            }
        }
        #endregion
        static bool echo = false;//是否输出（为啥我当时要把它要叫做echo
        static int TaskDone = 0;//已经完成的线程
        static object TaskLockCore = new object();//线程锁
        #region 抓取线程
        /// <summary>
        /// 抓取线程
        /// </summary>
        /// <param name="Taskaisle">线程id</param>
        public static void SpiderCore(int Taskaisle)
        {

            try
            {
                if (echo)
                {
                    Console.WriteLine("访问:" + ReadUrl[Taskaisle]);
                }
                WebDriver[Taskaisle].Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(3));
                WebDriver[Taskaisle].Navigate().GoToUrl(ReadUrl[Taskaisle]);

                /*  XUrl DoneUrl = new XUrl();
                  DoneUrl.url = ReadUrl[Taskaisle];
                  DoneUrl.Tile = WebDriver[Taskaisle].Title;
                  Console.WriteLine("添加链接:"+ ReadUrl[Taskaisle]+ "标题:"+ WebDriver[Taskaisle].Title);
                  ALLUrl.Add(DoneUrl);*/

                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(WebDriver[Taskaisle].PageSource);
                HtmlNodeCollection hrefList = page.DocumentNode.SelectNodes(".//a[@href]");
                int hrefList_Count = 0;
                if (hrefList != null)
                {
                    hrefList_Count = hrefList.Count;
                }
                for (int i2 = 0; i2 != hrefList_Count; i2++)//循环遍历抓取到的链接组
                {
                    HtmlNode href = hrefList[i2];
                    HtmlAttribute att = href.Attributes["href"];
                    bool IsNotOld = true;
                    string HTTPUri = att.Value;

                    //Console.WriteLine(HTTPUri.Length+ HTTPUri);

                    //替换非http开头的路径链接开头，并扔掉一些没用的，格式错误的链接
                    if (HTTPUri.Length < 2)
                    {
                        HTTPUri = "";
                    }
                    else if (HTTPUri.IndexOf("http") == -1 && HTTPUri.Substring(0, 2) == @"//")
                    {
                        HTTPUri = HTTPUri.Replace("//", "http://");
                    }
                    else if (HTTPUri.IndexOf("http") == -1 && HTTPUri.Substring(0, 2) == @"./")
                    {
                        HTTPUri = HTTPUri.Replace("./", ReadUrl[Taskaisle]);
                    }
                    else if (HTTPUri.IndexOf("http") == -1 && HTTPUri.Substring(0, 1) == @"/")
                    {
                        HTTPUri = ReadUrl[Taskaisle] + HTTPUri.Substring(1, HTTPUri.Length - 1);
                    }
                    else if (HTTPUri.IndexOf("http") == -1)
                    {
                        HTTPUri = "";
                    }
                    //查看是否重复抓取链接
                    for (int I_repeat = 0; I_repeat != ALLUrl.Count; I_repeat++)
                    {
                        if (ALLUrl[I_repeat].url == HTTPUri)
                        {
                            IsNotOld = false;
                        }
                    }
                    for (int I_repeat = 0; I_repeat != ReadUrl.Count; I_repeat++)
                    {
                        if (ReadUrl[I_repeat] == HTTPUri)
                        {
                            IsNotOld = false;
                        }
                    }

                    if (HTTPUri != "" & IsNotOld & HTTPUri.ToCharArray().Length <= 250)
                    {

                        //Console.WriteLine(HTTPUri.ToCharArray().Length);

                        //标题获取线程
                        Thread geturl = new Thread(() =>
                        {

                            string geturlstring = HTTPUri;
                            try
                            {
                                HtmlAgilityPack.HtmlWeb get = new HtmlWeb();
                                HtmlDocument tdoc = get.Load(geturlstring);
                                XUrl DoneUrl = new XUrl();
                                DoneUrl.url = geturlstring;
                                if (tdoc != null)
                                {
                                    if (tdoc.DocumentNode.SelectSingleNode("//title").InnerText != null)//获取标题
                                    {
                                        DoneUrl.Tile = tdoc.DocumentNode.SelectSingleNode("//title").InnerText;
                                    }
                                    else
                                    {
                                        DoneUrl.Tile = geturlstring;
                                    }
                                    if (DoneUrl.Tile != "" & DoneUrl.Tile.IndexOf("404") == -1 & DoneUrl.Tile.IndexOf("NOT FOUND") == -1 & DoneUrl.Tile.IndexOf("not found") == -1 & DoneUrl.Tile.IndexOf("Not Found") == -1 & DoneUrl.Tile.IndexOf("¤") == -1 & DoneUrl.Tile.IndexOf("￠") == -1)//防止部分标题乱码和无法访问的网页（需要改进
                                    {
                                        //把抓到的链接添加进去
                                        ALLUrl.Add(DoneUrl);
                                        ReadUrl.Add(geturlstring);
                                        if (echo)
                                        {
                                            Console.WriteLine("添加链接:" + geturlstring + "标题:" + DoneUrl.Tile);
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                if (echo)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        });
                        geturl.Start();//启动线程
                        float nowcpu = cpuCounter.NextValue();
                        if (ReadUrl.Count <= aisle)
                        {
                            while (geturl.ThreadState == System.Threading.ThreadState.Running) { }
                        }
                        else if (nowcpu > cpumax && opti)
                        {

                            if (echo)
                            {
                                Console.WriteLine("CPU总占用" + nowcpu + "超过设定值，开始限速");
                            }
                            Thread.Sleep(1000);
                            if (geturl.ThreadState == System.Threading.ThreadState.Running)
                            {
                                geturl.Interrupt();
                                Console.WriteLine("线程超时");
                            }
                            else
                            {
                                Debug.WriteLine(geturl.ThreadState);
                            }
                        }

                        /* else
                         {
                             Thread threadover = new Thread(() =>
                             {
                                 Thread.Sleep(1000);
                                 if (geturl.ThreadState == System.Threading.ThreadState.Running)
                                 {
                                     geturl.Abort();
                                     if (echo)
                                     {
                                         Console.WriteLine("线程超时");
                                     }
                                 }
                                 else
                                 {
                                     //  Debug.WriteLine("线程不超速");
                                 }
                             });
                             threadover.Start();
                         }*/
                         //CPU去世器↑已弃用
                    }
                }

                ReadUrl[Taskaisle] = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            lock (TaskLockCore)
            {
                TaskDone++;
            }
            if (echo)
            {
                Console.WriteLine("访问完成");
            }
        }
        #endregion
        #region 浏览器的配置
        public static PhantomJSDriver CreateDriver()
        {
            PhantomJSDriverService services = PhantomJSDriverService.CreateDefaultService();
            services.HideCommandPromptWindow = true;//隐藏控制台窗口
            return new PhantomJSDriver(services);
        }
        #endregion
        #region 打乱链接，防止长时间抓取某个网站
        public static List<string> Randomlist(List<string> anylist)
        {
            List<string> newanylist = anylist;
            if (!(anylist.Count <= aisle))
            {
                Random random = new Random();
                for (int i = 0; i <= anylist.Count * mixing; i++)
                {

                    int i1 = random.Next(aisle, anylist.Count);
                    int i2 = random.Next(aisle, anylist.Count);
                    while (i1 == i2)
                    {
                        i2 = random.Next(aisle, anylist.Count); ;
                    }
                    string o1 = newanylist[i1];
                    string o2 = newanylist[i2];
                    if (echo) { Console.WriteLine(o1 + ":" + o2); }
                    newanylist[i1] = o2;
                    newanylist[i2] = o1;
                }
            }
            else
            {
                Console.WriteLine("链接太少无法混淆");
            }
            return newanylist;
        }
        #endregion

    }
}
