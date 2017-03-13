using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Enum;
using DayEasy.Paper.Services.Model;

namespace DayEasy.Paper.Services.Helper.AutoMakePaper
{
    /// <summary>
    /// 试卷工厂
    /// </summary>
    public class PaperFactory
    {
        #region 根据用户的设置，获取试卷的构成属性（查询条件组合）
        /// <summary>
        /// 根据用户的设置，获取试卷的构成属性（查询条件组合）
        /// </summary>
        /// <param name="qTypeDic">题型和数量</param>
        /// <param name="kpsDic">知识点集合和占比</param>
        /// <param name="difficulty">试卷难度</param>
        /// <returns></returns>
        public List<PaperProperty> GetPaperProperties(Dictionary<int, int> qTypeDic, Dictionary<string, decimal> kpsDic, byte difficulty)
        {
            if (qTypeDic != null && qTypeDic.Count > 0)
            {
                var paperProperties = new List<PaperProperty>();

                //策略：大于最低比例阀值的按照原比例，低于的按照 0-10%，10%-20% 的进行分组随机
                var kpsDicTemp = new Dictionary<string, decimal>();
                if (kpsDic.Count > (int)PaperPoint.RandomCount)
                {
                    //最低比例阀值
                    const decimal smallPercent = (int)PaperPoint.AveragePercent / (decimal)100;

                    //大于最低比例阀值
                    var morethanList = kpsDic.Where(d => d.Value >= smallPercent).ToList();
                    foreach (var kp in morethanList)
                    {
                        kpsDicTemp.Add(kp.Key, kp.Value);
                    }

                    var totalCount = qTypeDic.Sum(u => u.Value);//总的题目数量
                    //小于最低比例阀值 并且 >=10%
                    var kps10Two20Dic = RandomKpBag(totalCount, kpsDic, 1);
                    if (kps10Two20Dic.Any())
                    {
                        foreach (var kp in kps10Two20Dic)
                        {
                            kpsDicTemp.Add(kp.Key, kp.Value);
                        }
                    }
                    //小于最低比例阀值 并且 <10%
                    var kpslessThanPercent10Dic = RandomKpBag(totalCount, kpsDic, 2);
                    if (kpslessThanPercent10Dic.Any())
                    {
                        foreach (var kp in kpslessThanPercent10Dic)
                        {
                            kpsDicTemp.Add(kp.Key, kp.Value);
                        }
                    }
                }
                else
                {
                    kpsDicTemp = kpsDic;
                }

                //获取难度系数星级分布
                var qStars = GetDifficultyPercent(difficulty);

                //大比例优先
                var kps = kpsDicTemp.OrderByDescending(d => d.Value);
                var difficulties = qStars.OrderByDescending(d => d.Value);

                foreach (var qType in qTypeDic)//遍历题型
                {
                    var totalTypeNum = 0;//记录当前题型已经加入的数量

                    foreach (var kp in kps)//知识点优先
                    {
                        var kpNum = qType.Value * kp.Value;
                        var currentKpNum = Convert.ToInt32(kpNum < 1 ? Math.Ceiling(kpNum) : Math.Floor(kpNum));//当前知识点的题目数量

                        int difficTotalCount = 0;
                        foreach (var diffic in difficulties)//遍历难度系数
                        {
                            if (totalTypeNum == qType.Value)
                            {
                                break;
                            }

                            var difficNum = currentKpNum * diffic.Value;
                            var currentDifficNum = Convert.ToInt32(difficNum < 1 ? Math.Ceiling(difficNum) : Math.Floor(difficNum));//当前知识点的当前难度题目数量

                            if (difficTotalCount + currentDifficNum > currentKpNum)
                            {
                                currentDifficNum = currentKpNum - difficTotalCount;
                            }

                            if (totalTypeNum + currentDifficNum <= qType.Value && currentDifficNum > 0)//判断当前题型总数量
                            {
                                var property = new PaperProperty();
                                property.QType = qType.Key;
                                property.Points = new List<string>() { kp.Key };
                                property.Count = currentDifficNum;
                                property.Difficulties = new List<double>() { 3 };
                                if (diffic.Key == 12)
                                {
                                    property.Difficulties = new List<double>() { 1, 2 };
                                }
                                else if (diffic.Key == 45)
                                {
                                    property.Difficulties = new List<double>() { 4, 5 };
                                }

                                paperProperties.Add(property);//添加到结果集

                                totalTypeNum += currentDifficNum;
                                difficTotalCount += currentDifficNum;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    //判断是否当前题型题目数量不够
                    if (totalTypeNum < qType.Value)
                    {
                        //查找到比例最大的知识点和比例最大的难度系数
                        var property = new PaperProperty();
                        property.QType = qType.Key;
                        property.Points = new List<string>() { kps.First().Key };
                        property.Count = qType.Value - totalTypeNum;
                        property.Difficulties = new List<double>() { 3 };
                        if (difficulties.First().Key == 12)
                        {
                            property.Difficulties = new List<double>() { 1, 2 };
                        }
                        else if (difficulties.First().Key == 45)
                        {
                            property.Difficulties = new List<double>() { 4, 5 };
                        }

                        paperProperties.Add(property);//添加到结果集
                    }
                }

                return paperProperties;
            }

            return null;
        }
        #endregion

        #region 获取试卷难度系数对应的题目难度系数比例
        /// <summary>
        /// 获取试卷难度系数对应的题目难度系数比例
        /// </summary>
        /// <param name="difficulty"></param>
        private Dictionary<int, decimal> GetDifficultyPercent(byte difficulty)
        {
            var difficulties = new Dictionary<int, decimal>();
            switch (difficulty)
            {
                case (byte)PaperDifficulty.Easy:
                    difficulties.Add(12, (decimal)Easy.Star12 / 100);
                    difficulties.Add(3, (decimal)Easy.Star3 / 100);
                    difficulties.Add(45, (decimal)Easy.Star45 / 100);
                    break;
                case (byte)PaperDifficulty.Difficulty:
                    difficulties.Add(12, (decimal)Difficulty.Star12 / 100);
                    difficulties.Add(3, (decimal)Difficulty.Star3 / 100);
                    difficulties.Add(45, (decimal)Difficulty.Star45 / 100);
                    break;
                default:
                    difficulties.Add(12, (decimal)Normal.Star12 / 100);
                    difficulties.Add(3, (decimal)Normal.Star3 / 100);
                    difficulties.Add(45, (decimal)Normal.Star45 / 100);
                    break;
            }

            return difficulties;
        }
        #endregion

        #region 打包知识点随机
        /// <summary>
        /// 打包知识点随机
        /// </summary>
        private Dictionary<string, decimal> RandomKpBag(int totalCount, Dictionary<string, decimal> kpsDic, int type)
        {
            var kpsDicTemp = new Dictionary<string, decimal>();

            //最低比例阀值
            const decimal smallPercent = (int)PaperPoint.AveragePercent / (decimal)100;

            List<KeyValuePair<string, decimal>> tempkpsList = null;
            if (type == 1)//小于最低比例阀值 并且 >=10%
            {
                tempkpsList = kpsDic.Where(d => d.Value < smallPercent && d.Value >= (decimal)0.1).ToList();
            }
            else//小于最低比例阀值 并且 <10%
            {
                tempkpsList = kpsDic.Where(d => d.Value < (decimal)0.1).ToList();
            }

            if (tempkpsList.Any())
            {
                var sameKpPercentSum = tempkpsList.Sum(d => d.Value);//打包知识点总共占比
                var sameKpArr = tempkpsList.Select(d => d.Key).ToArray();//打包知识点集合

                //打包知识点总共产生题目数量
                var tempCount = totalCount * sameKpPercentSum;
                var currentDifficNum = Convert.ToInt32(tempCount < 1 ? Math.Ceiling(tempCount) : Math.Floor(tempCount));

                //打包知识点中随机知识点的每个占比
                var perRandomPercent = sameKpPercentSum / currentDifficNum;

                for (int i = 0; i < currentDifficNum; i++)//随机知识点分布
                {
                    var r = new Random(Guid.NewGuid().GetHashCode());
                    var index = r.Next(currentDifficNum * 10, currentDifficNum * 100);//随机数
                    var arrIndex = index % sameKpArr.Length;//随机知识点下标

                    var currentKp = sameKpArr[arrIndex];
                    if (kpsDicTemp.Keys.Contains(currentKp))
                    {
                        kpsDicTemp[currentKp] += perRandomPercent;
                    }
                    else
                    {
                        kpsDicTemp.Add(sameKpArr[arrIndex], perRandomPercent);
                    }
                }
            }

            return kpsDicTemp;
        }
        #endregion
    }
}
