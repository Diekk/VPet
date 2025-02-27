﻿using LinePutScript;
using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// 图像显示核心
    /// </summary>
    public class GraphCore
    {


        public GraphCore()
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            CommConfig["Cache"] = new List<string>();
        }

        public static string CachePath = AppDomain.CurrentDomain.BaseDirectory + @"\cache";
        /// <summary>
        /// 动画类型
        /// </summary>
        public enum GraphType
        {
            //  名称 ?描述? ?动作? 大约时间(L:(1000+)M:(200-800)S:(125))
            /// <summary>
            /// 不被启用/使用的 不包含在GrapType S
            /// </summary>
            Not_Able,
            /// <summary>
            /// 被提起动态 (循环) L
            /// </summary>
            Raised_Dynamic,
            /// <summary>
            /// 被提起静态 (开始) L
            /// </summary>
            Raised_Static_A_Start,
            /// <summary>
            /// 被提起静态 (循环) L
            /// </summary>
            Raised_Static_B_Loop,
            /// <summary>
            /// 从上向右爬 (循环) M
            /// </summary>
            Climb_Top_Right,
            /// <summary>
            /// 从上向左爬 (循环) M
            /// </summary>
            Climb_Top_Left,
            /// <summary>
            /// 爬起向右 M
            /// </summary>
            Climb_Up_Right,
            /// <summary>
            /// 爬起向左 M
            /// </summary>
            Climb_Up_Left,
            /// <summary>
            /// 从右边爬 (开始) S
            /// </summary>
            Climb_Right_A_Start,
            /// <summary>
            /// 从左边爬 (开始) S
            /// </summary>
            Climb_Left_A_Start,
            /// <summary>
            /// 从右边爬 (循环) M
            /// </summary>
            Climb_Right_B_Loop,
            /// <summary>
            /// 从左边爬 (循环) M
            /// </summary>
            Climb_Left_B_Loop,
            /// <summary>
            /// 呼吸 (循环) L
            /// </summary>
            Default,
            /// <summary>
            /// 摸头 (开始) S
            /// </summary>
            Touch_Head_A_Start,
            /// <summary>
            /// 摸头 (循环) M
            /// </summary>
            Touch_Head_B_Loop,
            /// <summary>
            /// 摸头 (结束) S
            /// </summary>
            Touch_Head_C_End,
            /// <summary>
            /// 摸身体 (开始) S
            /// </summary>
            Touch_Body_A_Start,
            /// <summary>
            /// 摸身体 (循环) M
            /// </summary>
            Touch_Body_B_Loop,
            /// <summary>
            /// 摸身体 (结束) S
            /// </summary>
            Touch_Body_C_End,
            /// <summary>
            /// 爬行向右 (开始) S
            /// </summary>
            Crawl_Right_A_Start,
            /// <summary>
            /// 爬行向右 (循环) M
            /// </summary>
            Crawl_Right_B_Loop,
            /// <summary>
            /// 爬行向右 (结束) S
            /// </summary>
            Crawl_Right_C_End,
            /// <summary>
            /// 爬行向左 (开始) S
            /// </summary>
            Crawl_Left_A_Start,
            /// <summary>
            /// 爬行向左 (循环) M
            /// </summary>
            Crawl_Left_B_Loop,
            /// <summary>
            /// 爬行向左 (结束) S
            /// </summary>
            Crawl_Left_C_End,
            /// <summary>
            /// 下蹲 (开始) S
            /// </summary>
            Squat_A_Start,
            /// <summary>
            /// 下蹲 (循环) M
            /// </summary>
            Squat_B_Loop,
            /// <summary>
            /// 下蹲 (结束) S
            /// </summary>
            Squat_C_End,
            /// <summary>
            /// 下落向左 (开始) S
            /// </summary>
            Fall_Left_A_Start,
            /// <summary>
            /// 下落向左 (循环) M
            /// </summary>
            Fall_Left_B_Loop,
            /// <summary>
            /// 下落向左 (结束) S
            /// </summary>
            Fall_Left_C_End,
            /// <summary>
            /// 下落向右 (开始) S
            /// </summary>
            Fall_Right_A_Start,
            /// <summary>
            /// 下落向右 (循环) M
            /// </summary>
            Fall_Right_B_Loop,
            /// <summary>
            /// 下落向右 (结束) S
            /// </summary>
            Fall_Right_C_End,
            /// <summary>
            /// 走路向右 (开始) S
            /// </summary>
            Walk_Right_A_Start,
            /// <summary>
            /// 走路向右 (循环) M
            /// </summary>
            Walk_Right_B_Loop,
            /// <summary>
            /// 走路向右 (结束) S
            /// </summary>
            Walk_Right_C_End,
            /// <summary>
            /// 走路向左 (开始) S
            /// </summary>
            Walk_Left_A_Start,
            /// <summary>
            /// 走路向左 (循环) M
            /// </summary>
            Walk_Left_B_Loop,
            /// <summary>
            /// 走路向左 (结束) S
            /// </summary>
            Walk_Left_C_End,
            /// <summary>
            /// 无聊 (开始) S
            /// </summary>
            Boring_A_Start,
            /// <summary>
            /// 无聊 (循环) M
            /// </summary>
            Boring_B_Loop,
            /// <summary>
            /// 无聊 (结束) S
            /// </summary>
            Boring_C_End,
            /// <summary>
            /// 睡觉 (开始) S
            /// </summary>
            Sleep_A_Start,
            /// <summary>
            /// 睡觉 (循环) M
            /// </summary>
            Sleep_B_Loop,
            /// <summary>
            /// 睡觉 (结束) S
            /// </summary>
            Sleep_C_End,
            /// <summary>
            /// 说话 (开始) S
            /// </summary>
            Say_Serious_A_Start,
            /// <summary>
            /// 说话 (循环) M
            /// </summary>
            Say_Serious_B_Loop,
            /// <summary>
            /// 说话 (结束) S
            /// </summary>
            Say_Serious_C_End,
            /// <summary>
            /// 说话 (开始) S
            /// </summary>
            Say_Shining_A_Start,
            /// <summary>
            /// 说话 (循环) M
            /// </summary>
            Say_Shining_B_Loop,
            /// <summary>
            /// 说话 (结束) S
            /// </summary>
            Say_Shining_C_End,
            /// <summary>
            /// 说话 (开始) S
            /// </summary>
            Say_Self_A_Start,
            /// <summary>
            /// 说话 (循环) M
            /// </summary>
            Say_Self_B_Loop,
            /// <summary>
            /// 说话 (结束) S
            /// </summary>
            Say_Self_C_End,
            /// <summary>
            /// 待机 模式1 (开始) S
            /// </summary>
            Idel_StateONE_A_Start,
            /// <summary>
            /// 待机 模式1 (循环) M
            /// </summary>
            Idel_StateONE_B_Loop,
            /// <summary>
            /// 待机 模式1 (结束) S
            /// </summary>
            Idel_StateONE_C_End,
            /// <summary>
            /// 待机 模式2 (开始) S
            /// </summary>
            Idel_StateTWO_A_Start,
            /// <summary>
            /// 待机 模式2 (循环) M
            /// </summary>
            Idel_StateTWO_B_Loop,
            /// <summary>
            /// 待机 模式2 (结束) S
            /// </summary>
            Idel_StateTWO_C_End,
            /// <summary>
            /// 开机 M
            /// </summary>
            StartUP,
            /// <summary>
            /// 关机 M
            /// </summary>
            Shutdown,
            /// <summary>
            /// 学习 (开始) S
            /// </summary>
            Study_A_Start,
            /// <summary>
            /// 学习 (循环) M
            /// </summary>
            Study_B_Loop,
            /// <summary>
            /// 学习 (结束) S
            /// </summary>
            Study_C_End,
            /// <summary>
            /// 工作 (开始) S
            /// </summary>
            WorkONE_A_Start,
            /// <summary>
            /// 工作 (循环) M
            /// </summary>
            WorkONE_B_Loop,
            /// <summary>
            /// 工作 (结束) S
            /// </summary>
            WorkONE_C_End,
            /// <summary>
            /// 直播 (开始) S
            /// </summary>
            WorkTWO_A_Start,
            /// <summary>
            /// 直播 (循环) M
            /// </summary>
            WorkTWO_B_Loop,
            /// <summary>
            /// 直播 (结束) S
            /// </summary>
            WorkTWO_C_End,
            /// <summary>
            /// 口渴 M
            /// </summary>
            Switch_Thirsty,
            /// <summary>
            /// 饥饿 M
            /// </summary>
            Switch_Hunger,
            /// <summary>
            /// 吃东西 M
            /// </summary>
            Eat,
            /// <summary>
            /// 喝东西 M
            /// </summary>
            Drink,
        }

        /// <summary>
        /// 图像字典
        /// </summary>
        public Dictionary<GraphType, List<IGraph>> Graphs = new Dictionary<GraphType, List<IGraph>>();
        /// <summary>
        /// 图像字典(不被主要引用)
        /// </summary>
        public Dictionary<string, List<IGraph>> CommGraphs = new Dictionary<string, List<IGraph>>();
        /// <summary>
        /// 通用UI资源
        /// </summary>
        public Dictionary<string, UIElement> CommUIElements = new Dictionary<string, UIElement>();
        /// <summary>
        /// 通用设置属性/方法
        /// </summary>
        public Dictionary<string, dynamic> CommConfig = new Dictionary<string, dynamic>();
        /// <summary>
        /// 添加动画
        /// </summary>
        /// <param name="graph">动画</param>
        /// <param name="type">类型</param>
        public void AddGraph(IGraph graph, GraphType type)
        {
            //switch (graph.GraphType)
            //{
            //    case GraphType.Default:
            //    case GraphType.Boring_B_Loop:
            //    case GraphType.Squat_B_Loop:
            //        graph.IsLoop = true;
            //        break;
            //}//循环真要不得,要做随机循环
            if (!Graphs.ContainsKey(type))
            {
                Graphs.Add(type, new List<IGraph>());
            }
            Graphs[type].Add(graph);
        }
        /// <summary>
        /// 添加动画
        /// </summary>
        /// <param name="graph">动画</param>
        public void AddGraph(IGraph graph) => AddGraph(graph, graph.GraphType);
        /// <summary>
        /// 添加动画
        /// </summary>
        /// <param name="graph">动画</param>
        /// <param name="type">类型</param>
        public void AddCOMMGraph(IGraph graph, string Name)
        {
            if (!CommGraphs.ContainsKey(Name))
            {
                CommGraphs.Add(Name, new List<IGraph>());
            }
            CommGraphs[Name].Add(graph);
        }
        /// <summary>
        /// 添加动画 自动创建
        /// </summary>
        /// <param name="path">位置</param>
        /// <param name="modetype">状态类型</param>
        /// <param name="graphtype">动画类型</param>
        ///// <param name="storemem">是否储存到内存以节约加载</param>
        public void AddGraph(string path, GameSave.ModeType modetype, GraphType graphtype)//, bool storemem = false)
        {
            var paths = new DirectoryInfo(path).GetFiles();
            if (paths.Length == 0)
                return;
            if (paths.Length == 1)
                AddGraph(new Picture(this, paths[0].FullName, modetype, graphtype,
                    int.Parse(paths[0].Name.Split('.').Reverse().ToArray()[1].Split('_').Last())), graphtype);
            else
                AddGraph(new PNGAnimation(this, path, paths, modetype, graphtype), graphtype);
        }
        /// <summary>
        /// 随机数字典(用于确保随机动画不会错位)
        /// </summary>
        public Dictionary<int, int> RndGraph = new Dictionary<int, int>();

        /// <summary>
        /// 查找动画
        /// </summary>
        /// <param name="type">动画类型</param>
        /// <param name="mode">状态类型,找不到就找相同动画类型</param>
        ///// <param name="storernd">是否储存随机数字典</param>
        public IGraph FindGraph(GraphType type, GameSave.ModeType mode)
        {
            if (Graphs.ContainsKey(type))
            {
                var list = Graphs[type].FindAll(x => x.ModeType == mode);
                if (list.Count > 0)
                {
                    if (list.Count == 1)
                        return list[0];
                    if (GraphConfig.StoreRnd.Contains(type.ToString()))
                        if (RndGraph.TryGetValue(list.Count, out int index))
                        {
                            return list[index];
                        }
                        else
                        {
                            index = Function.Rnd.Next(list.Count);
                            RndGraph.Add(list.Count, index);
                            return list[index];
                        }
                    else
                        return list[Function.Rnd.Next(list.Count)];

                }
                int i = (int)mode + 1;
                if (i < 3)
                {
                    //向下兼容的动画
                    list = Graphs[type].FindAll(x => x.ModeType == (GameSave.ModeType)i);
                    if (list.Count > 0)
                        return list[Function.Rnd.Next(list.Count)];
                }
                i = (int)mode - 1;
                if (i >= 0)
                {
                    //向上兼容的动画
                    list = Graphs[type].FindAll(x => x.ModeType == (GameSave.ModeType)i);
                    if (list.Count > 0)
                        return list[Function.Rnd.Next(list.Count)];
                }
                //如果实在找不到,就走随机数
                //if (mode != GameSave.ModeType.Ill)
                //{
                list = Graphs[type];
                if (list.Count > 0)
                    return list[Function.Rnd.Next(list.Count)];
                //}
            }
            return null;// FindGraph(GraphType.Default, mode);
        }
        /// <summary>
        /// 查找动画
        /// </summary>
        /// <param name="type">动画类型</param>
        /// <param name="mode">状态类型,找不到就找相同动画类型</param>
        ///// <param name="storernd">是否储存随机数字典</param>
        public IGraph FindCOMMGraph(string Name, GameSave.ModeType mode)
        {
            if (CommGraphs.ContainsKey(Name))
            {
                var list = CommGraphs[Name].FindAll(x => x.ModeType == mode);
                if (list.Count > 0)
                {
                    if (list.Count == 1)
                        return list[0];
                    if (GraphConfig.StoreRnd.Contains(Name))
                        if (RndGraph.TryGetValue(list.Count, out int index))
                        {
                            return list[index];
                        }
                        else
                        {
                            index = Function.Rnd.Next(list.Count);
                            RndGraph.Add(list.Count, index);
                            return list[index];
                        }
                    else
                        return list[Function.Rnd.Next(list.Count)];

                }
                if (mode != GameSave.ModeType.Ill)
                {
                    list = CommGraphs[Name].FindAll(x => x.ModeType != GameSave.ModeType.Ill);
                    if (list.Count > 0)
                        return list[Function.Rnd.Next(list.Count)];
                }
            }
            return null;// FindGraph(GraphType.Default, mode);
        }
        static string[] graphtypevalue = null;
        /// <summary>
        /// 动画类型默认前文本
        /// </summary>
        public static string[] GraphTypeValue
        {
            get
            {
                if (graphtypevalue == null)
                {
                    List<string> gtv = new List<string>();
                    foreach (string v in Enum.GetNames(typeof(GraphType)))
                    {
                        gtv.Add(v.Replace("_Start", "").Replace("_Loop", "").Replace("_End", "").ToLower());
                    }
                    graphtypevalue = gtv.ToArray();
                }
                return graphtypevalue;
            }
        }

        public Config GraphConfig;
        /// <summary>
        /// 动画设置
        /// </summary>
        public class Config
        {
            /// <summary>
            /// 摸头触发位置
            /// </summary>
            public Point TouchHeadLocate;
            /// <summary>
            /// 提起触发位置
            /// </summary>
            public Point[] TouchRaisedLocate;
            /// <summary>
            /// 摸头触发大小
            /// </summary>
            public Size TouchHeadSize;
            /// <summary>
            /// 提起触发大小
            /// </summary>
            public Size[] TouchRaisedSize;

            /// <summary>
            /// 提起定位点
            /// </summary>
            public Point[] RaisePoint;
            /// <summary>
            /// 行走速度
            /// </summary>
            public double SpeedWalk;
            /// <summary>
            /// 侧边爬行速度
            /// </summary>
            public double SpeedClimb;
            /// <summary>
            /// 顶部爬行速度
            /// </summary>
            public double SpeedClimbTop;
            /// <summary>
            /// 爬行速度
            /// </summary>
            public double SpeedCrawl;
            /// <summary>
            /// 掉落速度 X轴
            /// </summary>
            public double SpeedFallX;
            /// <summary>
            /// 掉落速度 Y轴
            /// </summary>
            public double SpeedFallY;
            /// <summary>
            /// 定位爬行左边距离
            /// </summary>
            public double LocateClimbLeft;
            /// <summary>
            /// 定位爬行右边距离
            /// </summary>
            public double LocateClimbRight;
            /// <summary>
            /// 定位爬行上边距离
            /// </summary>
            public double LocateClimbTop;
            public List<string> StoreRnd = new List<string>();
            public ILine Str;
            public WorkTimer.UIStyleConfig UIStyleWork1;
            public WorkTimer.UIStyleConfig UIStyleWork2;
            public WorkTimer.UIStyleConfig UIStyleStudy;

            /// <summary>
            /// 初始化设置
            /// </summary>
            /// <param name="lps"></param>
            public Config(LpsDocument lps)
            {
                TouchHeadLocate = new Point(lps["touchhead"][(gdbe)"px"], lps["touchhead"][(gdbe)"py"]);
                TouchHeadSize = new Size(lps["touchhead"][(gdbe)"sw"], lps["touchhead"][(gdbe)"sh"]);
                TouchRaisedLocate = new Point[] {
                    new Point(lps["touchraised"][(gdbe)"happy_px"], lps["touchraised"][(gdbe)"happy_py"]),
                    new Point(lps["touchraised"][(gdbe)"nomal_px"], lps["touchraised"][(gdbe)"nomal_py"]),
                    new Point(lps["touchraised"][(gdbe)"poorcondition_px"], lps["touchraised"][(gdbe)"poorcondition_py"]),
                    new Point(lps["touchraised"][(gdbe)"ill_px"], lps["touchraised"][(gdbe)"ill_py"])
                };
                TouchRaisedSize = new Size[] {
                    new Size(lps["touchraised"][(gdbe)"happy_sw"], lps["touchraised"][(gdbe)"happy_sh"]),
                    new Size(lps["touchraised"][(gdbe)"nomal_sw"], lps["touchraised"][(gdbe)"nomal_sh"]),
                    new Size(lps["touchraised"][(gdbe)"poorcondition_sw"], lps["touchraised"][(gdbe)"poorcondition_sh"]),
                    new Size(lps["touchraised"][(gdbe)"ill_sw"], lps["touchraised"][(gdbe)"ill_sh"])
                };
                RaisePoint = new Point[] {
                    new Point(lps["raisepoint"][(gdbe)"happy_x"], lps["raisepoint"][(gdbe)"happy_y"]),
                    new Point(lps["raisepoint"][(gdbe)"nomal_x"], lps["raisepoint"][(gdbe)"nomal_y"]),
                    new Point(lps["raisepoint"][(gdbe)"poorcondition_x"], lps["raisepoint"][(gdbe)"poorcondition_y"]),
                    new Point(lps["raisepoint"][(gdbe)"ill_x"], lps["raisepoint"][(gdbe)"ill_y"])
                };
                var s = lps["speed"];
                SpeedWalk = s[(gdbe)"walk"];
                SpeedClimb = s[(gdbe)"climb"];
                SpeedClimbTop = s[(gdbe)"climbtop"];
                SpeedCrawl = s[(gdbe)"crawl"];
                SpeedFallX = s[(gdbe)"fallx"];
                SpeedFallY = s[(gdbe)"fally"];


                s = lps["locate"];
                LocateClimbLeft = s[(gdbe)"climbleft"];
                LocateClimbRight = s[(gdbe)"climbright"];
                LocateClimbTop = s[(gdbe)"climbtop"];

                foreach (Sub sub in lps["storernd"])
                {
                    StoreRnd.Add(sub.Name);
                }
                var line = lps.FindLine("UIStyleConfig", "work1");
                UIStyleWork1 = line == null ? new WorkTimer.UIStyleConfig() : LPSConvert.DeserializeObject<WorkTimer.UIStyleConfig>(line);
                line = lps.FindLine("UIStyleConfig", "work2");
                UIStyleWork2 = line == null ? new WorkTimer.UIStyleConfig() : LPSConvert.DeserializeObject<WorkTimer.UIStyleConfig>(line);
                line = lps.FindLine("UIStyleConfig", "study");
                UIStyleStudy = line == null ? new WorkTimer.UIStyleConfig() : LPSConvert.DeserializeObject<WorkTimer.UIStyleConfig>(line);
                Str = lps["str"];
            }
            /// <summary>
            /// 加载更多设置,新的替换后来的,允许空内容
            /// </summary>
            public void Set(LpsDocument lps)
            {
                if (lps.FindLine("touchhead") != null)
                {
                    TouchHeadLocate = new Point(lps["touchhead"][(gdbe)"px"], lps["touchhead"][(gdbe)"py"]);
                    TouchHeadSize = new Size(lps["touchhead"][(gdbe)"sw"], lps["touchhead"][(gdbe)"wh"]);
                }
                if (lps.FindLine("touchraised") != null)
                {
                    TouchRaisedLocate = new Point[] {
                    new Point(lps["touchraised"][(gdbe)"happy_px"], lps["touchraised"][(gdbe)"happy_py"]),
                    new Point(lps["touchraised"][(gdbe)"nomal_px"], lps["touchraised"][(gdbe)"nomal_py"]),
                    new Point(lps["touchraised"][(gdbe)"poorcondition_px"], lps["raistouchraisedepoint"][(gdbe)"poorcondition_py"]),
                    new Point(lps["touchraised"][(gdbe)"ill_px"], lps["touchraised"][(gdbe)"ill_py"])
                };
                    TouchRaisedSize = new Size[] {
                    new Size(lps["touchraised"][(gdbe)"happy_sx"], lps["touchraised"][(gdbe)"happy_sy"]),
                    new Size(lps["touchraised"][(gdbe)"nomal_sx"], lps["touchraised"][(gdbe)"nomal_sy"]),
                    new Size(lps["touchraised"][(gdbe)"poorcondition_sx"], lps["touchraised"][(gdbe)"poorcondition_sy"]),
                    new Size(lps["touchraised"][(gdbe)"ill_sx"], lps["touchraised"][(gdbe)"ill_sy"])
                };
                }
                if (lps.FindLine("raisepoint") != null)
                {
                    RaisePoint = new Point[] {
                    new Point(lps["raisepoint"].GetDouble("happy_x",RaisePoint[0].X), lps["raisepoint"].GetDouble("happy_y",RaisePoint[0].Y)),
                    new Point(lps["raisepoint"].GetDouble ("nomal_x",RaisePoint[1].X), lps["raisepoint"].GetDouble("nomal_y",RaisePoint[1].Y)),
                    new Point(lps["raisepoint"].GetDouble("poorcondition_x",RaisePoint[2].X), lps["raisepoint"].GetDouble("poorcondition_y",RaisePoint[2].Y)),
                    new Point(lps["raisepoint"].GetDouble("ill_x",RaisePoint[3].X), lps["raisepoint"].GetDouble("ill_y",RaisePoint[3].Y))
                };
                }
                var s = lps.FindLine("speed");
                if (s != null)
                {
                    SpeedWalk = s.GetDouble("walk", SpeedWalk);
                    SpeedClimb = s.GetDouble("climb", SpeedClimb);
                    SpeedClimbTop = s.GetDouble("climbtop", SpeedClimbTop);
                    SpeedCrawl = s.GetDouble("crawl", SpeedCrawl);
                    SpeedFallX = s.GetDouble("fallx", SpeedFallX);
                    SpeedFallY = s.GetDouble("fally", SpeedFallY);
                }
                s = lps.FindLine("locate");
                if (s != null)
                {
                    LocateClimbLeft = s.GetDouble("climbleft", LocateClimbLeft);
                    LocateClimbRight = s.GetDouble("climbright", LocateClimbRight);
                    LocateClimbTop = s.GetDouble("climbtop", LocateClimbTop);
                }
                foreach (Sub sub in lps["storernd"])
                {
                    if (!StoreRnd.Contains(sub.Name))
                        StoreRnd.Add(sub.Name);
                }
                foreach (Sub sub in lps["str"])
                {
                    if (!Str.Contains(sub.Name))
                        Str.Add(sub);
                }
            }
        }

        public static class Helper
        {
            public enum AnimatType
            {
                A_Start,
                B_Loop,
                C_End,
            }
            public enum SayType
            {
                /// <summary>
                /// 无:不执行动作
                /// </summary>
                None,
                /// <summary>
                /// 默认说话
                /// </summary>
                Default,
                /// <summary>
                /// 严肃
                /// </summary>
                Serious,
                /// <summary>
                /// 闪亮
                /// </summary>
                Shining,
                /// <summary>
                /// 自己
                /// </summary>
                Self,
                /// <summary>
                /// 错误
                /// </summary>
                Error
            }
            public static GraphType Convert(SayType sayType, AnimatType type)
            {
                string say = "Say_" + sayType.ToString() + '_' + type.ToString(); // (type == null ? "" : '_' + type.ToString());
                return (GraphType)Enum.Parse(typeof(GraphType), say);
            }
        }
    }
}
