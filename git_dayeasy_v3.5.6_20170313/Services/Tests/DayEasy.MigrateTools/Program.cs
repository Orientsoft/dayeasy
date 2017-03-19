
using DayEasy.MigrateTools.Migrate;
using DayEasy.Utility.Logging;
using System;
using System.Text;

namespace DayEasy.MigrateTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("+++================================++++");
            sb.AppendLine("请输入以下命令：");
            sb.AppendLine("1.生成得一号");
            sb.AppendLine("2.转移机构数据");
            sb.AppendLine("3.导入机构数据,agency.txt");
            sb.AppendLine("4.转移班级到班级圈");
            sb.AppendLine("5.班级圈成员");
            sb.AppendLine("6.圈子默认logo");
            sb.AppendLine("7.圈子动态");
            sb.AppendLine("8.生成协同试卷分配");
            sb.AppendLine("9.完成阅卷__batches.txt");
            sb.AppendLine("10.默认管理员");
            sb.AppendLine("11.重新计算分数__batches.txt");
            sb.AppendLine("12.默认标记/客观题错题_batches.txt");
            sb.AppendLine("13.自动批阅试卷图片");
            sb.AppendLine("14.分配协同试卷图片");
            sb.AppendLine("15.重新批阅图片客观题__pictures.txt");
            sb.AppendLine("16.重新批阅图片客观题(已完成阅卷)__pictures.txt");
            sb.AppendLine("17.更新错题库");
            sb.AppendLine("18.导出协同各题得分明细");
            sb.AppendLine("19.同步试卷答案");
            sb.AppendLine("20.同步用户机构信息");
            sb.AppendLine("21.检测协同图片任务，MongoDB数据生成__joints.txt");
            sb.AppendLine("22.重新批阅客观题__batches.txt");
            sb.AppendLine("23.套打顺序");
            sb.AppendLine("24.重新提交图片___joints.txt");
            sb.AppendLine("25.修改客观题答案___changeAnswers.txt");
            sb.AppendLine("26.重新发送动态");
            sb.AppendLine("27.转移下载日志");
            sb.AppendLine("28.导出班级成绩");
            sb.AppendLine("29.导出协同成绩");
            sb.AppendLine("30.同步知识点统计");
            sb.AppendLine("0.退出");
            sb.AppendLine("+++================================++++");
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write(sb);
            var isRunning = true;
            while (isRunning)
            {
                var mission = Console.ReadLine();
                try
                {
                    switch (mission)
                    {
                        case "1":
                            Console.WriteLine("生成得一号");
                            new UserMigrate().GenerateDCode();
                            break;
                        case "2":
                            Console.WriteLine("转移机构数据");
                            new AgencyMigrate().Main();
                            break;
                        case "3":
                            Console.WriteLine("导入机构数据");
                            new AgencyMigrate().Import();
                            break;
                        case "4":
                            Console.WriteLine("转移班级到班级圈");
                            new ClassMigrate().Main();
                            break;
                        case "5":
                            Console.WriteLine("班级圈成员");
                            new ClassMigrate().Members();
                            break;
                        case "6":
                            Console.WriteLine("圈子默认logo");
                            new ClassMigrate().DefaultAvatar();
                            break;
                        case "7":
                            Console.WriteLine("圈子动态");
                            new DynamicMigrate().Main();
                            break;
                        case "8":
                            Console.WriteLine("生成协同试卷分配");
                            new JointPicture().CheckPicture();
                            break;
                        case "9":
                            Console.WriteLine("完成阅卷");
                            new FinishMarking().Finish();
                            break;
                        case "10":
                            Console.WriteLine("默认管理员");
                            new ClassMigrate().GroupManager();
                            break;
                        case "11":
                            Console.WriteLine("重新计算分数");
                            new RemakringObjective().ReCalcScores();
                            break;
                        case "12":
                            Console.WriteLine("默认标记/客观题错题");
                            new FinishMarking().SetRightIconAndScoreMark();
                            break;
                        case "13":
                            Console.WriteLine("自动批阅试卷图片");
                            new HandinTask().Main();
                            break;
                        case "14":
                            Console.WriteLine("分配协同试卷图片");
                            new HandinTask().PictureDist();
                            break;
                        case "15":
                            Console.WriteLine("重新批阅图片客观题");
                            new RemakringObjective().Remarking();
                            break;
                        case "16":
                            Console.WriteLine("重新批阅图片客观题(已完成阅卷)");
                            new RemakringObjective().Remarking(true);
                            break;
                        case "17":
                            Console.WriteLine("更新错题库");
                            new ErrorQuestionMigrate().Check();
                            break;
                        case "18":
                            Console.WriteLine("导出协同各题得分明细");
                            new JointManage().ExportDetail();
                            break;
                        case "19":
                            Console.WriteLine("同步试卷答案");
                            new PaperMigrate().ChangePaperAnswer();
                            break;
                        case "20":
                            Console.WriteLine("同步用户机构信息");
                            new UserMigrate().UserAgency();
                            break;
                        case "21":
                            Console.WriteLine("检测协同图片任务，MongoDB数据生成");
                            new JointPicture().PictureTask();
                            break;
                        case "22":
                            Console.WriteLine("重新批阅客观题");
                            new RemakringObjective().RemarkingObjective();
                            break;
                        case "23":
                            Console.WriteLine("套打顺序");
                            new JointPicture().PictureSorts();
                            break;
                        case "24":
                            Console.WriteLine("重新提交图片");
                            new HandinTask().ReCommit();
                            break;
                        case "25":
                            Console.WriteLine("修改客观题答案");
                            new HandinTask().ChangeAnswers();
                            break;
                        case "26":
                            Console.WriteLine("重新发送动态");
                            new FinishMarking().SendMessage();
                            break;
                        case "27":
                            Console.WriteLine("转移下载日志");
                            new AgencyMigrate().DownloadLog();
                            break;
                        case "28":
                            Console.WriteLine("导出班级成绩");
                            new ExportTask().ClassExaminations();
                            break;
                        case "29":
                            Console.WriteLine("导出协同成绩");
                            new ExportTask().JointStatistics();
                            break;
                        case "30":
                            Console.WriteLine("同步知识点统计");
                            new StatisticMigrate().KpStatistic();
                            break;
                        case "0":
                            isRunning = false;
                            Console.WriteLine("已退出");
                            break;
                        default:
                            Console.WriteLine("命令无效~！");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    LogManager.Logger("task").Error(ex.Message, ex);
                }
            }
        }
    }
}
