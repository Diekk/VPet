﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Core;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Application = System.Windows.Application;
using System.Timers;
using LinePutScript;
using System.Diagnostics;
using ChatGPT.API.Framework;
using static VPet_Simulator.Core.GraphCore;
using Panuon.WPF.UI;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.IGraph;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq;

namespace VPet_Simulator.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        private NotifyIcon notifyIcon;
        public System.Timers.Timer AutoSaveTimer = new System.Timers.Timer();
        public MainWindow()
        {
            //判断是不是Steam用户,因为本软件会发布到Steam
            //在 https://store.steampowered.com/app/1920960/VPet
            try
            {
#if DEMO
                SteamClient.Init(2293870, true);
#else
                SteamClient.Init(1920960, true);
#endif
                SteamClient.RunCallbacks();
                IsSteamUser = SteamClient.IsValid;
                ////同时看看有没有买dlc,如果有就添加dlc按钮
                //if (Steamworks.SteamApps.IsDlcInstalled(1386450))
                //  dlcToolStripMenuItem.Visible = true;
            }
            catch
            {
                IsSteamUser = false;
            }
            //给正在玩这个游戏的主播/游戏up主做个小功能
            if (IsSteamUser)
            {
                rndtext.Add(new Tuple<string, Helper.SayType>($"关注 {SteamClient.Name} 谢谢喵", Helper.SayType.Shining));
            }
            else
            {
                rndtext.Add(new Tuple<string, Helper.SayType>($"关注 {Environment.UserName} 谢谢喵", Helper.SayType.Shining));
            }
            //加载游戏设置
            if (new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Setting.lps").Exists)
            {
                Set = new Setting(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Setting.lps"));
            }
            else
                Set = new Setting("Setting#VPET:|\n");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\ChatGPTSetting.json"))
                CGPTClient = ChatGPTClient.Load(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\ChatGPTSetting.json"));
            //this.Width = 400 * ZoomSlider.Value;
            //this.Height = 450 * ZoomSlider.Value;
            InitializeComponent();

            //this.Height = 500 * Set.ZoomLevel;
            this.Width = 500 * Set.ZoomLevel;

            if (Set.StartRecordLast)
            {
                var point = Set.StartRecordLastPoint;
                if (point.X != 0 || point.Y != 0)
                {
                    this.Left = point.X;
                    this.Top = point.Y;
                }
            }
            else
            {
                var point = Set.StartRecordPoint;
                Left = point.X; Top = point.Y;
            }

            //不存在就关掉
            var modpath = new DirectoryInfo(ModPath + @"\0000_core\pet\vup");
            if (!modpath.Exists)
            {
                MessageBox.Show("缺少模组Core,无法启动桌宠", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            Task.Run(GameLoad);
        }
        public new void Close()
        {
            if (Main == null)
            {
                base.Close();
            }
            else
            {
                Main.DisplayClose(() => Dispatcher.Invoke(base.Close));
            }
        }
        public void Restart()
        {
            this.Closed -= Window_Closed;
            this.Closed += Restart_Closed;
            base.Close();
        }

        private void Restart_Closed(object sender, EventArgs e)
        {
            Save();
            try
            {
                //关闭所有插件
                foreach (MainPlugin mp in Plugins)
                    mp.EndGame();
            }
            catch { }
            Main?.Dispose();
            notifyIcon?.Dispose();
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Environment.Exit(0);
        }

        private List<Tuple<string, Helper.SayType>> rndtext = new List<Tuple<string, Helper.SayType>>
        {
              new Tuple<string, Helper.SayType>("你知道吗? 鼠标右键可以打开菜单栏", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("如果你觉得目前功能太少,那就多挂会机. 宠物会自己动的", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("你知道吗? 你可以在设置里面修改游戏的缩放比例", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("想要宠物不乱动? 设置里可以设置智能移动或者关闭移动", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("有建议/游玩反馈? 来 菜单-系统-反馈中心 反馈吧", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("你现在乱点说话是说话系统的一部分,不过还没做,在做了在做了ing", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("你添加了虚拟主播模拟器和虚拟桌宠模拟器到愿望单了吗? 快去加吧", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("这游戏开发这么慢,都怪画师太咕了", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("长按脑袋拖动桌宠到你喜欢的任意位置", Helper.SayType.Serious),
              new Tuple<string, Helper.SayType>("欢迎加入 虚拟主播模拟器群 430081239", Helper.SayType.Shining),
        };
        public long lastclicktime { get; set; }
        public void GameLoad()
        {
            //加载所有MOD
            List<DirectoryInfo> Path = new List<DirectoryInfo>();
            Path.AddRange(new DirectoryInfo(ModPath).EnumerateDirectories());
            if (IsSteamUser)//如果是steam用户,尝试加载workshop
            {
                Dispatcher.Invoke(new Action(() => LoadingText.Content = "尝试加载 Steam Workshop"));
                int i = 1;
                while (true)
                {
                    var page = Steamworks.Ugc.Query.ItemsReadyToUse.GetPageAsync(i++).Result;
                    if (page.HasValue && page.Value.ResultCount != 0)
                    {
                        foreach (Steamworks.Ugc.Item entry in page.Value.Entries)
                        {
                            if (entry.Directory != null)
                                Path.Add(new DirectoryInfo(entry.Directory));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Dispatcher.Invoke(new Action(() => LoadingText.Content = "尝试加载 Steam Workshop"));
            //加载mod
            foreach (DirectoryInfo di in Path)
            {
                if (!File.Exists(di.FullName + @"\info.lps"))
                    continue;
                Dispatcher.Invoke(new Action(() => LoadingText.Content = $"尝试加载 MOD数据: {di.Name}"));
                CoreMODs.Add(new CoreMOD(di, this));
            }
            foreach (CoreMOD cm in CoreMODs)
                if (!cm.SuccessLoad)
                    if (Set.IsPassMOD(cm.Name))
                        MessageBoxX.Show($"模组 {cm.Name} 的代码插件损坏\n虚拟桌宠模拟器未能成功加载该插件\n请联系作者修复该问题", $"{cm.Name} 未加载代码插件");
                    else if (Set.IsMSGMOD(cm.Name))
                        MessageBoxX.Show($"由于 {cm.Name} 包含代码插件\n虚拟桌宠模拟器已自动停止加载该插件\n请手动前往设置允许启用该mod 代码插件", $"{cm.Name} 未加载代码插件");
            CoreMOD.NowLoading = null;
            //判断是否需要清空缓存
            if (Set.LastCacheDate < CoreMODs.Max(x => x.CacheDate))
            {//需要清理缓存
                Set.LastCacheDate = DateTime.Now;
                if (Directory.Exists(CachePath))
                {
                    Directory.Delete(CachePath, true);
                    Directory.CreateDirectory(CachePath);
                }
            }
            Dispatcher.Invoke(new Action(() => LoadingText.Content = "尝试加载游戏MOD"));

            //加载游戏内容
            Core.Controller = new MWController(this);
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Save.lps"))
                Core.Save = GameSave.Load(new LpsDocument(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Save.lps")).First());
            else
                Core.Save = new GameSave("萝莉斯");

            AutoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;

            if (Set.AutoSaveInterval > 0)
            {
                AutoSaveTimer.Interval = Set.AutoSaveInterval * 60000;
                AutoSaveTimer.Start();
            }

            Dispatcher.Invoke(new Action(() =>
            {
                LoadingText.Content = "尝试加载动画和生成缓存";
                var pl = Pets.Find(x => x.Name == Set.PetGraph);
                Core.Graph = pl == null ? Pets[0].Graph() : pl.Graph();
                LoadingText.Content = "正在加载CGPT";

                winSetting = new winGameSetting(this);
                winBetterBuy = new winBetterBuy(this);
                Main = new Main(Core) { };
                if (!Set["CGPT"][(gbol)"enable"] && IsSteamUser)
                {
                    TalkBox = new TalkBox(this);
                    Main.ToolBar.MainGrid.Children.Add(TalkBox);
                }
                else if (Set["CGPT"][(gbol)"enable"])
                {
                    TalkBox = new TalkBoxAPI(this);
                    Main.ToolBar.MainGrid.Children.Add(TalkBox);
                }
                LoadingText.Content = "正在加载游戏";
                var m = new System.Windows.Controls.MenuItem()
                {
                    Header = "MOD管理",
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                };
                m.Click += (x, y) =>
                {
                    Main.ToolBar.Visibility = Visibility.Collapsed;
                    Topmost = false;
                    winSetting.MainTab.SelectedIndex = 5;
                    winSetting.Show();
                };
                Main.FunctionSpendHandle += lowStrength;
                Main.ToolBar.MenuMODConfig.Items.Add(m);
                try
                {
                    //加载游戏创意工坊插件
                    foreach (MainPlugin mp in Plugins)
                        mp.LoadPlugin();
                }
                catch (Exception e)
                {
                    new winReport(this, "由于插件引起的游戏启动错误\n" + e.ToString()).Show();
                }
                Foods.ForEach(item => item.LoadImageSource(this));

                Main.DefaultClickAction = () =>
                {
                    if (new TimeSpan(DateTime.Now.Ticks - lastclicktime).TotalSeconds > 20)
                    {
                        lastclicktime = DateTime.Now.Ticks;
                        var v = rndtext[Function.Rnd.Next(rndtext.Count)];
                        Main.Say(v.Item1, v.Item2);
                    }
                };
                Main.PlayVoiceVolume = Set.VoiceVolume;
                DisplayGrid.Child = Main;
                Task.Run(() =>
                {
                    while (Main.IsWorking)
                    {
                        Thread.Sleep(100);
                    }
                    Dispatcher.Invoke(() => LoadingText.Visibility = Visibility.Collapsed);
                });

                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Setting, "退出桌宠", () => { Main.ToolBar.Visibility = Visibility.Collapsed; Close(); });
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Setting, "开发控制台", () => { Main.ToolBar.Visibility = Visibility.Collapsed; new winConsole(this).Show(); });
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Setting, "反馈中心", () => { Main.ToolBar.Visibility = Visibility.Collapsed; new winReport(this).Show(); });
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Setting, "设置面板", () =>
                {
                    Main.ToolBar.Visibility = Visibility.Collapsed;
                    Topmost = false;
                    winSetting.Show();
                });

                //this.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Res/TopLogo2019.PNG")));

                //Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "喂食测试", () =>
                //    {
                //        Main.ToolBar.Visibility = Visibility.Collapsed;
                //        IRunImage eat = (IRunImage)Core.Graph.FindGraph(GraphType.Eat, GameSave.ModeType.Nomal);
                //        var b = Main.FindDisplayBorder(eat);
                //        eat.Run(b, new BitmapImage(new Uri("pack://application:,,,/Res/汉堡.png")), Main.DisplayToNomal);
                //    }
                //);
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "吃饭", () =>
                    {
                        winBetterBuy.Show(Food.FoodType.Meal);
                    }
                );
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "喝水", () =>
                {
                    winBetterBuy.Show(Food.FoodType.Drink);
                }
              );
                Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "药品", () =>
                {
                    winBetterBuy.Show(Food.FoodType.Drug);
                }
              );
                Main.SetMoveMode(Set.AllowMove, Set.SmartMove, Set.SmartMoveInterval * 1000);
                Main.SetLogicInterval((int)(Set.LogicInterval * 1000));
                if (Set.MessageBarOutside)
                    Main.MsgBar.SetPlaceOUT();

                //加载图标
                notifyIcon = new NotifyIcon();
                notifyIcon.Text = "虚拟桌宠模拟器";
                ContextMenu m_menu;

                m_menu = new ContextMenu();
                m_menu.MenuItems.Add(new MenuItem("操作教程", (x, y) => { Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\Tutorial.html"); }));
                m_menu.MenuItems.Add(new MenuItem("重置状态", (x, y) =>
                {
                    Main.CleanState();
                    Main.DisplayToNomal();
                    Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                    Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
                }));
                m_menu.MenuItems.Add(new MenuItem("反馈中心", (x, y) => { new winReport(this).Show(); }));
                m_menu.MenuItems.Add(new MenuItem("开发控制台", (x, y) => { new winConsole(this).Show(); }));

                m_menu.MenuItems.Add(new MenuItem("设置面板", (x, y) =>
                {
                    Topmost = false;
                    winSetting.Show();
                }));
                m_menu.MenuItems.Add(new MenuItem("退出桌宠", (x, y) => Close()));

                LoadDIY();

                notifyIcon.ContextMenu = m_menu;

                notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/vpeticon.ico")).Stream);

                notifyIcon.Visible = true;
                notifyIcon.BalloonTipClicked += (a, b) =>
                {
                    Topmost = false;
                    winSetting.Show();
                };

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Tutorial.html") && Set["SingleTips"].GetDateTime("tutorial") <= new DateTime(2023, 6, 20))
                {
                    Set["SingleTips"].SetDateTime("tutorial", DateTime.Now);
                    Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\Tutorial.html");
                }
                if (!Set["SingleTips"].GetBool("helloworld"))
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(2000);
                        Set["SingleTips"].SetBool("helloworld", true);
                        notifyIcon.ShowBalloonTip(10, "你好" + (IsSteamUser ? Steamworks.SteamClient.Name : Environment.UserName),
                        "欢迎使用虚拟桌宠模拟器!\n如果遇到桌宠爬不见了,可以在我这里设置居中或退出桌宠", ToolTipIcon.Info);
                        Thread.Sleep(2000);
                        Main.Say("欢迎使用虚拟桌宠模拟器\n这是个中期的测试版,若有bug请多多包涵\n欢迎加群虚拟主播模拟器430081239或在菜单栏-管理-反馈中提交bug或建议", GraphCore.Helper.SayType.Shining);
                    });
                }
                else if (Set["SingleTips"].GetDateTime("update") <= new DateTime(2023, 6, 21))
                {
                    if (Set["SingleTips"].GetDateTime("update") > new DateTime(2023, 6, 19)) // 上次更新日期时间
                        notifyIcon.ShowBalloonTip(10, "更新通知 06/21", //本次更新内容
                        "添加喝水,口渴,睡觉,肚子饿,喂食,等动画\n新增低状态的说话文本,更新食物图片\n提高了学习收益和降低了学习消耗", ToolTipIcon.Info);
                    else// 累计更新内容
                        notifyIcon.ShowBalloonTip(10, "更新通知 06/21",
                    "现已支持数据计算,桌宠现在需要进行吃饭喝水等\n更新了新的状态动画文件\n新增自动备份存档功能\n数据计算数据相关优化", ToolTipIcon.Info);
                    Set["SingleTips"].SetDateTime("update", DateTime.Now);
                }
                Save();
            }));
        }

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Save();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Save();
            try
            {
                //关闭所有插件
                foreach (MainPlugin mp in Plugins)
                    mp.EndGame();
            }
            catch { }
            Main?.Dispose();
            notifyIcon?.Dispose();
            System.Environment.Exit(0);
        }


        //public void DEBUGValue()
        //{
        //  Dispatcher.Invoke(() =>
        //  {
        //    Console.WriteLine("Left:" + mwc.GetWindowsDistanceLeft());
        //    Console.WriteLine("Right:" + mwc.GetWindowsDistanceRight());
        //  });
        //  Thread.Sleep(1000);
        //  DEBUGValue(); 
        //}

    }
}
