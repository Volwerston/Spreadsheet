using System.Collections.Generic;

namespace SpreadsheetEngine
{
    public class ExpTree
    {
        private abstract class Node
        {
            public abstract double Eval(Dictionary<string, double> m_lookup);
        }

        private class ConstNode : Node
        {
            private double m_value;

            public ConstNode(double value) { m_value = value; }

            public override double Eval(Dictionary<string, double> m_lookup)
            {
                return m_value;
            }
        }

        private class OpNode : Node
        {
            private char m_op;
            private Node m_left, m_right;

            public OpNode(char op, Node left, Node right)
            {
                m_op = op;
                m_left = left;
                m_right = right;
            }

            public override double Eval(Dictionary<string, double> m_lookup)
            {
                double left = m_left.Eval(m_lookup);
                double right = m_right.Eval(m_lookup);
                switch (m_op)
                {
                    case '+':
                        return left + right;
                    case '-':
                        return left - right; 
                    case '*':
                        return left * right;
                    case '/':
                        return left / right;
                }
                return 0;
            }
        }

        private class VarNode : Node
        {
            private string m_varName;

            public VarNode(string varName)
            {
                m_varName = varName;
            }

            public override double Eval(Dictionary<string, double> m_lookup)
            {
                double return_val = 0;
                try { return_val = m_lookup[m_varName]; }
                catch
                {
                    System.Console.WriteLine("Error, variable " + m_varName + " not found. Returning 0");
                }
                return return_val;
            }
        }

        private Node m_root;

        private Dictionary<string, double> m_lookup = new Dictionary<string, double>();

        public ExpTree(string expression)
        {
            m_root = Compile(expression, m_lookup);
        }

        public List<string> GetVariables()
        {
            List<string> keys = new List<string>(m_lookup.Keys);
            return keys;
        }

        private static Node Compile(string exp, Dictionary<string, double> m_lookup)
        {
            exp = exp.Replace(" ", "");

            if (string.IsNullOrEmpty(exp))
            {
                return new ConstNode(0);
            }

            if (exp[0] == '(')
            {
                var counter = 1;
                for (var i = 1; i < exp.Length; i++)
                {
                    if (exp[i] == ')')
                    {
                        counter--;
                        if (counter == 0)
                        {
                            if (i == exp.Length - 1)
                            {
                                return Compile(exp.Substring(1, (exp.Length - 2)), m_lookup);
                            }

                            break;
                        }
                    }
                    if (exp[i] == '(')
                    {
                        counter++;
                    }
                }
            }

            int index = GetLowOpIndex(exp);
            if (index != -1)
            {
                return new OpNode(
                    exp[index],
                    Compile(exp.Substring(0, index), m_lookup),
                    Compile(exp.Substring(index + 1), m_lookup));
            }

            return BuildSimple(exp, m_lookup);
        }

        public void SetVar(string varName, double varValue)
        {
            m_lookup[varName] = varValue;
        }

        private static Node BuildSimple(string exp, IDictionary<string, double> lookup)
        {
            if (double.TryParse(exp, out var num))
            {
                return new ConstNode(num);
            }
            if (!lookup.ContainsKey(exp))
            {
                lookup[exp] = 0;
            }
            return new VarNode(exp);
        }

        public double Eval()
        {
            return m_root?.Eval(m_lookup) ?? double.NaN;
        }

        private static int GetLowOpIndex(string exp)
        {
            // 3+4*5+6  --> 5
            // 3*4+5*6  --> 3
            // 3-4*5-6  --> 5

            int parenCounter = 0;                   
            int index = -1;
            for (int i = exp.Length - 1; i >= 0; i--)
            {
                switch (exp[i])
                {
                    case ')':
                        parenCounter--;
                        break;
                    case '(':
                        parenCounter++;
                        break;
                    case '+':
                    case '-':
                        if (parenCounter == 0)
                        {
                            return i;
                        }
                        break;

                    case '*':
                    case '/':
                        if (parenCounter == 0 && index == -1)
                        {
                            index = i;
                        }
                        break;
                }
            }
            return index;

        }
    }


}

