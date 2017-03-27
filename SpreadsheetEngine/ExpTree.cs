using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Jackson Peven, 11382715

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
                        return left - right; //this might be wrong
                    case '*':
                        return left * right;
                    case '/':
                        return left / right;
                        //etc
                }
                return 0;
            }
        }

        private class VarNode : Node
        {
            private string m_varName;

            //private Dictionary<string, double> m_lookup;

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
            //TODO: Parse the expression string

            // For next homework support:
            // No parens, single operator
            // Examples:
            // "A1 + 47 + 654 + Hello + 2"
            // "54 + 275 + 98 + 1000"
            // "6*7*8*9
            // "A2"

            //For HW6
            //2+3*4  + should be the root
            //(55-11)/11
            //2+3*4
            //2*3+4
            //2*3+4
            //(2*3) + 4
            //((2+3)*4)-(1+3)

            //HIGHER PRIORITY -> LOWER LOCATION IN THE TREE

            m_root = Compile(expression, m_lookup);

        }

        public List<string> GetVariables()
        {
            List<string> keys = new List<string>(m_lookup.Keys);
            return keys;
        }

        private static Node Compile(string exp, Dictionary<string, double> m_lookup)
        {
            // Find first operation
            // Build parent operator node
            // parent.left = BuildSimple(before op char)
            // parent.right = Compile(after op char)
            // return parent;
            exp = exp.Replace(" ", "");//This will remove the whitespace from the expression

            //check for being entirely enclosed in ()
            //lowestPriority(3+4+5) would return -1

            //(3+4)*(5+6) ---> 3+4)*(5+6 which is wrong
            //if first char is '(' and last character is matching ')', remove parens
            
            if(exp == "" || exp == null)
            {
                return new ConstNode(0);
            }

            if (exp[0] == '(')
            {
                int counter = 1;
                for (int i = 1; i < exp.Length; i++)
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
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (exp[i] == '(')
                    {
                        counter++;
                    }
                }
            }


            //get low op index
            //build op node for char at that index
            //need to strip the outer parenthesis before calling BuildSimple
            //we want to be able 

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

        private static Node BuildSimple(string exp, Dictionary<string, double> m_lookup)
        {
            double num;
            if (double.TryParse(exp, out num))
            {
                return new ConstNode(num);
            }
            if (!m_lookup.ContainsKey(exp))//if the expression is currently not in the dictionary
            {
                m_lookup[exp] = 0;
            }
            return new VarNode(exp);
        }

        public double Eval()
        {
            if (m_root != null) { return m_root.Eval(m_lookup); }
            else
                return double.NaN;
        }

        private static int GetLowOpIndex(string exp)
        {
            // 3+4*5+6  --> 5
            // 3*4+5*6  --> 3
            // 3-4*5-6  --> 5

            int parenCounter = 0;                   //if this isn't 0, we don't care what it is
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

