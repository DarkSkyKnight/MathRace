

using System;
using System.Collections.Generic;
using System.Linq;

namespace MathRace.Util
{

    public class MathExp
    {

        private List<Unit> units;

        public int solution;

        public MathExp(List<Unit> exp, int answer)
        {

            solution = answer;
            units = exp;

        }

        public MathExp(string exp, bool isRPN = false) : this(parseToList(exp, isRPN), solve(parseToList(exp, isRPN)))
        {
        }

        private static int solve(List<Unit> x)
        {
            // TODO
            return 0;
        }

        /// <summary>
        /// Parses a mathematical expression to a List of units. Throws exceptions in different cases.
        /// </summary>

        public static List<Unit> parseToList(String exp, bool isRPN)
        {

            char[] temp = exp.ToCharArray();

            Unit current = new Unit(0);

            List<Unit> z = new List<Unit>();

            List<char> x = new List<char>();


            for (int i = 0; i < temp.Length; i++)
            {

                if (Unit.isMathematicalSymbolInclPara(temp[i]))
                {

                    if (Char.IsNumber(temp[i - 1]))
                    {

                        current = new Unit(Int32.Parse(string.Concat(x)));

                        z.Add(current);

                        x.Clear();

                    }

                    current = new Unit(temp[i]);

                    z.Add(current);

                }
                else if (Char.IsNumber(temp[i]))
                {

                    x.Add(temp[i]);

                }
                else {

                    throw new Exception("MathRace.Util.MathExp.parseToList: " + "something is neither a number nor a mathematical symbol: " + temp[i]);

                }

                if ((i == temp.Length - 1) && (!isRPN))
                {

                    current = new Unit(Int32.Parse(string.Concat(x)));
                    z.Add(current);
                    x.Clear();

                }

            }

            if (!isRPN)
            {

                z = convertToRPN(z);

            }

            return z;

        }

        public static List<Unit> convertToRPN(List<Unit> exp)
        {

            List<Unit> outqueue = new List<Unit>();

            List<Unit> stack = new List<Unit>();

            for (int i = 0; i < exp.Count; i++)
            {

                // If the token is a number, then add it to the output queue

                if (exp[i].isNumber)
                {

                    outqueue.Add(exp[i]);

                }

                // If the token is an operator

                else if (!exp[i].isNumber)
                {

                    if (stack.Count() != 0)
                    {

                        int m = stack.Count() - 1;

                        // Checks precdence

                        while (m >= 0 && exp[i].prec <= stack[m].prec)
                        {

                            outqueue.Add(stack[m]);

                            stack.RemoveAt(stack.Count - 1);

                            m = stack.Count() - 1;

                        }

                    }

                    stack.Add(exp[i]);

                }

                else if (!exp[i].isNumber && Unit.isLeftPara(exp[i].x.GetValueOrDefault()))
                {

                    stack.Add(exp[i]);

                }

            }


            return null;
        }

    }

    public class Unit
    {


        static int PR_LEFTPAR = 3;
        static int PR_RIGHTPAR = 3;
        static int PR_PLUS = 4;
        static int PR_MINUS = 4;
        static int PR_MULT = 5;
        static int PR_DIV = 5;
        static int PR_MOD = 5;
        static int PR_POWER = 6;

        public bool isNumber { get; set; }

        public Unit(char a)
        {
            x = a;
            setPrec(a);
        }
        public Unit(int a)
        {
            y = a;
        }


        public char? x
        {
            get { return x; }
            set
            {

                if (isMathematicalSymbol(value.GetValueOrDefault()).Item1)
                {
                    x = value;
                }
                else {

                }

                isNumber = false;
                y = null;


            }
        }

        public int? y
        {
            get { return y; }
            set
            {
                isNumber = true;
                x = null;
            }
        }

        public int prec
        {

            get;
            set;

        }

        private void setPrec(char x)
        {

            switch (x)
            {
                case '+':
                    prec = PR_PLUS;
                    break;
                case '-':
                    prec = PR_MINUS;
                    break;
                case '*':
                    prec = PR_MULT;
                    break;
                case '/':
                    prec = PR_DIV;
                    break;
                case '%':
                    prec = PR_MOD;
                    break;
                case '^':
                    prec = PR_POWER;
                    break;
                case '(':
                    prec = PR_LEFTPAR;
                    break;
                case ')':
                    prec = PR_RIGHTPAR;
                    break;
                case '[':
                    prec = PR_LEFTPAR;
                    break;
                case ']':
                    prec = PR_RIGHTPAR;
                    break;
                case '{':
                    prec = PR_LEFTPAR;
                    break;
                case '}':
                    prec = PR_RIGHTPAR;
                    break;
                default:
                    break;
            }

        }


        public static Tuple<bool, int> isMathematicalSymbol(char x)
        {

            switch (x)
            {

                case '+':
                    return new Tuple<bool, int>(true, PR_PLUS);
                case '-':
                    return new Tuple<bool, int>(true, PR_MINUS);
                case '*':
                    return new Tuple<bool, int>(true, PR_MULT);
                case '/':
                    return new Tuple<bool, int>(true, PR_DIV);
                case '%':
                    return new Tuple<bool, int>(true, PR_MOD);
                case '^':
                    return new Tuple<bool, int>(true, PR_POWER);
                default:
                    return new Tuple<bool, int>(false, 0);
            }
        }

        public static bool isMathematicalSymbolInclPara(char x)
        {

            if (isMathematicalSymbol(x).Item1)
            {
                return true;
            }
            else {
                return isPara(x);
            }

        }

        public static bool isPara(char x)
        {

            return (isLeftPara(x) || isRightPara(x));

        }

        public static bool isLeftPara(char x)
        {

            switch (x)
            {


                case '(':
                    return true;
                case '[':
                    return true;
                case '{':
                    return true;
                default:
                    return false;

            }

        }

        public static bool isRightPara(char x)
        {

            switch (x)
            {


                case ')':
                    return true;
                case ']':
                    return true;
                case '}':
                    return true;
                default:
                    return false;

            }

        }

    }

}