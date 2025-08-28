/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
语法分析器，表达式token流->表达式byteCode字节流
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpByteCodeVM;
using static ExpParseV.ExpLexer;

namespace ExpParseV
{
    public class ExpParser
    {
        public const string TAG = "ExpParser";
        
        public enum OpType
        {
            OP_NONE = -1,
            //二元运算符
            OP_ADD,
            OP_SUB,
            OP_MUL,
            OP_DIV,
            OP_MOD,
            OP_BAND,
            OP_BOR,
            OP_BXOR,
            OP_BSHL,
            OP_BSHR,
            OP_POW,
            OP_LT,
            OP_GT,
            OP_NEQ,
            OP_EQ,
            OP_LE,
            OP_GE,
            OP_AND,
            OP_OR,
            //一元运算符
            OP_NEG,             //负数
            OP_BNOT,
            OP_NOT,
            //数值
            OP_TVAL,
            //变量
            OP_VAR,
            //函数
            OP_FUNCCALL,
        }

        struct BinaryOpPriority
        {
            public byte left;
            public byte right;
            public BinaryOpPriority(byte left, byte right)
            {
                this.left = left;
                this.right = right;
            }
        }

        /// <summary>
        /// 一元运算符优先级
        /// </summary>
        const byte UNARY_OP_PRIORITY = 12;

        /// <summary>
        /// 二元运算符优先级
        /// </summary>
        static readonly BinaryOpPriority[] BINARY_OP_PRIORITYS =
        {
            new BinaryOpPriority(10, 10), new BinaryOpPriority(10, 10), //+ -
            new BinaryOpPriority(11, 11), new BinaryOpPriority(11, 11), new BinaryOpPriority(11, 11), //* / %
            new BinaryOpPriority(6, 6), new BinaryOpPriority(4, 4), new BinaryOpPriority(5, 5), //& | ~
            new BinaryOpPriority(7, 7), new BinaryOpPriority(7, 7), //<< >>
            new BinaryOpPriority(14, 13), //^
            new BinaryOpPriority(3, 3),new BinaryOpPriority(3, 3), //< >
            new BinaryOpPriority(3, 3),new BinaryOpPriority(3, 3), //!= ==
            new BinaryOpPriority(3, 3),new BinaryOpPriority(3, 3), //<= >=
            new BinaryOpPriority(2, 2),new BinaryOpPriority(1, 1), //&& ||
        };

        static TValue sTValue = new TValue();
        static List<byte> sByteList = new List<byte>();
        static List<byte[]> sByteArrayList = new List<byte[]>();
        public static ExpLogger sLogger = new ExpLogger(TAG);

        ExpLexer _expLexer = null;
        List<byte> _expByteCode = null;
        
        public ExpLexer expLexer
        {
            get
            {
                return _expLexer;
            }
        }

        public ExpParser()
        {
            _expLexer = new ExpLexer();
            _expByteCode = new List<byte>();
        }
        
        public void ParseException(string msg, params object[] args)
        {
            string tokenStr = _expLexer.tokenStr;
            if (!string.IsNullOrEmpty(tokenStr))
            {
                msg = string.Format("{0} {1}", msg, tokenStr);
            }
            sLogger.LogExceptionFormat(msg, args);
        }

        /// <summary>
        /// 获取一元运算符op
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        OpType _GetUnaryOp(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.TK_OP_SUB:
                    return OpType.OP_NEG;
                case TokenType.TK_OP_BNOT:
                    return OpType.OP_BNOT;
                case TokenType.TK_OP_NOT:
                    return OpType.OP_NOT;
            }
            return OpType.OP_NONE;
        }
        
        /// <summary>
        /// 获取二元运算符op
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        OpType _GetBinaryOp(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.TK_OP_ADD:
                    return OpType.OP_ADD;
                case TokenType.TK_OP_SUB:
                    return OpType.OP_SUB;
                case TokenType.TK_OP_MUL:
                    return OpType.OP_MUL;
                case TokenType.TK_OP_DIV:
                    return OpType.OP_DIV;
                case TokenType.TK_OP_MOD:
                    return OpType.OP_MOD;
                case TokenType.TK_OP_BAND:
                    return OpType.OP_BAND;
                case TokenType.TK_OP_BOR:
                    return OpType.OP_BOR;
                case TokenType.TK_OP_BNOT:
                    return OpType.OP_BXOR;
                case TokenType.TK_OP_POW:
                    return OpType.OP_POW;
                case TokenType.TK_OP_LT:
                    return OpType.OP_LT;
                case TokenType.TK_OP_GT:
                    return OpType.OP_GT;
                case TokenType.TK_OP_AND:
                    return OpType.OP_AND;
                case TokenType.TK_OP_OR:
                    return OpType.OP_OR;
                case TokenType.TK_OP_NEQ:
                    return OpType.OP_NEQ;
                case TokenType.TK_OP_EQ:
                    return OpType.OP_EQ;
                case TokenType.TK_OP_BSHL:
                    return OpType.OP_BSHL;
                case TokenType.TK_OP_LE:
                    return OpType.OP_LE;
                case TokenType.TK_OP_BSHR:
                    return OpType.OP_BSHR;
                case TokenType.TK_OP_GE:
                    return OpType.OP_GE;
            }
            return OpType.OP_NONE;
        }

        public static void ByteCodeAddHeader(List<byte> byteCode)
        {

        }

        public static void ByteCodeAddOpType(List<byte> byteCode, OpType opType)
        {
            byteCode.Add((byte)opType);
        }

        public static void ByteCodeAddBool(List<byte> byteCode, bool value)
        {
#if EXPV_DEBUG
            sLogger.LogFormat("push bool:{0}", value);
#endif
            ByteCodeAddOpType(byteCode, OpType.OP_TVAL);
            sTValue.SetBool(value);
            byteCode.AddRange(sTValue.ToByteArray());
        }

        public static void ByteCodeAddInt(List<byte> byteCode, int value)
        {
#if EXPV_DEBUG
            sLogger.LogFormat("push int:{0}", value);
#endif
            ByteCodeAddOpType(byteCode, OpType.OP_TVAL);
            sTValue.SetInt(value);
            byteCode.AddRange(sTValue.ToByteArray());
        }

        public static void ByteCodeAddFlt(List<byte> byteCode, float value)
        {
#if EXPV_DEBUG
            sLogger.LogFormat("push float:{0}", value);
#endif
            ByteCodeAddOpType(byteCode, OpType.OP_TVAL);
            sTValue.SetFloat(value);
            byteCode.AddRange(sTValue.ToByteArray());
        }

        public static void ByteCodeAddString(List<byte> byteCode, string value, StrType strType)
        {
#if EXPV_DEBUG
            sLogger.LogFormat("push str:{0} strType:{1}", value, strType);
#endif
            ByteCodeAddOpType(byteCode, OpType.OP_TVAL);
            sTValue.SetString(value);
            byteCode.AddRange(sTValue.ToByteArray(strType));
        }

        public static byte[] BoolToByteCode(bool value)
        {
            sByteList.Clear();
            ByteCodeAddBool(sByteList, value);
            return sByteList.ToArray();
        }

        public static byte[] IntToByteCode(int value)
        {
            sByteList.Clear();
            ByteCodeAddInt(sByteList, value);
            return sByteList.ToArray();
        }

        public static byte[] FltToByteCode(float value)
        {
            sByteList.Clear();
            ByteCodeAddFlt(sByteList, value);
            return sByteList.ToArray();
        }

        public static byte[] StringToByteCode(string value, StrType strType = StrType.ST_NONE)
        {
            sByteList.Clear();
            ByteCodeAddString(sByteList, value, strType);
            return sByteList.ToArray();
        }

        void _ResetByteCode()
        {
            _expByteCode.Clear();
        }

        /// <summary>
        /// 解析表达式源码->表达式byteCode字节流
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public byte[] ParseExp(string sourceCode)
        {
            _ResetByteCode();
            _expLexer.SetSourceCode(sourceCode);
            ByteCodeAddHeader(_expByteCode);
            _expLexer.NextToken();
            _ParseExp();
#if EXPV_DEBUG
            sLogger.LogFormat("the end char:{0}", _expLexer.tokenChar);
#endif
            //_expLexer.CheckEndToken();
            sLogger.LogFormat("the end char:{0}", _expLexer.tokenChar);
            return _expByteCode.ToArray();
        }

        /// <summary>
        /// 解析表达式源码(支持多个表达式之间用逗号,分隔)->一个或多个表达式byteCode字节流
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public byte[][] ParseExps(string sourceCode)
        {
            sByteArrayList.Clear();
            _expLexer.SetSourceCode(sourceCode);
            
            while (true)
            {
                _ResetByteCode();
                ByteCodeAddHeader(_expByteCode);
                _expLexer.NextToken();
                if (_expLexer.tokenChar == NULL_CHAR)
                {
                    break;
                }
                _ParseExp();
#if EXPV_DEBUG
                sLogger.LogFormat("the end char:{0}", _expLexer.tokenChar);
#endif
                sByteArrayList.Add(_expByteCode.ToArray());
                
                if (_expLexer.tokenChar != ',')
                {
                    break;
                }
            }
            
            return sByteArrayList.ToArray();
        }

        void _ParseExp()
        {
            _ParseSubExp(0);
        }
        
        OpType _ParseSubExp(byte priorityLimit)
        {
            OpType unaryOp = _GetUnaryOp(_expLexer.tokenType);
            if(unaryOp != OpType.OP_NONE)
            {
                char unaryOpr = _expLexer.tokenChar;
                _expLexer.NextToken();
                _ParseSubExp(UNARY_OP_PRIORITY);
#if EXPV_DEBUG
                sLogger.LogFormat("push unaryOp:{0}", unaryOpr);
#endif
                ByteCodeAddOpType(_expByteCode, unaryOp);
            }
            else
            {
                _ParseSimpleExp();
            }

            OpType binaryOp = _GetBinaryOp(_expLexer.tokenType);
            while(binaryOp != OpType.OP_NONE && BINARY_OP_PRIORITYS[(int)binaryOp].left > priorityLimit)
            {
                string binaryOpr = _expLexer.tokenStr;
                _expLexer.NextToken();
                OpType nextBinaryOp = _ParseSubExp(BINARY_OP_PRIORITYS[(int)binaryOp].right);
#if EXPV_DEBUG
                sLogger.LogFormat("push binaryOp:{0}", binaryOpr);
#endif
                ByteCodeAddOpType(_expByteCode, binaryOp);
                binaryOp = nextBinaryOp;
            }

            return binaryOp;
        }

        void _ParseSimpleExp()
        {
            switch(_expLexer.tokenType)
            {
                case TokenType.TK_RSRVD_TRUE:
                case TokenType.TK_RSRVD_FALSE:
                    {
                        ByteCodeAddBool(_expByteCode, _ParseBool());
                        break;
                    }
                case TokenType.TK_INT:
                    {
                        ByteCodeAddInt(_expByteCode, _ParseInt());
                        break;
                    }
                case TokenType.TK_FLT:
                    {
                        ByteCodeAddFlt(_expByteCode, _ParseFlt());
                        break;
                    }
                case TokenType.TK_STR:
                    {
                        ByteCodeAddString(_expByteCode, _expLexer.tokenStr, StrType.ST_NONE);
                        break;
                    }
                case TokenType.TK_DELIM_OPEN_S_BRACE:
                    {
                        _expLexer.NextToken();
                        _ParseExp();
                        _expLexer.CheckTokenChar(')');
                        break;
                    }
                case TokenType.TK_IDENT:
                    {
                        string identityName = _expLexer.tokenStr;
                        _expLexer.NextToken();
                        if (_expLexer.CheckTokenChar('(', false))
                        {
                            //函数
                            _expLexer.NextToken();
                            int argCount = 0;
                            while(true)
                            {
                                if (_expLexer.CheckTokenChar(')', false) || _expLexer.tokenType == TokenType.TK_EOS)
                                {
                                    break;
                                }
                                if (_expLexer.CheckTokenChar(',', false) && argCount > 0)
                                {
                                    _expLexer.NextToken();
                                }
                                _ParseExp();
                                argCount++;
                            }
                            
                            ByteCodeAddString(_expByteCode, identityName, StrType.ST_FUNC_NAME); //函数名
                            ByteCodeAddInt(_expByteCode, argCount); //参数个数
                            ByteCodeAddOpType(_expByteCode, OpType.OP_FUNCCALL);
#if EXPV_DEBUG
                            sLogger.LogFormat("push func:{0} argCount:{1}", identityName, argCount);
#endif
                            _expLexer.CheckTokenChar(')');
                        }
                        else
                        {
                            _expLexer.RollbackCurLexState();
                            //变量
                            ByteCodeAddString(_expByteCode, identityName, StrType.ST_VAR_NAME);
                            ByteCodeAddOpType(_expByteCode, OpType.OP_VAR);
#if EXPV_DEBUG
                            sLogger.LogFormat("push var:{0}", identityName);
#endif
                        }
                        break;
                    }
                default:
                    _expLexer.LexException("unexpected symbol");
                    return;
            }

            _expLexer.NextToken();
        }
        
        bool _ParseBool()
        {
            bool ret;
            if (!bool.TryParse(_expLexer.tokenStr, out ret))
            {
                ParseException("cannot parse symbol");
            }
            return ret;
        }

        int _ParseInt()
        {
            int ret;
            if(!int.TryParse(_expLexer.tokenStr, out ret))
            {
                ParseException("cannot parse symbol");
            }
            return ret;
        }

        float _ParseFlt()
        {
            float ret;
            if (!float.TryParse(_expLexer.tokenStr, out ret))
            {
                ParseException("cannot parse symbol");
            }
            return ret;
        }

        double _ParseDouble()
        {
            double ret;
            if (!double.TryParse(_expLexer.tokenStr, out ret))
            {
                ParseException("cannot parse symbol");
            }
            return ret;
        }
    }
}