
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Course_design_of_compiling_principles
{
    public partial class MainWindow : Window
    {
        List<grammar> grammarList = new List<grammar>();
        LR0 lr0;
        public static string inputstr;
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            lr0 = new LR0();
            lr0.Init();
            
            InitializeComponent();
            showTable();
            showGrammar();
        }
        private void startbtn_Click(object sender, RoutedEventArgs e)
        {

            lr0.inStr = inputArea.Text;
            showArea.ItemsSource = null;
            lr0.Analysis();
            showArea.AutoGenerateColumns = false;
            showArea.ItemsSource = lr0.dataList;
        }
        
        private void showTable()
        {
            AnalysisTable.Text = "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ACTION" + '\t' + '\t' + '\t' + '\t' + '\t' + '\t' + '\t' + "GOTO");
            sb.Append("状态" + '\t');
            for (int i = 0; i < lr0.vt.Count; i++)
                sb.Append(lr0.vt[i] + '\t');
            for (int i = 0; i < lr0.vn.Count; i++)
                sb.Append(lr0.vn[i] + '\t');
            sb.Append('\n');
            for (int i = 0; i < lr0.C.Count; i++)
            {
                sb.Append(i + "" + '\t');
                for (int j = 0; j < lr0.vt.Count; j++)
                    sb.Append(lr0.action[i, j] + '\t');
                for (int j = 0; j < lr0.vn.Count; j++)
                    sb.Append(lr0.GOTO[i, j] + '\t');
                sb.Append('\n');
            }
            string str = sb.ToString();
            AnalysisTable.Text = str;
        }

        private void showGrammar()
        {
            grammarInput.AutoGenerateColumns = false;
            for (int i = 1; i < lr0.produceFormula.Count; i++)
            {
                grammarList.Add(new grammar() { GrammarStr = lr0.produceFormula[i] });
            }
            grammarInput.ItemsSource = grammarList;
        }
    }

    public class grammar
    {
        public string GrammarStr { get; set; }
    }
}
