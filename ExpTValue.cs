/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
值类型
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExpParseV.ExpIdentifierProvider;

namespace ExpParseV
{
    public enum ValueType
    {
        /// <summary>
        /// 空类型
        /// </summary>
        VT_NONE = 0,
        /// <summary>
        /// 布尔值
        /// </summary>
        VT_BOOL,
        /// <summary>
        /// 32位整数
        /// </summary>
        VT_INT,
        /// <summary>
        /// 32位浮点数
        /// </summary>
        VT_FLOAT,
        /// <summary>
        /// 64位浮点数
        /// </summary>
        VT_DOUBLE,
        /// <summary>
        /// 字符串(只支持ASCII)
        /// </summary>
        VT_STR,
    }

    /*
     * TValue字节流格式：
     * 布尔值：ValueType(1个字节)|value(1个字节)
     * 32位整数：ValueType(1个字节)|value(4个字节)
     * 32位浮点数：ValueType(1个字节)|value(4个字节)
     * 64位浮点数：ValueType(1个字节)|value(8个字节)
     * 字符串：ValueType(1个字节)|StrType(1个字节)|字符串长度(2个字节)|字符串索引(2个字节)|字符串内容
    */

    /// <summary>
    /// 字符串类型
    /// </summary>
    public enum StrType
    {
        /// <summary>
        /// 默认
        /// </summary>
        ST_NONE = 0,
        /// <summary>
        /// 变量名
        /// </summary>
        ST_VAR_NAME,
        /// <summary>
        /// 函数名
        /// </summary>
        ST_FUNC_NAME,
    }

    /// <summary>
    /// 类型值
    /// </summary>
    public struct TValue
    {
        public const int STR_TYPE_BYTE_SIZE = 1;
        public const int STR_LEN_BYTE_SIZE = 2;
        public const int STR_PID_BYTE_SIZE = 2;
        public const int STR_IDX_BYTE_SIZE = 2;
        public const int STR_HEAD_DATA_BYTE_SIZE = STR_TYPE_BYTE_SIZE + STR_LEN_BYTE_SIZE + STR_PID_BYTE_SIZE + STR_IDX_BYTE_SIZE;
        public const int STR_HEAD_BYTE_SIZE = 1 + STR_HEAD_DATA_BYTE_SIZE;

        public const int STR_PID_BIT_SIZE = STR_PID_BYTE_SIZE * 8;
        public const int INT_BIT_MASK1 = ~0;
        public const int SHORT_BIT_MASK1 = ~(INT_BIT_MASK1 << 16);

        /// <summary>
        /// 字符串最大长度
        /// </summary>
        public const int STR_LEN_MAX = ushort.MaxValue;
        /// <summary>
        /// 字符串provider id最大值
        /// </summary>
        public const int STR_PID_MAX = ushort.MaxValue;
        public const ushort STR_PID_NONE = 0;
        /// <summary>
        /// 字符串索引最大值
        /// </summary>
        public const int STR_IDX_MAX = short.MaxValue;
        public const short STR_IDX_NONE = -1;
        
        ValueType _type;
        string _valStr;
        double _value;

        public ValueType type
        {
            get
            {
                return _type;
            }
        }

        public bool typeIsNumber
        {
            get
            {
                return _type == ValueType.VT_INT || _type == ValueType.VT_FLOAT || _type == ValueType.VT_DOUBLE;
            }
        }

        public string valStr
        {
            get
            {
                return _valStr;
            }
        }

        public double value
        {
            get
            {
                return _value;
            }
        }

        public TValue(ValueType type, bool value)
        {
            _type = type;
            _valStr = string.Empty;
            if (value)
            {
                _value = 1;
            }
            else
            {
                _value = 0;
            }
        }
        public TValue(ValueType type, int value)
        {
            _type = type;
            _valStr = string.Empty;
            _value = (double)value;
        }
        public TValue(ValueType type, float value)
        {
            _type = type;
            _valStr = string.Empty;
            _value = (double)value;
        }
        public TValue(ValueType type, double value)
        {
            _type = type;
            _valStr = string.Empty;
            _value = value;
        }
        public TValue(ValueType type, string value)
        {
            _type = type;
            _valStr = value;
            _value = 0;
        }

        public void SetBool(bool value)
        {
            _type = ValueType.VT_BOOL;
            _valStr = string.Empty;
            if (value)
            {
                _value = 1;
            }
            else
            {
                _value = 0;
            }
        }

        public void SetInt(int value)
        {
            _type = ValueType.VT_INT;
            _valStr = string.Empty;
            _value = (double)value;
        }

        public void SetFloat(float value)
        {
            _type = ValueType.VT_FLOAT;
            _valStr = string.Empty;
            _value = (double)value;
        }

        public void SetDouble(double value)
        {
            _type = ValueType.VT_DOUBLE;
            _valStr = string.Empty;
            _value = value;
        }

        public void SetString(string value)
        {
            _type = ValueType.VT_STR;
            _valStr = value;
            _value = 0;
            
            int strLen = 0;
            if (!string.IsNullOrEmpty(_valStr))
            {
                strLen = _valStr.Length;
            }
            if (strLen > TValue.STR_LEN_MAX)
            {
                throw new Exception(string.Format("TValue SetString error! value={0}…… len={1} too long", _valStr.Substring(0, 20), strLen));
            }
        }

        public bool ToBool()
        {
            return _value != 0;
        }

        public bool TryToBool(out bool value)
        {
            if (_type == ValueType.VT_STR)
            {
                if (!string.IsNullOrEmpty(_valStr) && bool.TryParse(_valStr, out value))
                {
                    return true;
                }
                else
                {
                    value = false;
                    return false;
                }
            }
            else if (_type == ValueType.VT_NONE)
            {
                value = false;
                return false;
            }
            else
            {
                value = (_value != 0);
                return true;
            }
        }
        
        public int ToInt()
        {
            return (int)_value;
        }

        public bool TryToInt(out int value)
        {
            if (_type == ValueType.VT_STR)
            {
                if (!string.IsNullOrEmpty(_valStr) && int.TryParse(_valStr, out value))
                {
                    return true;
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
            else if (_type == ValueType.VT_NONE)
            {
                value = 0;
                return false;
            }
            else
            {
                value = (int)_value;
                return true;
            }
        }

        public float ToFloat()
        {
            return (float)_value;
        }

        public bool TryToFloat(out float value)
        {
            if (_type == ValueType.VT_STR)
            {
                if (!string.IsNullOrEmpty(_valStr) && float.TryParse(_valStr, out value))
                {
                    return true;
                }
                else
                {
                    value = 0f;
                    return false;
                }
            }
            else if(_type == ValueType.VT_NONE)
            {
                value = 0f;
                return false;
            }
            else
            {
                value = (float)_value;
                return true;
            }
        }

        public bool TryToDouble(out double value)
        {
            if (_type == ValueType.VT_STR)
            {
                if (!string.IsNullOrEmpty(_valStr) && double.TryParse(_valStr, out value))
                {
                    return true;
                }
                else
                {
                    value = 0f;
                    return false;
                }
            }
            else if (_type == ValueType.VT_NONE)
            {
                value = 0f;
                return false;
            }
            else
            {
                value = _value;
                return true;
            }
        }

        public byte[] ToByteArray(StrType strType = StrType.ST_NONE)
        {
            switch (_type)
            {
                case ValueType.VT_BOOL:
                    {
                        byte value = 0;
                        if (_value != 0)
                        {
                            value = 1;
                        }
                        byte[] bytes = new byte[] { (byte)_type, value };
                        return bytes;
                    }
                case ValueType.VT_INT:
                    {
                        byte[] bytes = new byte[5];
                        bytes[0] = (byte)_type;
                        byte[] valueBytes = BitConverter.GetBytes(ToInt());
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }
                case ValueType.VT_FLOAT:
                    {
                        byte[] bytes = new byte[5];
                        bytes[0] = (byte)_type;
                        byte[] valueBytes = BitConverter.GetBytes(ToFloat());
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }
                case ValueType.VT_DOUBLE:
                    {
                        byte[] bytes = new byte[9];
                        bytes[0] = (byte)_type;
                        byte[] valueBytes = BitConverter.GetBytes(_value);
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }
                case ValueType.VT_STR:
                    {
                        int strLen = 0;
                        if (!string.IsNullOrEmpty(_valStr))
                        {
                            strLen = _valStr.Length;
                        }

                        if(strLen > TValue.STR_LEN_MAX)
                        {
                            throw new Exception(string.Format("TValue ToByteArray error! str={0}…… len={1} too long", _valStr.Substring(0, 20), strLen));
                        }
                        
                        byte[] bytes = new byte[TValue.STR_HEAD_BYTE_SIZE + strLen];
                        bytes[0] = (byte)_type;
                        bytes[1] = (byte)strType;
                        byte[] strLenBytes = BitConverter.GetBytes((ushort)strLen);
                        strLenBytes.CopyTo(bytes, 2);
                        //字符串pid
                        byte[] strPidBytes = BitConverter.GetBytes(TValue.STR_PID_NONE);
                        strPidBytes.CopyTo(bytes, 4);
                        //字符串索引
                        byte[] strIdxBytes = BitConverter.GetBytes(TValue.STR_IDX_NONE);
                        strIdxBytes.CopyTo(bytes, 6);
                        for (int i = 0; i < strLen; i++)
                        {
                            bytes[TValue.STR_HEAD_BYTE_SIZE + i] = (byte)_valStr[i];
                        }
                        return bytes;
                    }

            }

            return null;
        }

        public bool FromByteArray(byte[] bytes, int startIndex, ExpByteCodeVM byteCodeVM = null)
        {
            if (bytes == null)
            {
                return false;
            }

            int bytesLen = bytes.Length;
            if (startIndex < 0 || startIndex >= bytesLen)
            {
                return false;
            }

            int count = 0;
            int endIndex = 0;

            ValueType type = (ValueType)bytes[startIndex];
            switch (type)
            {
                case ValueType.VT_BOOL:
                    count = 1;
                    endIndex = startIndex + count;
                    if (endIndex >= bytesLen)
                    {
                        return false;
                    }

                    SetBool(bytes[endIndex] != 0);
                    return true;
                case ValueType.VT_INT:
                    count = 4;
                    endIndex = startIndex + count;
                    if (endIndex >= bytesLen)
                    {
                        return false;
                    }

                    SetInt(BitConverter.ToInt32(bytes, startIndex + 1));
                    return true;
                case ValueType.VT_FLOAT:
                    count = 4;
                    endIndex = startIndex + count;
                    if (endIndex >= bytesLen)
                    {
                        return false;
                    }

                    SetFloat(BitConverter.ToSingle(bytes, startIndex + 1));
                    return true;
                case ValueType.VT_DOUBLE:
                    count = 8;
                    endIndex = startIndex + count;
                    if (endIndex >= bytesLen)
                    {
                        return false;
                    }

                    SetDouble(BitConverter.ToDouble(bytes, startIndex + 1));
                    return true;
                case ValueType.VT_STR:
                    count = TValue.STR_HEAD_DATA_BYTE_SIZE;
                    endIndex = startIndex + count;
                    if (endIndex >= bytesLen)
                    {
                        return false;
                    }

                    StrType strType = (StrType)bytes[startIndex + 1];
                    int strLen = BitConverter.ToUInt16(bytes, startIndex + 2);
                    int strPidStartIndex = startIndex + 4;
                    int strPid = BitConverter.ToUInt16(bytes, strPidStartIndex);
                    int strIdxStartIndex = startIndex + 6;
                    int strIdx = BitConverter.ToInt16(bytes, strIdxStartIndex);
                    
                    string strValue = string.Empty;
                    if (strLen > 0)
                    {
                        int strStartIndex = startIndex + 8;
                        if (byteCodeVM != null)
                        {
                            if(strIdx >= 0)
                            {
                                //通过字符串pid和索引查找字符串
                                switch (strType)
                                {
                                    case StrType.ST_VAR_NAME:
                                        {
                                            ExpIdentifierProvider baseIdentProvider = byteCodeVM.baseIdentProvider;
                                            ExpIdentifierProvider identProvider = byteCodeVM.identProvider;
                                            ByteCodeVarGetterDef getterDef = null;
                                            if(strPid == baseIdentProvider.id)
                                            {
                                                baseIdentProvider.TryGetVarGetter(strIdx, out getterDef);
                                            }
                                            else if (identProvider == null || identProvider.id != strPid || !identProvider.TryGetVarGetter(strIdx, out getterDef))
                                            {
                                                strIdx = TValue.STR_IDX_NONE;
                                            }

                                            if (getterDef != null)
                                            {
                                                strValue = getterDef.name;
                                            }
                                            break;
                                        }
                                    case StrType.ST_FUNC_NAME:
                                        {
                                            ExpIdentifierProvider baseIdentProvider = byteCodeVM.baseIdentProvider;
                                            ExpIdentifierProvider identProvider = byteCodeVM.identProvider;
                                            ByteCodeFuncDef funcDef = null;
                                            if (strPid == baseIdentProvider.id)
                                            {
                                                baseIdentProvider.TryGetFunc(strIdx, out funcDef);
                                            }
                                            else if (identProvider == null || identProvider.id != strPid || !identProvider.TryGetFunc(strIdx, out funcDef))
                                            {
                                                strIdx = TValue.STR_IDX_NONE;
                                            }

                                            if (funcDef != null)
                                            {
                                                strValue = funcDef.name;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            ExpStrIdxTable strIdxTable = byteCodeVM.strIdxTable;
                                            strIdxTable.TryGetStr(strIdx, out strValue);
                                            break;
                                        }
                                }
                            }
                            
                            if (strIdx < 0)
                            {
                                //查找字符串pid和索引
                                strValue = Encoding.ASCII.GetString(bytes, strStartIndex, strLen);
                                switch(strType)
                                {
                                    case StrType.ST_VAR_NAME:
                                        {
                                            ExpIdentifierProvider identProvider = byteCodeVM.identProvider;
                                            if (identProvider == null || !identProvider.TryGetVarGetterIdx(strValue, out strIdx))
                                            {
                                                ExpIdentifierProvider baseIdentProvider = byteCodeVM.baseIdentProvider;
                                                if (baseIdentProvider.TryGetVarGetterIdx(strValue, out strIdx))
                                                {
                                                    strPid = baseIdentProvider.id;
                                                }
                                            }
                                            else
                                            {
                                                strPid = identProvider.id;
                                            }
                                            break;
                                        }
                                    case StrType.ST_FUNC_NAME:
                                        {
                                            ExpIdentifierProvider identProvider = byteCodeVM.identProvider;
                                            if (identProvider == null || !identProvider.TryGetFuncIdx(strValue, out strIdx))
                                            {
                                                ExpIdentifierProvider baseIdentProvider = byteCodeVM.baseIdentProvider;
                                                if(baseIdentProvider.TryGetFuncIdx(strValue, out strIdx))
                                                {
                                                    strPid = baseIdentProvider.id;
                                                }
                                            }
                                            else
                                            {
                                                strPid = identProvider.id;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            ExpStrIdxTable strIdxTable = byteCodeVM.strIdxTable;
                                            if(!strIdxTable.TryGetStrIdx(strValue, out strIdx))
                                            {
                                                strIdx = strIdxTable.RegStr(strValue, false);
                                            }
                                            strPid = strIdxTable.id;
                                            break;
                                        }
                                }
                                
                                if (strIdx >= 0)
                                {
                                    if(strIdx <= TValue.STR_IDX_MAX)
                                    {
                                        //缓存字符串pid和索引
                                        byte[] strPidBytes = BitConverter.GetBytes((ushort)strPid);
                                        strPidBytes.CopyTo(bytes, strPidStartIndex);
                                        byte[] strIdxBytes = BitConverter.GetBytes((short)strIdx);
                                        strIdxBytes.CopyTo(bytes, strIdxStartIndex);
                                    }
                                }
                            }
                        }

                        if(string.IsNullOrEmpty(strValue))
                        {
                            if(byteCodeVM != null)
                            {
                                //通过字符串pid和索引查找字符串失败
                                return false;
                            }
                            strValue = Encoding.ASCII.GetString(bytes, strStartIndex, strLen);
                        }
                    }
                    SetString(strValue);
                    _value = (strPid << STR_PID_BIT_SIZE) | strIdx;
                    return true;
            }

            return false;
        }

        public bool FromByteArray(byte[] bytes)
        {
            return FromByteArray(bytes, 0);
        }
    }
    
    /// <summary>
    /// TValue扩展
    /// </summary>
    public static class TValueExt
    {
        public readonly static TValue sDefaultTValue = new TValue();
        public static System.Random sRandom = new System.Random();

        /// <summary>
        /// TValue加
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueAdd(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() + tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(tv1.ToFloat() + tv2.ToFloat());
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(val1 + val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to + on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue减
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueSub(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() - tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(tv1.ToFloat() - tv2.ToFloat());
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(val1 - val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to - on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue乘
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueMul(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() * tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(tv1.ToFloat() * tv2.ToFloat());
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(val1 * val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to * on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue除
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueDiv(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(tv1.ToFloat() / tv2.ToFloat());
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(val1 / val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to / on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue求余
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueMod(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() % tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(tv1.ToFloat() % tv2.ToFloat());
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(val1 % val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to % on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-与
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueBand(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() & tv2.ToInt());
            }
            else
            {
                isOk = false;
                if ((tv1.type == ValueType.VT_INT || tv1.type == ValueType.VT_STR) && (tv2.type == ValueType.VT_INT || tv2.type == ValueType.VT_STR))
                {
                    int val1;
                    int val2;
                    if (tv1.TryToInt(out val1) && tv2.TryToInt(out val2))
                    {
                        tv1.SetInt(val1 % val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to & on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-或
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueBor(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() | tv2.ToInt());
            }
            else
            {
                isOk = false;
                if ((tv1.type == ValueType.VT_INT || tv1.type == ValueType.VT_STR) && (tv2.type == ValueType.VT_INT || tv2.type == ValueType.VT_STR))
                {
                    int val1;
                    int val2;
                    if (tv1.TryToInt(out val1) && tv2.TryToInt(out val2))
                    {
                        tv1.SetInt(val1 | val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to | on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-异或
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueBxor(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() ^ tv2.ToInt());
            }
            else
            {
                isOk = false;
                if ((tv1.type == ValueType.VT_INT || tv1.type == ValueType.VT_STR) && (tv2.type == ValueType.VT_INT || tv2.type == ValueType.VT_STR))
                {
                    int val1;
                    int val2;
                    if (tv1.TryToInt(out val1) && tv2.TryToInt(out val2))
                    {
                        tv1.SetInt(val1 ^ val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to ^ on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-左移
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueBshl(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() << tv2.ToInt());
            }
            else
            {
                isOk = false;
                if ((tv1.type == ValueType.VT_INT || tv1.type == ValueType.VT_STR) && (tv2.type == ValueType.VT_INT || tv2.type == ValueType.VT_STR))
                {
                    int val1;
                    int val2;
                    if (tv1.TryToInt(out val1) && tv2.TryToInt(out val2))
                    {
                        tv1.SetInt(val1 << val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to << on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-右移
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueBshr(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(tv1.ToInt() >> tv2.ToInt());
            }
            else
            {
                isOk = false;
                if ((tv1.type == ValueType.VT_INT || tv1.type == ValueType.VT_STR) && (tv2.type == ValueType.VT_INT || tv2.type == ValueType.VT_STR))
                {
                    int val1;
                    int val2;
                    if (tv1.TryToInt(out val1) && tv2.TryToInt(out val2))
                    {
                        tv1.SetInt(val1 >> val2);
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to << on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue负
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueNeg(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_INT)
            {
                tv.SetInt(-tv.ToInt());
            }
            else if (tv.typeIsNumber)
            {
                tv.SetFloat(-tv.ToFloat());
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to neg on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue位运算-取反
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueBnot(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_INT)
            {
                tv.SetInt(~tv.ToInt());
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to ~ on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue幂运算
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValuePow(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt((int)Math.Pow(tv1.ToFloat(), tv2.ToFloat()));
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat((float)Math.Pow(tv1.ToFloat(), tv2.ToFloat()));
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat((float)Math.Pow(val1, val2));
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to pow on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue小于
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueLessThan(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetBool(tv1.ToInt() < tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetBool(tv1.ToFloat() < tv2.ToFloat());
            }
            else if (tv1.type == ValueType.VT_STR && tv2.type == ValueType.VT_STR)
            {
                tv1.SetBool(string.Compare(tv1.valStr, tv2.valStr) == -1);
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to < on {0} value and {1} value", tv1.type, tv2.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue等于
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueEqual(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetBool(tv1.ToInt() == tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetBool(tv1.ToFloat() == tv2.ToFloat());
            }
            else if (tv1.type == ValueType.VT_STR && tv2.type == ValueType.VT_STR)
            {
                tv1.SetBool(string.Compare(tv1.valStr, tv2.valStr) == 0);
            }
            else if (tv1.type == ValueType.VT_BOOL && tv2.type == ValueType.VT_BOOL)
            {
                tv1.SetBool(tv1.ToBool() == tv2.ToBool());
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to equal on {0} value and {1} value", tv1.type, tv2.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue小于等于
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueLessEqual(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetBool(tv1.ToInt() <= tv2.ToInt());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetBool(tv1.ToFloat() <= tv2.ToFloat());
            }
            else if (tv1.type == ValueType.VT_STR && tv2.type == ValueType.VT_STR)
            {
                tv1.SetBool(string.Compare(tv1.valStr, tv2.valStr) <= 0);
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to <= on {0} value and {1} value", tv1.type, tv2.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue逻辑与
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueAnd(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_BOOL && tv2.type == ValueType.VT_BOOL)
            {
                tv1.SetBool(tv1.ToBool() && tv2.ToBool());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetBool(tv1.ToBool() && tv2.ToBool());
            }
            else
            {
                isOk = false;
                bool val1;
                bool val2;
                if (tv1.TryToBool(out val1) && tv2.TryToBool(out val2))
                {
                    tv1.SetBool(val1 && val2);
                    isOk = true;
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to && on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue逻辑或
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueOr(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_BOOL && tv2.type == ValueType.VT_BOOL)
            {
                tv1.SetBool(tv1.ToBool() || tv2.ToBool());
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetBool(tv1.ToBool() || tv2.ToBool());
            }
            else
            {
                isOk = false;
                bool val1;
                bool val2;
                if (tv1.TryToBool(out val1) && tv2.TryToBool(out val2))
                {
                    tv1.SetBool(val1 || val2);
                    isOk = true;
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to || on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue逻辑非
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueNot(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_BOOL || tv.typeIsNumber)
            {
                tv.SetBool(!tv.ToBool());
            }
            else if(tv.type == ValueType.VT_STR)
            {
                float val;
                if (tv.TryToFloat(out val))
                {
                    tv.SetBool(!(val != 0f));
                    isOk = true;
                }
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to ! on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue绝对值
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueAbs(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_INT)
            {
                tv.SetInt(Math.Abs(tv.ToInt()));
            }
            else if (tv.typeIsNumber)
            {
                tv.SetFloat(Math.Abs(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to abs on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue向下取整
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueFloor(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_FLOAT)
            {
                tv.SetInt((int)Math.Floor(tv.value));
            }
            else if (tv.type == ValueType.VT_DOUBLE)
            {
                tv.SetInt((int)Math.Floor(tv.value));
            }
            else if (tv.typeIsNumber)
            {

            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to floor on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue向上取整
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueCeil(ref TValue tv)
        {
            bool isOk = true;
            if (tv.type == ValueType.VT_FLOAT)
            {
                tv.SetInt((int)Math.Ceiling(tv.value));
            }
            else if (tv.type == ValueType.VT_DOUBLE)
            {
                tv.SetInt((int)Math.Ceiling(tv.value));
            }
            else if (tv.typeIsNumber)
            {

            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to ceil on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue余弦
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueCos(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Cos(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to cos on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue正弦
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueSin(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Sin(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to sin on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue正切
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueTan(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Tan(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to tan on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue反余弦
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueAcos(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Acos(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to acos on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue反正弦
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueAsin(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Asin(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to asin on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue反正切
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueAtan(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Atan(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to atan on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue对数
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueLog(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Log(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to log on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue最大值
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueMax(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(Math.Max(tv1.ToInt(), tv2.ToInt()));
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(Math.Max(tv1.ToFloat(), tv2.ToFloat()));
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(Math.Max(val1, val2));
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to max on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue最小值
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueMin(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.type == ValueType.VT_INT && tv2.type == ValueType.VT_INT)
            {
                tv1.SetInt(Math.Min(tv1.ToInt(), tv2.ToInt()));
            }
            else if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetFloat(Math.Min(tv1.ToFloat(), tv2.ToFloat()));
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(Math.Min(val1, val2));
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to min on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }

        /// <summary>
        /// TValue四舍五入
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static bool TValueRound(ref TValue tv)
        {
            bool isOk = true;
            if (tv.typeIsNumber)
            {
                tv.SetFloat((float)Math.Round(tv.ToFloat()));
            }
            else
            {
                isOk = false;
                throw new Exception(string.Format("attempt to round on {0} value", tv.type));
            }

            return isOk;
        }

        /// <summary>
        /// TValue随机数
        /// </summary>
        /// <param name="tv1"></param>
        /// <param name="tv2"></param>
        /// <returns></returns>
        public static bool TValueRandom(ref TValue tv1, ref TValue tv2)
        {
            bool isOk = true;
            if (tv1.typeIsNumber && tv2.typeIsNumber)
            {
                tv1.SetInt(sRandom.Next(tv1.ToInt(), tv2.ToInt()));
            }
            else
            {
                isOk = false;
                if (tv1.type != ValueType.VT_BOOL && tv2.type != ValueType.VT_BOOL)
                {
                    float val1;
                    float val2;
                    if (tv1.TryToFloat(out val1) && tv2.TryToFloat(out val2))
                    {
                        tv1.SetFloat(sRandom.Next((int)val1, (int)val2));
                        isOk = true;
                    }
                }

                if (!isOk)
                {
                    throw new Exception(string.Format("attempt to random on {0} value and {1} value", tv1.type, tv2.type));
                }
            }

            return isOk;
        }
    }
}

