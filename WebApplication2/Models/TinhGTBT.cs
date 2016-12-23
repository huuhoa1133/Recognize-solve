using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class TinhGTBT
    {
        //Các thông số cần tính
        public int a;
        public int b;
        public int c;
        public int d;

        private int Priority(char c)
        {
            if (c == '+' || c == '-')
                return 1;
            if (c == '*' || c == '/')
                return 2;
            if (c == '^')
                return 3;
            return 0;
        }

        private string Convert(string exp)
        {
            string result = "";
            Stack<char> stk = new Stack<char>();
            for (int i = 0; i < exp.Length; i++)
            {
                //Nếu là toán tử
                if (exp[i] == '+' || exp[i] == '-' || exp[i] == '/' || exp[i] == '*' || exp[i] == '^')
                {
                    if (stk.Count > 0)
                    {
                        if (Priority(stk.Peek()) >= Priority(exp[i]))
                        {
                            result += stk.Pop();
                        }
                    }
                    stk.Push(exp[i]);
                }
                //Nếu là toán hạng
                if ((exp[i] >= 'a' && exp[i] <= 'z'))
                {
                    result += exp[i];
                }
                //Nếu là số hạng
                if (exp[i] >= '0' && exp[i] <= '9')
                {
                    //tách số từ vị trí đang xét. Thêm dấu # để ngăn cách các số
                    int pos = i;
                    while (IsNumber(exp[i]))
                    {
                        i++;
                        if (i == exp.Length)
                            break;
                    }
                    //Thêm dấu # để ngăn cách các số hạng
                    result += exp.Substring(pos, i - pos) + '#';
                    i--;

                }
                if (exp[i] == '(')
                {
                    stk.Push('(');
                }

                if (exp[i] == ')')
                {
                    char tmp = stk.Pop();
                    while (tmp != '(')
                    {
                        result += tmp;
                        tmp = stk.Pop();
                    }
                }
            }
            foreach (char i in stk)
                result += i;
            return result;
        }

        private bool IsNumber(char c)
        {
            return (c >= '0' && c <= '9') || (c == '.');
        }

        public double Calculate(string exp)
        {
            string bt = Convert(exp);
            Stack<double> stk = new Stack<double>();
            Console.WriteLine(bt);
            double tmp1, tmp2;
            for (int i = 0; i < bt.Length; i++)
            {
                switch (bt[i])
                {
                    case 'a':
                        stk.Push(a);
                        break;
                    case 'b':
                        stk.Push(b);
                        break;
                    case 'c':
                        stk.Push(c);
                        break;
                    case 'd':
                        stk.Push(d);
                        break;
                    case '*':
                        tmp1 = stk.Pop();
                        tmp2 = stk.Pop();
                        stk.Push(tmp1 * tmp2);
                        break;
                    case '/':
                        tmp1 = stk.Pop();
                        tmp2 = stk.Pop();
                        stk.Push(tmp2 / tmp1);
                        break;
                    case '-':
                        tmp1 = stk.Pop();
                        tmp2 = stk.Pop();
                        stk.Push(tmp2 - tmp1);
                        break;
                    case '+':
                        tmp1 = stk.Pop();
                        tmp2 = stk.Pop();
                        stk.Push(tmp1 + tmp2);
                        break;
                    case '^':
                        tmp1 = stk.Pop();
                        tmp2 = stk.Pop();
                        stk.Push((int)Math.Pow(tmp2, tmp1));
                        break;
                    default:
                        if (IsNumber(bt[i]))
                        {
                            int pos = i;
                            while (IsNumber(bt[i]))
                                i++;
                            stk.Push(int.Parse(bt.Substring(pos, i - pos)));
                            i--;
                        }
                        break;
                }
            }

            return stk.Pop();
        }
    }

}