using DayEasy.Utility;
using System.IO;
using System.Linq;

namespace DayEasy.MigrateTools.Migrate
{
    public static class TxtFileHelper
    {
        private const string BasePath = "config";
        private const string BatchesFile = "batches.txt";
        private const string JointsFile = "joints.txt";
        private const string PicturesFile = "pictures.txt";

        internal static string[] ReadConfig(this string fileName)
        {
            var path = Path.Combine(Utils.GetCurrentDir(), BasePath, fileName);
            if (!File.Exists(path))
                return new string[] { };
            var list = File.ReadAllLines(Path.Combine(Utils.GetCurrentDir(), BasePath, fileName));
            return list.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        }

        /// <summary> 批次号列表 </summary>
        public static string[] Batches()
        {
            return BatchesFile.ReadConfig();
        }

        /// <summary> 协同批次列表 </summary>
        public static string[] Joints()
        {
            return JointsFile.ReadConfig();
        }

        /// <summary> 试卷图片Id列表 </summary>
        public static string[] Pictures()
        {
            return PicturesFile.ReadConfig();
        }
    }
}
