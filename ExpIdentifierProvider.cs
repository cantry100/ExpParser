/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
标识符（变量、函数）提供者，注册/获取标识符处理函数
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpByteCodeVM;
using static ExpParseV.ExpLogger;

namespace ExpParseV
{
    public class ExpIdentifierProvider
    {
        public const string TAG = "ExpIdentifierProvider";
        
        public delegate void ByteCodeVarGetter(System.Object userData, ref TValue returnValue);
        public delegate void ByteCodeFunc(System.Object userData, List < TValue> args, ref TValue returnValue);

        /// <summary>
        /// 变量定义
        /// </summary>
        public class ByteCodeVarGetterDef
        {
            public string name;
            public ByteCodeVarGetter getter;
            
            public ByteCodeVarGetterDef(string name, ByteCodeVarGetter getter)
            {
                this.name = name;
                this.getter = getter;
            }
        }

        /// <summary>
        /// 函数定义
        /// </summary>
        public class ByteCodeFuncDef
        {
            public string name;
            public int argCount;
            public ByteCodeFunc func;
            
            public ByteCodeFuncDef(string name, int argCount, ByteCodeFunc func)
            {
                this.name = name;
                this.argCount = argCount;
                this.func = func;
            }
        }

        public const int ID_MAX = TValue.STR_PID_MAX;
        public const int VAR_GETTER_IDX_MAX = TValue.STR_IDX_MAX;
        public const int FUNC_IDX_MAX = TValue.STR_IDX_MAX;
        
        public static ExpLogger sLogger = new ExpLogger(TAG);
        static int sId = TValue.STR_PID_NONE;

        static int NextId
        {
            get
            {
                if(sId == ID_MAX)
                {
                    throw new Exception(string.Format("NextId Fail! sId==ID_MAX={0}", ID_MAX));
                }
                sId++;
                return sId;
            }
        }

        int _id = TValue.STR_PID_NONE;
        Dictionary<string, int> _regVarGetterIdxDict = new Dictionary<string, int>();
        List<ByteCodeVarGetterDef> _regVarGetters = new List<ByteCodeVarGetterDef>();
        Dictionary<string, int> _regFuncIdxDict = new Dictionary<string, int>();
        List<ByteCodeFuncDef> _regFuncs = new List<ByteCodeFuncDef>();
        
        public int id
        {
            get
            {
                return _id;
            }
        }

        public ExpIdentifierProvider()
        {
            _id = NextId;
        }
        
        /// <summary>
        /// 注册变量
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="getter"></param>
        public int RegVarGetter(string varName, ByteCodeVarGetter getter)
        {
            int index = -1;
            if (_regVarGetterIdxDict.TryGetValue(varName, out index))
            {
                if (getter == null)
                {
                    _regVarGetters[index] = null;
                }
                else if(_regVarGetters[index] == null)
                {
                    _regVarGetters[index] = new ByteCodeVarGetterDef(varName, getter);
                }
                else
                {
                    sLogger.LogWarnFormat("RegVarGetter {0} Fail! getter already exist", varName);
                }
            }
            else if (getter != null)
            {
                index = _regVarGetters.Count;
                if (index < VAR_GETTER_IDX_MAX)
                {
                    _regVarGetterIdxDict.Add(varName, index);
                    _regVarGetters.Add(new ByteCodeVarGetterDef(varName, getter));
                }
                else
                {
                    throw new Exception(string.Format("RegVarGetter {0} Fail! index={1} >= indexMax={2}", varName, index, VAR_GETTER_IDX_MAX));
                }
            }

            return index;
        }
        
        /// <summary>
        /// 注册函数
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="func"></param>
        public int RegFunc(string funcName, int argCount, ByteCodeFunc func)
        {
            int index = -1;
            if (_regFuncIdxDict.TryGetValue(funcName, out index))
            {
                if (func == null)
                {
                    _regFuncs[index] = null;
                }
                else if(_regFuncs[index] == null)
                {
                    _regFuncs[index] = new ByteCodeFuncDef(funcName, argCount, func);
                }
                else
                {
                    sLogger.LogWarnFormat("RegFunc {0} Fail! func already exist", funcName);
                }
            }
            else if (func != null)
            {
                index = _regFuncs.Count;
                if (index < FUNC_IDX_MAX)
                {
                    _regFuncIdxDict.Add(funcName, index);
                    _regFuncs.Add(new ByteCodeFuncDef(funcName, argCount, func));
                }
                else
                {
                    throw new Exception(string.Format("RegFunc {0} Fail! index={1} >= indexMax={2}", funcName, index, FUNC_IDX_MAX));
                }
            }

            return index;
        }

        /// <summary>
        /// 根据变量名获取其索引
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetVarGetterIdx(string varName, out int index)
        {
            if (!_regVarGetterIdxDict.TryGetValue(varName, out index))
            {
                index = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据变量索引获取其定义
        /// </summary>
        /// <param name="index"></param>
        /// <param name="getterDef"></param>
        /// <returns></returns>
        public bool TryGetVarGetter(int index, out ByteCodeVarGetterDef getterDef)
        {
            if (index >= 0 && index < _regVarGetters.Count)
            {
                getterDef = _regVarGetters[index];
                return true;
            }
            getterDef = null;
            return false;
        }

        /// <summary>
        /// 根据变量名获取其定义
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="getterDef"></param>
        /// <returns></returns>
        public bool TryGetVarGetter(string varName, out ByteCodeVarGetterDef getterDef)
        {
            int index = 0;
            if (!_regVarGetterIdxDict.TryGetValue(varName, out index))
            {
                getterDef = null;
                return false;
            }
            getterDef = _regVarGetters[index];
            return true;
        }

        /// <summary>
        /// 根据函数名获取其索引
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetFuncIdx(string funcName, out int index)
        {
            if (!_regFuncIdxDict.TryGetValue(funcName, out index))
            {
                index = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据函数索引获取其定义
        /// </summary>
        /// <param name="index"></param>
        /// <param name="funcDef"></param>
        /// <returns></returns>
        public bool TryGetFunc(int index, out ByteCodeFuncDef funcDef)
        {
            if (index >= 0 && index < _regFuncs.Count)
            {
                funcDef = _regFuncs[index];
                return true;
            }
            funcDef = null;
            return false;
        }

        /// <summary>
        /// 根据函数名获取其定义
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="funcDef"></param>
        /// <returns></returns>
        public bool TryGetFunc(string funcName, out ByteCodeFuncDef funcDef)
        {
            int index = 0;
            if (!_regFuncIdxDict.TryGetValue(funcName, out index))
            {
                funcDef = null;
                return false;
            }
            funcDef = _regFuncs[index];
            return true;
        }

        /// <summary>
        /// 注册基础变量和函数
        /// </summary>
        public void RegBase()
        {
            RegBaseVarGetter();
            RegBaseFunc();
        }

        /// <summary>
        /// 注册基础变量
        /// </summary>
        public void RegBaseVarGetter()
        {
            RegVarGetter("PI", (System.Object userData, ref TValue returnValue) => {
#if EXPV_DEBUG
                sLogger.LogFormat("==============PI");
#endif
                returnValue.SetFloat(Mathf.PI);
            });
        }
        
        /// <summary>
        /// 注册基础函数
        /// </summary>
        public void RegBaseFunc()
        {
            RegFunc("abs", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============abs arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueAbs(ref returnValue);
            });

            RegFunc("floor", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============floor arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueFloor(ref returnValue);
            });

            RegFunc("ceil", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============ceil arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueCeil(ref returnValue);
            });

            RegFunc("cos", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============cos arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueCos(ref returnValue);
            });

            RegFunc("sin", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============sin arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueSin(ref returnValue);
            });

            RegFunc("tan", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============tan arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueTan(ref returnValue);
            });

            RegFunc("acos", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============acos arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueAcos(ref returnValue);
            });

            RegFunc("asin", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============asin arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueAsin(ref returnValue);
            });

            RegFunc("atan", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============atan arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueAtan(ref returnValue);
            });

            RegFunc("log", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============log arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueLog(ref returnValue);
            });
            
            RegFunc("max", 2, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
                TValue tv2 = args[1];
#if EXPV_DEBUG
                sLogger.LogFormat("=============max arg tv1 type={0} value={1} tv2 type={2} value={3}", returnValue.type, returnValue.value, tv2.type, tv2.value);
#endif
                TValueExt.TValueMax(ref returnValue, ref tv2);
            });
            
            RegFunc("min", 2, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
                TValue tv2 = args[1];
#if EXPV_DEBUG
                sLogger.LogFormat("=============min arg tv1 type={0} value={1} tv2 type={2} value={3}", returnValue.type, returnValue.value, tv2.type, tv2.value);
#endif
                TValueExt.TValueMin(ref returnValue, ref tv2);
            });

            RegFunc("round", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============round arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                TValueExt.TValueRound(ref returnValue);
            });

            RegFunc("random", 2, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
                TValue tv2 = args[1];
#if EXPV_DEBUG
                sLogger.LogFormat("=============random arg tv1 type={0} value={1} tv2 type={2} value={3}", returnValue.type, returnValue.value, tv2.type, tv2.value);
#endif
                TValueExt.TValueRandom(ref returnValue, ref tv2);
            });

            RegFunc("print", 1, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                returnValue = args[0];
#if EXPV_DEBUG
                sLogger.LogFormat("=============print arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                LogLevel backupLogLevel = sLogger.logLevel;
                sLogger.logLevel = LogLevel.Log;
                sLogger.Log(returnValue.value.ToString());
                sLogger.logLevel = backupLogLevel;
            });

            RegFunc("printf", 2, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
                string strFormat = args[0].valStr;
                returnValue = args[1];
#if EXPV_DEBUG
                sLogger.LogFormat("=============printf arg tv type={0} value={1}", returnValue.type, returnValue.value);
#endif
                LogLevel backupLogLevel = sLogger.logLevel;
                sLogger.logLevel = LogLevel.Log;
                sLogger.LogFormat(strFormat, returnValue.value);
                sLogger.logLevel = backupLogLevel;
            });

            RegFunc("ifelse", 3, (System.Object userData, List<TValue> args, ref TValue returnValue) => {
#if EXPV_DEBUG
                sLogger.LogFormat("=============ifelse arg0 type={0} value={1}", args[0].type, args[0].value);
#endif
                if (args[0].ToBool())
                {
                    returnValue = args[1];
                }
                else
                {
                    returnValue = args[2];
                }
            });
        }
    }
}