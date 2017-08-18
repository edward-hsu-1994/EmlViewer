using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmlViewer {
    static class Program {
        public static List<MainForm> forms = new List<MainForm>();
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm.OpenForms(
                args?.Where(x => MainForm.IsEml(x))
                .Select(x => x.Replace(@":\", @":\\"))
                .ToArray(), true);
            Application.Run();
        }
    }
}
