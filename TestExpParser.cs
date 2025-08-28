/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
示例
author：尧
*/
using ExpParseV;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpByteCodeVM;

public class TestExpParser : MonoBehaviour
{
    static string WordEndCharStr = " +-*/%><!=()\t\r\n";
    static char[] WordEndChars = WordEndCharStr.ToCharArray();

    [Multiline]
    public string sourceCode = "";

    public string numStr = "";

    ExpParser _expParser = new ExpParser();
    
    // Start is called before the first frame update
    void Start()
    {
        //_expParser.ParseExp(sourceCode);
        /*
        while(!string.IsNullOrEmpty(expStr))
        {
            Debug.LogFormat("=============nextToken={0}", _NextToken(ref expStr));
            Debug.LogFormat("=============expStr={0}", expStr);
        }
        Debug.LogFormat("=============expStr len={0}", expStr.Length);
        */
        /*
        double ret;
        Debug.LogFormat("=============str={0} to double isok={1} ret={2}", numStr, double.TryParse(numStr, out ret), ret);
        Debug.LogFormat("=============double={0} to int ret={1}", ret, (int)ret);
        int intvar = 66;
        Debug.LogFormat("=============int={0} to double ret={1}", intvar, (double)intvar);
        Debug.LogFormat("=============double={0} to float ret={1}", ret, (float)ret);
        float fltvar = 66.6f;
        Debug.LogFormat("=============float={0} to double ret={1}", fltvar, (double)fltvar);
        Debug.LogFormat("=============double={0} to byte ret={1}", ret, (byte)ret);
        byte bytevar = 6;
        Debug.LogFormat("=============byte={0} to double ret={1}", bytevar, (double)bytevar);
        */
        
        TValue tv = new TValue();
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.value);
        tv.SetInt(-12);
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.value);
        byte[] bytes = tv.ToByteArray();
        Debug.LogFormat("=============tv byte[]={0} value={1} type={2} value={3}", bytes.Length, tv.FromByteArray(bytes), tv.type, tv.value);
        tv.SetBool(false);
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.value);
        bytes = tv.ToByteArray();
        Debug.LogFormat("=============tv byte[]={0} value={1} type={2} value={3}", bytes.Length, tv.FromByteArray(bytes), tv.type, tv.value);
        tv.SetFloat(-6);
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.value);
        bytes = tv.ToByteArray();
        Debug.LogFormat("=============tv byte[]={0} value={1} type={2} value={3}", bytes.Length, tv.FromByteArray(bytes), tv.type, tv.value);
        tv.SetString("name_1 Name126");
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.valStr);
        bytes = tv.ToByteArray();
        Debug.LogFormat("=============tv byte[]={0} value={1} type={2} value={3}", bytes.Length, tv.FromByteArray(bytes), tv.type, tv.valStr);
        tv.SetDouble(-9);
        Debug.LogFormat("=============tv type={0} value={1}", tv.type, tv.value);
        bytes = tv.ToByteArray();
        Debug.LogFormat("=============tv byte[]={0} value={1} type={2} value={3}", bytes.Length, tv.FromByteArray(bytes), tv.type, tv.value);
        
    }
    /*
    string _NextToken(ref string inExpStr)
    {
        inExpStr = inExpStr.Trim();
        if (string.IsNullOrEmpty(inExpStr))
        {
            return "";
        }
        
        int subLen = 0;

        //运算符
        char subChar = inExpStr[0];
        switch(subChar)
        {
            case '+':
            case '-':
            case '*':
            case '/':
            case '%':
            case '(':
            case ')':
                subLen = 1;
                break;
            case '>':
            case '<':
            case '!':
                if (inExpStr.Length > 1 && inExpStr[1] == '=')
                {
                    subLen = 2;
                    break;
                }
                subLen = 1;
                break;
            case '=':
                subLen = 2;
                break;
        }

        if(subLen == 0)
        {
            //数值
            int numCount = 0;
            bool visitedPoint = false;
            for (int i = 0; i < inExpStr.Length; i++)
            {
                subChar = inExpStr[i];
                if(subChar < '0' || subChar > '9')
                {
                    if(subChar == '.' && !visitedPoint)
                    {
                        visitedPoint = true;
                        numCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    numCount++;
                }
            }

            subLen = numCount;
        }

        if(subLen == 0)
        {
            //单词
            int worldEndIdx = inExpStr.IndexOfAny(WordEndChars);
            if(worldEndIdx > 0)
            {
                subLen = worldEndIdx;
            }
            else if(worldEndIdx == 0)
            {
                subLen = 1;
            }
            else
            {
                subLen = inExpStr.Length;
            }
        }

        if(subLen > 0)
        {
            string token = inExpStr.Substring(0, subLen);
            inExpStr = inExpStr.Substring(subLen);
            return token;
        }

        return "";
    }
    
    public const byte VT_NONE = 0;
    public const byte VT_BYTE = 1;
    public const byte VT_BOOL = 2;
    public const byte VT_INT = 3;
    public const byte VT_FLOAT = 4;
    public const byte VT_DOUBLE = 5;

    public struct TValue
    {
        byte _type;
        double _value;

        public byte type
        {
            get
            {
                return _type;
            }
        }

        public double value
        {
            get
            {
                return _value;
            }
        }
        
        public TValue(byte type, sbyte value)
        {
            _type = type;
            _value = (double)value;
        }
        public TValue(byte type, bool value)
        {
            _type = type;
            if (value)
            {
                _value = 1;
            }
            else
            {
                _value = 0;
            }
        }
        public TValue(byte type, int value)
        {
            _type = type;
            _value = (double)value;
        }
        public TValue(byte type, float value)
        {
            _type = type;
            _value = (double)value;
        }
        public TValue(byte type, double value)
        {
            _type = type;
            _value = value;
        }

        public void SetByte(sbyte value)
        {
            _type = VT_BYTE;
            _value = (double)value;
        }
        
        public void SetBool(bool value)
        {
            _type = VT_BOOL;
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
            _type = VT_INT;
            _value = (double)value;
        }

        public void SetFloat(float value)
        {
            _type = VT_FLOAT;
            _value = (double)value;
        }
        
        public void SetDouble(double value)
        {
            _type = VT_DOUBLE;
            _value = value;
        }

        public sbyte ToByte()
        {
            return (sbyte)_value;
        }

        public bool ToBool()
        {
            if(_value == 1)
            {
                return true;
            }
            return false;
        }

        public int ToInt()
        {
            return (int)_value;
        }

        public float ToFloat()
        {
            return (float)_value;
        }

        public byte[] ToByteArray()
        {
            switch(_type)
            {
                case VT_BYTE:
                case VT_BOOL:
                    {
                        byte[] bytes = new byte[] { _type, (byte)(_value + 128) };
                        return bytes;
                    }
                case VT_INT:
                    {
                        byte[] bytes = new byte[5];
                        bytes[0] = _type;
                        byte[] valueBytes = BitConverter.GetBytes(ToInt());
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }
                case VT_FLOAT:
                    {
                        byte[] bytes = new byte[5];
                        bytes[0] = _type;
                        byte[] valueBytes = BitConverter.GetBytes(ToFloat());
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }
                case VT_DOUBLE:
                    {
                        byte[] bytes = new byte[9];
                        bytes[0] = _type;
                        byte[] valueBytes = BitConverter.GetBytes(_value);
                        valueBytes.CopyTo(bytes, 1);
                        return bytes;
                    }

            }

            return null;
        }

        public bool FromByteArray(byte[] bytes)
        {
            if(bytes == null || bytes.Length < 1)
            {
                return false;
            }

            byte type = bytes[0];
            switch(type)
            {
                case VT_BYTE:
                    if (bytes.Length > 1)
                    {
                        SetByte((sbyte)(bytes[1] - 128));
                    }
                    else
                    {
                        SetByte(0);
                    }
                    return true;
                case VT_BOOL:
                    if (bytes.Length > 1)
                    {
                        SetBool(bytes[1] == 1);
                    }
                    else
                    {
                        SetBool(false);
                    }
                    return true;
                case VT_INT:
                    if (bytes.Length > 4)
                    {
                        SetInt(BitConverter.ToInt32(bytes, 1));
                    }
                    else
                    {
                        SetInt(0);
                    }
                    return true;
                case VT_FLOAT:
                    if (bytes.Length > 4)
                    {
                        SetFloat(BitConverter.ToSingle(bytes, 1));
                    }
                    else
                    {
                        SetFloat(0);
                    }
                    return true;
                case VT_DOUBLE:
                    if (bytes.Length > 8)
                    {
                        SetDouble(BitConverter.ToDouble(bytes, 1));
                    }
                    else
                    {
                        SetDouble(0);
                    }
                    return true;
            }

            return false;
        }
    }
    
    Stack<string> _optrStack = new Stack<string>();

    public const byte OP_NONE = 0;
    public const byte OP_ADD = 1;
    
    /// <summary>
    /// 值越大优先级越高
    /// </summary>
    /// <param name="optr">运算符</param>
    /// <returns></returns>
    int _GetOperatorPriority(string optr)
    {
        if (string.IsNullOrEmpty(optr))
        {
            return 0;
        }
        
        switch (optr)
        {
            case "(":
            case ")":
                return 15;
            case "*":
            case "/":
            case "%":
                return 13;
            case "+":
            case "-":
                return 12;
            case ">":
            case ">=":
            case "<":
            case "<=":
                return 10;
            case "==":
            case "!=":
                return 9;
            case "&&":
                return 5;
            case "||":
                return 4;
        }

        return 0;
    }

    bool _CheckOptrPriority(string optr, string parentOptr = null)
    {
        int preOptrPriority = 0;
        if (_optrStack.Count > 0)
        {
            preOptrPriority = _GetOperatorPriority(_optrStack.Peek());
        }
        
        int parentOptrPriority = _GetOperatorPriority(parentOptr);
        int curOptrPriority = _GetOperatorPriority(optr) + parentOptrPriority;
        return curOptrPriority > preOptrPriority;
    }

    bool _TryParseSubExp(ref string inExpStr, List<byte> outExp, string token, byte opCode, string parentOptr)
    {
        if (_CheckOptrPriority(token, parentOptr))
        {
            _optrStack.Push(token);
            string nextToken = _NextToken(ref inExpStr);
            _ParseToken(ref inExpStr, outExp, nextToken, token);
            if (opCode != OP_NONE)
            {
                outExp.Add(opCode);
            }
            _optrStack.Pop();
            return true;
        }
        return false;
    }

    bool _TryParseOpExp(ref string inExpStr, List<byte> outExp, string token, byte opCode, string parentOptr)
    {
        if (_CheckOptrPriority(token, parentOptr))
        {
            _optrStack.Push(token);
            string nextToken = _NextToken(ref inExpStr);
            _ParseToken(ref inExpStr, outExp, nextToken);
            if (opCode != OP_NONE)
            {
                outExp.Add(opCode);
            }
            _optrStack.Pop();
            return true;
        }
        return false;
    }

    public bool TryParseStr2Exp(string inExpStr, List<byte> outExp)
    {
        bool ret = false;
        try
        {
            string nextToken = _NextToken(ref inExpStr);
            _ParseToken(ref inExpStr, outExp, nextToken);
            ret = true;
        }
        catch(Exception e)
        {
            Debug.LogErrorFormat("[ExpParser] TryParseStr2Exp fail! inExpStr:{0}\r\nerror:{1}", inExpStr, e.Message);
            if(outExp != null)
            {
                outExp.Clear();
            }
        }

        return ret;
    }

    void _ParseToken(ref string inExpStr, List<byte> outExp, string token, string parentOptr = null)
    {
        if(string.IsNullOrEmpty(token))
        {
            return;
        }
        
        switch(token)
        {
            case "(":
                if (_TryParseSubExp(ref inExpStr, outExp, token, OP_NONE, parentOptr))
                {
                    
                    break;
                }
                inExpStr = token + " " + inExpStr;
                return;
            case ")":
                inExpStr = token + " " + inExpStr;
                return;
            case "+":
                if (_TryParseOpExp(ref inExpStr, outExp, token, OP_ADD, parentOptr))
                {
                    break;
                }
                inExpStr = token + " " + inExpStr;
                return;
            case "-":
                if (_TryParseOpExp(ref inExpStr, outExp, token, OP_ADD, parentOptr))
                {
                    break;
                }
                inExpStr = token + " " + inExpStr;
                return;
            default:
                double ret;
                if(double.TryParse(token, out ret))
                {
                    TValue tv = new TValue();
                    if(token.IndexOf('.') >= 0)
                    {
                        tv.SetFloat((float)ret);
                    }
                    else if(ret >= sbyte.MinValue && ret <= sbyte.MaxValue)
                    {
                        tv.SetByte((sbyte)ret);
                    }
                    else
                    {
                        tv.SetInt((int)ret);
                    }

                    outExp.AddRange(tv.ToByteArray());
                    break;
                }
                throw new Exception("unknow token:" + token);
        }

        string nextToken = _NextToken(ref inExpStr);
        _ParseToken(ref inExpStr, outExp, nextToken, parentOptr);
    }
    */
}
