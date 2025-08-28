/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
byteCode虚拟机，运行表达式byteCode字节流
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExpParseV.ExpParser;
using static ExpParseV.ExpIdentifierProvider;

namespace ExpParseV
{
    public class ExpByteCodeVM
    {
        public const string TAG = "ExpByteCodeVM";
        public const int BYTECODE_HEADER_BYTE_SIZE = 6;
        
        static List<TValue> sFuncArgs = new List<TValue>();
        public static ExpLogger sLogger = new ExpLogger(TAG);
        
        ExpIdentifierProvider _baseIdentProvider = new ExpIdentifierProvider();
        ExpIdentifierProvider _identProvider = null;
        ExpStrIdxTable _strIdxTable = new ExpStrIdxTable();
        Stack <TValue> _tvStack = new Stack<TValue>();
        
        /// <summary>
        /// 获取基础标识符提供者
        /// </summary>
        public ExpIdentifierProvider baseIdentProvider
        {
            get
            {
                return _baseIdentProvider;
            }
        }

        /// <summary>
        /// 获取当前运行传递进来的标识符提供者
        /// </summary>
        public ExpIdentifierProvider identProvider
        {
            get
            {
                return _identProvider;
            }
        }

        /// <summary>
        /// 获取字符串索引表
        /// </summary>
        public ExpStrIdxTable strIdxTable
        {
            get
            {
                return _strIdxTable;
            }
        }

        public ExpByteCodeVM()
        {
            _baseIdentProvider.RegBase();
        }

        void _PushTValue(OpType opType, byte[] byteCode, int startIndex)
        {
            TValue tv = new TValue();
            if(tv.FromByteArray(byteCode, startIndex, this))
            {
                _tvStack.Push(tv);
            }
            else
            {
                throw new Exception(string.Format("parse bytecode to tvalue error opType={0}", opType));
            }
        }

        /// <summary>
        /// 运行表达式byteCode字节流
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider">标识符提供者</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public TValue RunByteCode(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            if(byteCode == null || byteCode.Length == 0)
            {
                return TValueExt.sDefaultTValue;
            }

            try
            {
                _identProvider = identProvider;

                OpType opType = OpType.OP_NONE;
                for (int i = 0; i < byteCode.Length; i++)
                {
                    opType = (OpType)byteCode[i];
                    switch (opType)
                    {
                        case OpType.OP_TVAL:
                            {
                                i += 1;
                                _PushTValue(opType, byteCode, i);
                                TValue tv = _tvStack.Peek();
                                switch (tv.type)
                                {
                                    case ValueType.VT_BOOL:
                                        i += 1;
                                        break;
                                    case ValueType.VT_INT:
                                    case ValueType.VT_FLOAT:
                                        i += 4;
                                        break;
                                    case ValueType.VT_DOUBLE:
                                        i += 8;
                                        break;
                                    case ValueType.VT_STR:
                                        i += TValue.STR_HEAD_DATA_BYTE_SIZE;
                                        i += tv.valStr.Length;
                                        break;
                                    default:
                                        throw new Exception(string.Format("unidentify tvalue type {0}", tv.type));
                                }
                                break;
                            }
                        case OpType.OP_NEG:
                            {
                                TValue tv = _tvStack.Pop();
                                TValueExt.TValueNeg(ref tv);
                                _tvStack.Push(tv);
                                break;
                            }
                        case OpType.OP_BNOT:
                            {
                                TValue tv = _tvStack.Pop();
                                TValueExt.TValueBnot(ref tv);
                                _tvStack.Push(tv);
                                break;
                            }
                        case OpType.OP_NOT:
                            {
                                TValue tv = _tvStack.Pop();
                                TValueExt.TValueNot(ref tv);
                                _tvStack.Push(tv);
                                break;
                            }
                        case OpType.OP_ADD:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueAdd(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_SUB:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueSub(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_MUL:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueMul(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_DIV:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueDiv(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_MOD:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueMod(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_BAND:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueBand(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_BOR:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueBor(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_BXOR:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueBxor(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_BSHL:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueBshl(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_BSHR:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueBshr(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_POW:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValuePow(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_LT:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueLessThan(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_GT:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueLessThan(ref tv2, ref tv1);
                                _tvStack.Push(tv2);
                                break;
                            }
                        case OpType.OP_NEQ:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueEqual(ref tv1, ref tv2);
                                tv1.SetBool(!tv1.ToBool());
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_EQ:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueEqual(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_LE:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueLessEqual(ref tv1, ref tv2);
                                _tvStack.Push(tv1);
                                break;
                            }
                        case OpType.OP_GE:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueLessEqual(ref tv2, ref tv1);
                                _tvStack.Push(tv2);
                                break;
                            }
                        case OpType.OP_AND:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueAnd(ref tv2, ref tv1);
                                _tvStack.Push(tv2);
                                break;
                            }
                        case OpType.OP_OR:
                            {
                                TValue tv2 = _tvStack.Pop();
                                TValue tv1 = _tvStack.Pop();
                                TValueExt.TValueOr(ref tv2, ref tv1);
                                _tvStack.Push(tv2);
                                break;
                            }
                        case OpType.OP_VAR:
                            {
                                _VarGet(userData);
                                break;
                            }
                        case OpType.OP_FUNCCALL:
                            {
                                _FuncCall(userData);
                                break;
                            }
                        default:
                            throw new Exception(string.Format("unidentify bytecode optype={0}", opType));

                    }
                }

                return _tvStack.Pop();
            }
            catch(Exception e)
            {
                sLogger.LogExceptionFormat("bytecode run error:{0}", e);
            }

            return TValueExt.sDefaultTValue;
        }
        
        void _VarGet(System.Object userData)
        {
            TValue varNameTv = _tvStack.Pop();
            string varName = varNameTv.valStr;
            int intVal = varNameTv.ToInt();
            int varNamePid = (intVal >> TValue.STR_PID_BIT_SIZE) & TValue.SHORT_BIT_MASK1;
            int varNameIdx = intVal & TValue.SHORT_BIT_MASK1;
            
            ByteCodeVarGetterDef getterDef = null;
            if (_baseIdentProvider.id == varNamePid)
            {
                _baseIdentProvider.TryGetVarGetter(varNameIdx, out getterDef);
            }
            else if(_identProvider != null && _identProvider.id == varNamePid)
            {
                _identProvider.TryGetVarGetter(varNameIdx, out getterDef);
            }
            
            if (getterDef == null)
            {
                throw new Exception(string.Format("unidentify var {0}", varName));
            }
            
            getterDef.getter(userData, ref varNameTv);
            _tvStack.Push(varNameTv);
#if EXPV_DEBUG
            sLogger.LogFormat("======varGet varName={0}", varName);
#endif
        }

        void _FuncCall(System.Object userData)
        {
            sFuncArgs.Clear();

            TValue argCountTv = _tvStack.Pop();
            TValue funcNameTv = _tvStack.Pop();
            int argCount = argCountTv.ToInt();
            string funcName = funcNameTv.valStr;
            int intVal = funcNameTv.ToInt();
            int funcNamePid = (intVal >> TValue.STR_PID_BIT_SIZE) & TValue.SHORT_BIT_MASK1;
            int funcNameIdx = intVal & TValue.SHORT_BIT_MASK1;

            ByteCodeFuncDef funcDef = null;
            if(_baseIdentProvider.id == funcNamePid)
            {
                _baseIdentProvider.TryGetFunc(funcNameIdx, out funcDef);
            }
            else if(_identProvider != null && _identProvider.id == funcNamePid)
            {
                _identProvider.TryGetFunc(funcNameIdx, out funcDef);
            }
            
            if (funcDef == null)
            {
                throw new Exception(string.Format("unidentify func {0}", funcName));
            }
            
            for (int k = 0; k < argCount; k++)
            {
                sFuncArgs.Add(_tvStack.Pop());
            }
            sFuncArgs.Reverse();

            int inArgCount = sFuncArgs.Count;
            int reqArgCount = funcDef.argCount;
            if (inArgCount != reqArgCount)
            {
                throw new Exception(string.Format("func {0} inArgCount:{1} not match reqArgCount:{2}", funcName, inArgCount, reqArgCount));
            }

            funcDef.func(userData, sFuncArgs, ref funcNameTv);
            _tvStack.Push(funcNameTv);
#if EXPV_DEBUG
            sLogger.LogFormat("======funcCall funcName={0} argCount={1}", funcName, argCount);
#endif
        }
    }
}