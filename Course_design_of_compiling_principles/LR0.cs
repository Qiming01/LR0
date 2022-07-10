using System.Collections;
using System.Collections.Generic;

namespace Course_design_of_compiling_principles
{
    internal class LR0
    {
        public List<string> produceFormula = new List<string> { "", "E->aA", "E->bB", "A->cA", "A->d", "B->cB", "B->d" };
        public List<string> vt = new List<string>(); //终结符集
        public List<string> vn = new List<string>(); //非终结符集
        private List<string> symbol = new List<string>();
        public List<List<string>> C = new List<List<string>>();
        public string[,]? action; //action表
        public string[,]? GOTO; //goto表
        private Hashtable projectLink = new Hashtable();
        private Hashtable processedProduction = new Hashtable();
        public List<Data> dataList = new List<Data>();
        public string inStr = "bcd";
        private void ProduceFormulaHandle()
        {
            foreach (string s in vn)
                processedProduction.Add(s, new List<string>());
            foreach (string productionStr in produceFormula)
            {
                string[] splitedStr = productionStr.Split("->");
                if (splitedStr.Length > 1)
                {
                    List<string>? valueList = processedProduction[splitedStr[0]] as List<string>;
                    string[] splitedRight = splitedStr[1].Split('|');
                    foreach (string s in splitedRight)
                    {
                        if (valueList != null) valueList.Add(s);
                    }
                }
            }
        }

        //构造非终结符集
        private void Getvn()
        {
            foreach (string str in produceFormula)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    if (c >= 'A' && c <= 'Z')
                    {
                        if (i < str.Length - 1 && str[i + 1] == '\'')
                        {
                            if (!vn.Contains(c + "'")) vn.Add(c + "'");
                        }
                        else
                        {
                            if (!vn.Contains(c + "")) vn.Add(c + "");
                        }
                    }
                }
            }
        }

        private void Getvt()
        {
            foreach (string str in vn)
            {
                List<string> rightList = (List<string>)processedProduction[str];
                foreach (string s in rightList)
                {
                    foreach (char c in s)
                    {
                        if (c < 'A' || c > 'Z')
                        {
                            if (!vt.Contains(c + "")) vt.Add(c + "");
                        }
                    }
                }
            }
        }

        public void Init()
        {
            Getvn();
            ProduceFormulaHandle();
            Getvt();
            foreach (string str in vn) symbol.Add(str);
            foreach (string str in vt) symbol.Add(str);
            CreateProjectSet();
            GetLink();
            CreateAnalysisTable();
        }

        /**
         * Closure 求闭包CLOUSURE(I)：
         * I的任何项目都属于CLOUSURE(I)
         * 若 A->α·Bβ 属于CLOUSURE(I),那么对任何关于B的产生式 B->γ ，项目 B->·γ 也属于CLOUSURE(I)
         */
        private List<string>? Closure(List<string> I)
        {
            if (I == null || I.Count == 0) return null;
            List<string> result = new List<string>(I);
            foreach (string productionStr in I)//遍历I中的所有产生式
            {
                for (int i = 0; i < productionStr.Length; i++)
                {
                    if (productionStr[i] == '·' && i != productionStr.Length - 1 && vn.Contains(productionStr[i + 1] + ""))//找到 A->α·Bβ 属于CLOUSURE(I)
                    {
                        List<string>? productionRightList = processedProduction[productionStr[i + 1] + ""] as List<string>;
                        if (productionRightList == null) continue;
                        foreach (string rightStr in productionRightList)
                            result.Add(productionStr[i + 1] + "->·" + rightStr);
                    }
                }
            }
            return result;
        }

        /**
         * GO(I, X) = CLOSURE(J)
         * 其中 J={任何形如 A->αX·β 的项目 | A->α·Xβ 属于I}
         */
        private List<string>? GO(List<string> I, string X)
        {
            List<string> J = new List<string>();
            foreach (string projectStr in I)
            {
                int pointIndex = projectStr.IndexOf('·');
                if (pointIndex < projectStr.Length - 1 && (projectStr[pointIndex + 1] + "").Equals(X))  //找到形如 A->α·Xβ 的项目
                {
                    J.Add(projectStr.Substring(0, pointIndex) + X + '·' + projectStr.Substring(pointIndex + 2));
                }
            }
            J = Closure(J);
            return J;
        }

        private bool isExistedInC(List<List<string>> C, List<string> tmp)
        {
            foreach (List<string> List in C)
            {
                if (List[0].Equals(tmp[0])) return true;
            }
            return false;
        }

        //构造项目集规范族
        private void CreateProjectSet()
        {
            //拓展文法开始符号为S'
            produceFormula[0] = "S'->" + vn[0];
            //vn.Add("S'");

            C.Add(new List<string>());  //初始状态加入
            C[0].Add("S'->·" + vn[0]);
            C[0] = Closure(C[0]);
            while (true)
            {
                int oldCLength = C.Count;
                for (int i = 0; i < C.Count; i++)   //遍历C中的每一个项目集I
                {
                    List<string> I = C[i];
                    foreach (string X in symbol)
                    {
                        List<string> tmp = GO(I, X);
                        if (tmp != null && !isExistedInC(C, tmp))
                        {
                            C.Add(tmp);
                            //projectLink.Add(i + "," + X, C.Count-1);
                        }
                    }
                }
                if (C.Count == oldCLength) break;
            }
        }
        private int FindLinkNum(string targetFindStr)
        {
            for (int i = 0; i < C.Count; i++)
            {
                List<string> I = C[i];
                foreach (string str in I)
                {
                    if (str.Equals(targetFindStr)) return i;
                }
            }
            return -1;
        }
        private void GetLink()
        {
            for (int i = 0; i < C.Count; i++)
            {
                List<string> I = C[i];
                foreach (string str in I)
                {
                    int pointIndex = str.IndexOf('·');
                    if (pointIndex < str.Length - 1)  //找到形如 A->α·Xβ 的项目
                    {
                        string X = "" + str[pointIndex + 1];
                        string targetFindStr = str.Substring(0, pointIndex) + X + '·' + str.Substring(pointIndex + 2); //A->αX·β
                        int index = FindLinkNum(targetFindStr);
                        if (index >= 0)
                        {
                            if (!projectLink.ContainsKey(i + "," + X))
                                projectLink.Add(i + "," + X, index);
                        }
                    }
                }
            }
        }
        private void CreateAnalysisTable()
        {
            vt.Add("#"); // 加入#
            action = new string[C.Count, vt.Count];
            GOTO = new string[C.Count, vn.Count];
            for (int i = 0; i < C.Count; i++)
            {
                for (int j = 0; j < vt.Count; j++)
                {
                    string vtStr = vt[j];
                    if (projectLink.ContainsKey(i + "," + vtStr))
                    {
                        action[i, j] = "s" + projectLink[i + "," + vtStr];
                    }
                }
                for (int j = 0; j < vn.Count; j++)
                {
                    string vnStr = vn[j];
                    if (projectLink.ContainsKey(i + "," + vnStr))
                    {
                        GOTO[i, j] = "" + projectLink[i + "," + vnStr];
                    }
                }
                if (C[i].Count == 1 && C[i][0].IndexOf('·') == C[i][0].Length - 1)
                {
                    if (C[i][0].Equals(produceFormula[0] = "S'->" + vn[0] + "·"))
                    {
                        action[i, vt.Count - 1] = "acc";
                    }
                    else
                    {
                        int index = 0;
                        for (index = 0; index < produceFormula.Count; index++)
                        {
                            if (produceFormula[index].Equals(C[i][0].Replace("·", ""))) break;
                        }
                        for (int j = 0; j < vt.Count; j++)
                        {
                            action[i, j] = "r" + index;
                        }
                    }
                }
            }
        }

        public void Analysis()
        {
            dataList.Clear();
            int step = 0;
            Stack<int> stateStack = new Stack<int>(); //状态栈
            Stack<string> symbolStack = new Stack<string>(); //符号栈
            stateStack.Push(0);//将0压入状态栈
            symbolStack.Push("#");//将符号#压入符号栈
            //string inputStr = "bcd";
            string inputStr = inStr;
            inputStr = inputStr + "#";//输入串
            string a; //输入符号a
            int aIndex = 0; //指向输入串的头
            int s = 0; //栈顶状态s


            dataList.Add(new Data() { step = "" + step, state = "0", symbol = "#", inStr = inputStr.Substring(aIndex) });
            while (true)
            {
                string stateStr = "";
                string symbolStr = "";
                step++;
                a = inputStr[aIndex] + "";
                s = stateStack.Peek();
                string todo = null;
                if (vt.Contains(a))
                {
                    todo = action[s, vt.IndexOf(a)];
                }
                if (todo == null)
                {
                    //error
                    dataList.Add(new Data() { step = "" + step, state = "ERROR", symbol = "ERROR", inStr = "ERROR" });
                    return;
                }
                if (todo[0] == 's')
                {
                    stateStack.Push(int.Parse(todo.Replace("s", "")));
                    symbolStack.Push(a);
                    aIndex++;
                }
                else if (todo[0] == 'r')
                {
                    int length = produceFormula[int.Parse(todo.Replace("r", ""))].Split("->")[1].Length;
                    for (int i = 0; i < length; i++)
                    {
                        stateStack.Pop();
                        symbolStack.Pop();
                    }
                    symbolStack.Push(produceFormula[int.Parse(todo.Replace("r", ""))].Split("->")[0]);
                    stateStack.Push(int.Parse(GOTO[stateStack.Peek(), vn.IndexOf(symbolStack.Peek())]));
                }
                else if (todo.Equals("acc"))
                {
                    //acc
                    dataList.Add(new Data() { step = "" + step, state = "acc", symbol = "", inStr = "" });
                    return;
                }
                foreach (int i in stateStack)
                {
                    stateStr = i + stateStr;
                }
                foreach (string str in symbolStack)
                {
                    symbolStr = str + symbolStr;
                }
                dataList.Add(new Data() { step = "" + step, state = stateStr, symbol = symbolStr, inStr = inputStr.Substring(aIndex) });
            }
        }
    }

    class Data
    {
        public string step { get; set; }
        public string state { get; set; }
        public string symbol { get; set; }
        public string inStr { get; set; }
    }

}