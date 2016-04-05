

using System;
using System.Collections.Generic;
using System.Linq;

namespace MathRace.Util
{

    public enum TokenType
    {
        None,
        Number,
        Constant,
        Operator,
        Function,
        LeftParenthesis,
        RightParenthesis
    }

    public enum MathConstants
    {
        None,
        E,
        PI,
    }

    public enum Operators
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        Quotient,
        Exponent,
        UnaryMinus,
        Factorial,
    }

    public enum Functions
    {
        sin,
        cos,
        tan,
        arcsin,
        arccos,
        arctan
    }

    public struct RPNToken
    {
        public string TokenValue;
        public double NumberValue;
        public TokenType TokenValueType;
        public MathConstants ConstantValueType;
        public Operators OperatorType;
        public Functions FunctionType;
    }

    public class MathExp
    {
        public List<RPNToken> exp;
        
        public MathExp(List<RPNToken> RPNexpression)
        {
            exp = RPNexpression;
        }

        public RPNToken assign(string str)
        {

            RPNToken rn = new RPNToken();

            rn.TokenValue = str;

            if (str.All(x => x.isDigit() || x == '.')){
                if(double.TryParse(str, out rn.NumberValue))
                {
                    return rn;
                }
                else
                {
                    throw new Exception("Cannot parse number for RPN: in MathExp");
                }
            }
            else if(Enum.TryParse(str, out rn.ConstantValueType))
            {
                return rn;
            }
            else if (str.Single().isMathOperator())
            {

                rn.TokenValueType = TokenType.Operator;

                switch (str.Single())
                {
                    case '+':
                        rn.OperatorType = Operators.Plus;
                        return rn;
                    case '-':
                        rn.OperatorType = Operators.Minus;
                        return rn;
                    case '*':
                        rn.OperatorType = Operators.Multiply;
                        return rn;
                    case '/':
                        rn.OperatorType = Operators.Divide;
                        return rn;
                    case '%':
                        rn.OperatorType = Operators.Quotient;
                        return rn;
                    case '^':
                        rn.OperatorType = Operators.Exponent;
                        return rn;
                    case '!':
                        rn.OperatorType = Operators.Factorial;
                        return rn;
                    default:
                        throw new Exception("Can't parse operator");
                }
            }
            else if(Enum.TryParse(str, out rn.FunctionType))
            {
                return rn;
            }
            else
            {
                throw new Exception("Can't parse.");
            }
            
        }

    }

}