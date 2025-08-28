/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
词法分析器，表达式字符流->表达式token流
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExpParseV
{
    public class ExpLexer
    {
        public const string TAG = "ExpLexer";
        /// <summary>
        /// 空字符
        /// </summary>
        public const char NULL_CHAR = '\0';

        //词法分析器状态
        public const int LEX_STATE_NONE = 0;
        public const int LEX_STATE_START = 1;
        public const int LEX_STATE_INT = 2;
        public const int LEX_STATE_FLT = 3;
        public const int LEX_STATE_STR = 4;
        public const int LEX_STATE_IDENT = 5;
        public const int LEX_STATE_DELIM = 6;
        public const int LEX_STATE_OP = 7;
        
        /// <summary>
        /// token类型
        /// </summary>
        public enum TokenType
        {
            TK_EOS = -1,                    //结束
            TK_INT = 1,                     //整数
            TK_FLT,                         //浮点数
            TK_STR,                         //字符串
            TK_IDENT,                       //标识符
            TK_DOT,                         //点号

            TK_DELIM_START = 20,            //分隔符开始
            TK_DELIM_CMA,                   //,
            TK_DELIM_SCM,                   //;
            TK_DELIM_OPEN_S_BRACE,          //(
            TK_DELIM_CLOSE_S_BRACE,         //)
            TK_DELIM_OPEN_M_BRACE,          //[
            TK_DELIM_CLOSE_M_BRACE,         //]
            TK_DELIM_OPEN_L_BRACE,          //{
            TK_DELIM_CLOSE_L_BRACE,         //}
            TK_DELIM_END,                   //分隔符结束

            TK_OP_START = 40,               //运算符开始
            TK_OP_1_START,                  //单符号运算符开始
            TK_OP_ADD,                      //+
            TK_OP_SUB,                      //-
            TK_OP_MUL,                      //*
            TK_OP_DIV,                      ///
            TK_OP_MOD,                      //%
            TK_OP_BAND,                     //&
            TK_OP_BOR,                      //|
            TK_OP_BNOT,                     //~
            TK_OP_POW,                      //^
            TK_OP_NOT,                      //!
            TK_OP_ASSIGN,                   //=
            TK_OP_LT,                       //<
            TK_OP_GT,                       //>
            TK_OP_1_END,                    //单符号运算符结束

            TK_OP_2_START = 60,             //双符号运算符开始
            TK_OP_INC,                      //++
            TK_OP_ADD_ASSIGN,               //+=
            TK_OP_DEC,                      //--
            TK_OP_SUB_ASSIGN,               //-=
            TK_OP_MUL_ASSIGN,               //*=
            TK_OP_DIV_ASSIGN,               ///=
            TK_OP_MOD_ASSIGN,               //%=
            TK_OP_AND,                      //&&
            TK_OP_BAND_ASSIGN,              //&=
            TK_OP_OR,                       //||
            TK_OP_BOR_ASSIGN,               //|=
            TK_OP_BXOR_ASSIGN,              //^=
            TK_OP_NEQ,                      //!=
            TK_OP_EQ,                       //==
            TK_OP_BSHL,                     //<<
            TK_OP_LE,                       //<=
            TK_OP_BSHR,                     //>>
            TK_OP_GE,                       //>=
            TK_OP_2_END,                    //双符号运算符结束

            TK_OP_3_START = 90,             //三符号运算符开始
            TK_OP_3_END,                    //三符号运算符结束
            TK_OP_END,                      //运算符结束

            TK_RSRVD_START = 100,           //保留字开始
            TK_RSRVD_FALSE,                 //false
            TK_RSRVD_TRUE,                  //true
            TK_RSRVD_END,                   //保留字结束
        }

        /// <summary>
        /// 词法分析器状态
        /// </summary>
        public class LexState
        {
            public int state;
            public char curChar;
            public int curCharPos;
            public int nSrcUnRead;
            public int nSrcTotal;
            public int lexStartCharPos;
            public int lexEndCharPos;
            public TokenType tokenType = TokenType.TK_EOS;

            public string sourceCode = string.Empty;
            string _tokenStr = string.Empty;

            public char tokenChar
            {
                get
                {
                    if (lexStartCharPos < nSrcTotal)
                    {
                        return sourceCode[lexStartCharPos];
                    }
                    return NULL_CHAR;
                }
            }

            public string tokenStr
            {
                get
                {
                    if (!string.IsNullOrEmpty(_tokenStr))
                    {
                        return _tokenStr;
                    }

                    sBuff.Clear();
                    for (int i = lexStartCharPos; i < lexEndCharPos; i++)
                    {
                        sBuff.Append(sourceCode[i]);
                    }
                    _tokenStr = sBuff.ToString();
                    return _tokenStr;
                }
            }

            public void CopyTo(LexState dst)
            {
                dst.state = state;
                dst.curChar = curChar;
                dst.curCharPos = curCharPos;
                dst.nSrcUnRead = nSrcUnRead;
                dst.nSrcTotal = nSrcTotal;
                dst.lexStartCharPos = lexStartCharPos;
                dst.lexEndCharPos = lexEndCharPos;
                dst.tokenType = tokenType;
                dst.sourceCode = sourceCode;
                dst._tokenStr = _tokenStr;
            }

            public void ResetForNextToken()
            {
                state = LEX_STATE_START;
                tokenType = TokenType.TK_EOS;
                _tokenStr = string.Empty;
            }

            public void Reset()
            {
                state = LEX_STATE_NONE;
                curChar = NULL_CHAR;
                curCharPos = -1;
                nSrcUnRead = 0;
                nSrcTotal = 0;
                lexStartCharPos = 0;
                lexEndCharPos = 0;
                tokenType = TokenType.TK_EOS;
                sourceCode = string.Empty;
                _tokenStr = string.Empty;
            }
        }

        struct CharState
        {
            public char c;
            public TokenType tokenType;

            public bool IsNull
            {
                get
                {
                    return c == NULL_CHAR;
                }
            }

            public CharState(char c, TokenType tokenType)
            {
                this.c = c;
                this.tokenType = tokenType;
            }
        }

        struct CharGroupState
        {
            public CharState charState;
            public int subStateStartIdx;
            public int subStateCount;

            public bool IsNull
            {
                get
                {
                    return charState.c == NULL_CHAR;
                }
            }

            public CharGroupState(char c, TokenType tokenType, int subStateStartIdx, int subStateCount)
            {
                charState = new CharState(c, tokenType);
                this.subStateStartIdx = subStateStartIdx;
                this.subStateCount = subStateCount;
            }
        }
        
        static StringBuilder sBuff = new StringBuilder();
        public static ExpLogger sLogger = new ExpLogger(TAG);
        static readonly CharState sNullCharState = new CharState(NULL_CHAR, TokenType.TK_EOS);
        static readonly CharGroupState sNullCharGroupState = new CharGroupState(NULL_CHAR, TokenType.TK_EOS, 0, 0);
        static readonly CharState[] sDelimStates = {
            new CharState(',', TokenType.TK_DELIM_CMA),
            new CharState(';', TokenType.TK_DELIM_SCM),
            new CharState('(', TokenType.TK_DELIM_OPEN_S_BRACE),
            new CharState(')', TokenType.TK_DELIM_CLOSE_S_BRACE),
            new CharState('[', TokenType.TK_DELIM_OPEN_M_BRACE),
            new CharState(']', TokenType.TK_DELIM_CLOSE_M_BRACE),
            new CharState('{', TokenType.TK_DELIM_OPEN_L_BRACE),
            new CharState('}', TokenType.TK_DELIM_CLOSE_L_BRACE),
        };

        static readonly CharGroupState[][] sOpGroupStates = {
            new CharGroupState[]{
                new CharGroupState('+', TokenType.TK_OP_ADD, 0, 2),
                new CharGroupState('-', TokenType.TK_OP_SUB, 2, 2),
                new CharGroupState('*', TokenType.TK_OP_MUL, 4, 1),
                new CharGroupState('/', TokenType.TK_OP_DIV, 5, 1),
                new CharGroupState('%', TokenType.TK_OP_MOD, 6, 1),
                new CharGroupState('&', TokenType.TK_OP_BAND, 7, 2),
                new CharGroupState('|', TokenType.TK_OP_BOR, 9, 2),
                new CharGroupState('~', TokenType.TK_OP_BNOT, 0, 0),
                new CharGroupState('^', TokenType.TK_OP_POW, 11, 1),
                new CharGroupState('!', TokenType.TK_OP_NOT, 12, 1),
                new CharGroupState('=', TokenType.TK_OP_ASSIGN, 13, 1),
                new CharGroupState('<', TokenType.TK_OP_LT, 14, 2),
                new CharGroupState('>', TokenType.TK_OP_GT, 16, 2),
            },
            new CharGroupState[]{
                new CharGroupState('+', TokenType.TK_OP_INC, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_ADD_ASSIGN, 0, 0),
                new CharGroupState('-', TokenType.TK_OP_DEC, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_SUB_ASSIGN, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_MUL_ASSIGN, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_DIV_ASSIGN, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_MOD_ASSIGN, 0, 0),
                new CharGroupState('&', TokenType.TK_OP_AND, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_BAND_ASSIGN, 0, 0),
                new CharGroupState('|', TokenType.TK_OP_OR, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_BOR_ASSIGN, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_BXOR_ASSIGN, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_NEQ, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_EQ, 0, 0),
                new CharGroupState('<', TokenType.TK_OP_BSHL, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_LE, 0, 0),
                new CharGroupState('>', TokenType.TK_OP_BSHR, 0, 0),
                new CharGroupState('=', TokenType.TK_OP_GE, 0, 0),
            },
            new CharGroupState[]{

            },
        };

        LexState _backupLexState0 = new LexState();
        LexState _backupLexState1 = new LexState();
        LexState _preLexState = new LexState();
        LexState _curLexState = new LexState();
        
        public LexState curLexState
        {
            get
            {
                return _curLexState;
            }
        }

        public TokenType tokenType
        {
            get
            {
                return _curLexState.tokenType;
            }
        }

        public char tokenChar
        {
            get
            {
                return _curLexState.tokenChar;
            }
        }

        public string tokenStr
        {
            get
            {
                return _curLexState.tokenStr;
            }
        }

        #region 字符处理部分
        char _NextChar()
        {
            char c = NULL_CHAR;
            if (_curLexState.nSrcUnRead <= 0)
            {
                _curLexState.curChar = c;
                return c;
            }

            _curLexState.nSrcUnRead--;
            _curLexState.curCharPos++;
            c = _curLexState.sourceCode[_curLexState.curCharPos];
            _curLexState.curChar = c;
            return c;
        }

        char _SaveAndNextChar()
        {
            _curLexState.lexEndCharPos++;
            return _NextChar();
        }

        public char LookaheadChar(int lookaheadStep = 1)
        {
            int charPos = _curLexState.curCharPos + lookaheadStep;
            if (charPos < 0 || charPos >= _curLexState.nSrcTotal)
            {
                return NULL_CHAR;
            }

            char c = _curLexState.curChar;
            _BackupCurLexState(_backupLexState0);
            for (int i = 0; i < lookaheadStep; i++)
            {
                while (true)
                {
                    c = _NextChar();
                    if (!_IsCharWhitespace(c))
                    {
                        break;
                    }
                }
            }
            _RecoveCurLexState(_backupLexState0);

            return c;
        }

        void _RollbackCurCharPos()
        {
            if (_curLexState.curChar != NULL_CHAR)
            {
                _curLexState.nSrcUnRead++;
                _curLexState.curCharPos--;
            }
        }

        CharState _TryGetCharState(CharState[] charStates, char c)
        {
            for (int i = 0; i < charStates.Length; i++)
            {
                if (charStates[i].c == c)
                {
                    return charStates[i];
                }
            }

            return sNullCharState;
        }

        CharGroupState _TryGetCharGroupState(CharGroupState[][] charGroupStates, char c, int groupIdx, int subStateStartIdx = 0, int subStateCount = -1)
        {
            int groupCount = charGroupStates.Length;
            if (groupIdx < 0 || groupIdx >= groupCount)
            {
                return sNullCharGroupState;
            }

            CharGroupState[] subGroupStates = charGroupStates[groupIdx];
            int subGroupMaxIdx = subGroupStates.Length - 1;
            int subStateEndIdx = 0;
            if (subStateCount < 0)
            {
                subStateEndIdx = subGroupMaxIdx;
            }
            else
            {
                subStateEndIdx = subStateStartIdx + subStateCount - 1;
                if (subStateEndIdx > subGroupMaxIdx)
                {
                    subStateEndIdx = subGroupMaxIdx;
                }
            }

            for (int i = subStateStartIdx; i <= subStateEndIdx; i++)
            {
                if (subGroupStates[i].charState.c == c)
                {
                    return subGroupStates[i];
                }
            }

            return sNullCharGroupState;
        }

        bool _IsCharWhitespace(char c)
        {
            if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\v' || c == '\f')
            {
                return true;
            }
            return false;
        }

        bool _IsCharNumber(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return true;
            }
            return false;
        }

        bool _IsCharIdentifier(char c)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
            {
                return true;
            }
            return false;
        }

        bool _IsCharDelim(char c)
        {
            return !_TryGetCharState(sDelimStates, c).IsNull;
        }

        bool _IsCharOp(char c)
        {
            return !_TryGetCharGroupState(sOpGroupStates, c, 0).IsNull;
        }
        #endregion
        
        void _BackupCurLexState(LexState backupLexState)
        {
            Debug.AssertFormat(backupLexState.state == LEX_STATE_NONE, "[{0}] BackupCurLexState fail! backupLexState.state != LEX_STATE_NONE", TAG);
            _curLexState.CopyTo(backupLexState);
        }

        void _RecoveCurLexState(LexState backupLexState)
        {
            Debug.AssertFormat(backupLexState.state != LEX_STATE_NONE, "[{0}] RecoveCurLexState fail! backupLexState.state == LEX_STATE_NONE", TAG);
            backupLexState.CopyTo(_curLexState);
            backupLexState.state = LEX_STATE_NONE;
        }
        
        public void LexException(string msg, params object[] args)
        {
            string tokenStr = string.Empty;
            switch (tokenType)
            {
                case TokenType.TK_EOS:
                    tokenStr = "EOS";
                    break;
                default:
                    tokenStr = this.tokenStr;
                    break;
            }
            if (!string.IsNullOrEmpty(tokenStr))
            {
                msg = string.Format("{0} near {1}", msg, tokenStr);
            }
            sLogger.LogExceptionFormat(msg, args);
        }

        TokenType _TokenStr2TokenType(string tokenStr)
        {
            if(string.IsNullOrEmpty(tokenStr))
            {
                return TokenType.TK_EOS;
            }

            tokenStr = tokenStr.ToLower();
            switch (tokenStr)
            {
                case "true":
                    return TokenType.TK_RSRVD_TRUE;
                case "false":
                    return TokenType.TK_RSRVD_FALSE;
            }

            return TokenType.TK_EOS;
        }

        public TokenType NextToken()
        {
            _curLexState.CopyTo(_preLexState);
            _curLexState.ResetForNextToken();
            bool isAddChar = false;
            bool isLexDone = false;
            bool isRollbackCurCharPos = true;
            int curOpGroupIdx = 0;
            CharGroupState curOpGroupState = sNullCharGroupState;
            TokenType tokenType = TokenType.TK_EOS;

            while (true)
            {
                _NextChar();

                isAddChar = true;

                switch (_curLexState.state)
                {
                    case LEX_STATE_START:
                        _curLexState.lexStartCharPos = _curLexState.curCharPos;
                        _curLexState.lexEndCharPos = _curLexState.lexStartCharPos;

                        if (_IsCharWhitespace(_curLexState.curChar))
                        {
                            isAddChar = false;
                        }
                        else if (_IsCharNumber(_curLexState.curChar))
                        {
                            _curLexState.state = LEX_STATE_INT;
                        }
                        else if (_curLexState.curChar == '.')
                        {
                            _SaveAndNextChar();
                            if (_IsCharNumber(_curLexState.curChar))
                            {
                                _curLexState.state = LEX_STATE_FLT;
                            }
                            else if (_IsCharIdentifier(_curLexState.curChar))
                            {
                                isAddChar = false;
                                isLexDone = true;
                                tokenType = TokenType.TK_DOT;
                            }
                            else
                            {
                                sLogger.LogExceptionFormat("unexpected char {0} after {1}", + _curLexState.curChar, tokenStr);
                            }
                        }
                        else if (_curLexState.curChar == '"')
                        {
                            _curLexState.state = LEX_STATE_STR;
                            isAddChar = false;
                        }
                        else if (_IsCharIdentifier(_curLexState.curChar))
                        {
                            _curLexState.state = LEX_STATE_IDENT;
                        }
                        else if (_IsCharDelim(_curLexState.curChar))
                        {
                            _curLexState.state = LEX_STATE_DELIM;
                        }
                        else if (_IsCharOp(_curLexState.curChar))
                        {
                            _curLexState.state = LEX_STATE_OP;
                            curOpGroupState = _TryGetCharGroupState(sOpGroupStates, _curLexState.curChar, curOpGroupIdx);
                        }
                        else if (_curLexState.curChar == NULL_CHAR)
                        {
                            isAddChar = false;
                            isLexDone = true;
                        }

                        if (!isAddChar)
                        {
                            _curLexState.lexStartCharPos = _curLexState.curCharPos + 1;
                            _curLexState.lexEndCharPos = _curLexState.lexStartCharPos;
                        }

                        break;
                    case LEX_STATE_INT:
                        if (_curLexState.curChar == '.')
                        {
                            _curLexState.state = LEX_STATE_FLT;
                        }
                        else if (!_IsCharNumber(_curLexState.curChar))
                        {
                            isAddChar = false;
                            isLexDone = true;
                        }
                        break;
                    case LEX_STATE_FLT:
                        if (!_IsCharNumber(_curLexState.curChar))
                        {
                            isAddChar = false;
                            isLexDone = true;
                        }
                        break;
                    case LEX_STATE_STR:
                        if (_curLexState.curChar == '"')
                        {
                            isAddChar = false;
                            isLexDone = true;
                            isRollbackCurCharPos = false;
                        }
                        else if (_curLexState.curChar == NULL_CHAR)
                        {
                            sLogger.LogException("expected char \"");
                        }
                        break;
                    case LEX_STATE_IDENT:
                        if (!_IsCharIdentifier(_curLexState.curChar))
                        {
                            isAddChar = false;
                            isLexDone = true;
                        }
                        break;
                    case LEX_STATE_DELIM:
                        //分隔符只有一个字符
                        isAddChar = false;
                        isLexDone = true;
                        break;
                    case LEX_STATE_OP:
                        if (_IsCharOp(_curLexState.curChar))
                        {
                            if (curOpGroupState.subStateCount > 0)
                            {
                                curOpGroupIdx++;
                                CharGroupState subOpGroupState = _TryGetCharGroupState(sOpGroupStates, _curLexState.curChar, curOpGroupIdx, curOpGroupState.subStateStartIdx, curOpGroupState.subStateCount);
                                if (subOpGroupState.IsNull)
                                {
                                    isAddChar = false;
                                    isLexDone = true;
                                }
                                else
                                {
                                    curOpGroupState = subOpGroupState;
                                }
                            }
                            else
                            {
                                sLogger.LogExceptionFormat("unexpected char {0} after {1}", _curLexState.curChar, tokenStr);
                            }
                        }
                        else
                        {
                            isAddChar = false;
                            isLexDone = true;
                        }
                        break;
                }

                if (isAddChar)
                {
                    _curLexState.lexEndCharPos++;
                }

                if (isLexDone)
                {
                    if (isRollbackCurCharPos)
                    {
                        _RollbackCurCharPos();
                    }
                    break;
                }
            }

            switch (_curLexState.state)
            {
                case LEX_STATE_INT:
                    tokenType = TokenType.TK_INT;
                    break;
                case LEX_STATE_FLT:
                    tokenType = TokenType.TK_FLT;
                    break;
                case LEX_STATE_STR:
                    tokenType = TokenType.TK_STR;
                    break;
                case LEX_STATE_IDENT:
                    tokenType = _TokenStr2TokenType(tokenStr);
                    if(tokenType == TokenType.TK_EOS)
                    {
                        tokenType = TokenType.TK_IDENT;
                    }
                    break;
                case LEX_STATE_DELIM:
                    {
                        CharState charState = _TryGetCharState(sDelimStates, _curLexState.sourceCode[_curLexState.lexStartCharPos]);
                        tokenType = charState.tokenType;
                    }
                    break;
                case LEX_STATE_OP:
                    tokenType = curOpGroupState.charState.tokenType;
                    break;
            }

            _curLexState.tokenType = tokenType;
            return tokenType;
        }
        
        public bool CheckTokenChar(char reqTokenChar, bool riseError = true)
        {
            if (tokenChar != reqTokenChar)
            {
                if(riseError)
                {
                    LexException("expected symbol {0}", reqTokenChar);
                }
                return false;
            }
            return true;
        }

        public bool CheckTokenStr(string reqTokenStr, bool riseError = true)
        {
            if (tokenStr != reqTokenStr)
            {
                if(riseError)
                {
                    LexException("expected symbol {0}", reqTokenStr);
                }
                return false;
            }
            return true;
        }

        public TokenType CheckAndNextToken(char reqLexChar)
        {
            CheckTokenChar(reqLexChar);
            TokenType nextTokenType = NextToken();
            return nextTokenType;
        }
        
        public TokenType CheckAndNextToken(string reqLexStr)
        {
            CheckTokenStr(reqLexStr);
            TokenType nextTokenType = NextToken();
            return nextTokenType;
        }

        public void CheckEndToken()
        {
            char reqTokenChar = NULL_CHAR;
            if (tokenChar != reqTokenChar)
            {
                LexException("unexpected symbol");
            }
        }

        public TokenType LookaheadToken(int lookaheadStep = 1)
        {
            TokenType tokenType = _curLexState.tokenType;
            _BackupCurLexState(_backupLexState1);
            for (int i = 0; i < lookaheadStep; i++)
            {
                tokenType = NextToken();
            }
            _RecoveCurLexState(_backupLexState1);

            return tokenType;
        }

        public TokenType LookaheadToken(out string lex, int lookaheadStep = 1)
        {
            TokenType tokenType = _curLexState.tokenType;
            _BackupCurLexState(_backupLexState1);
            for (int i = 0; i < lookaheadStep; i++)
            {
                tokenType = NextToken();
            }
            lex = tokenStr;
            _RecoveCurLexState(_backupLexState1);

            return tokenType;
        }

        public bool IsOpToken(TokenType tokenType)
        {
            return tokenType >= TokenType.TK_OP_START && tokenType <= TokenType.TK_OP_END;
        }
        
        public void RollbackCurLexState()
        {
            _preLexState.CopyTo(_curLexState);
        }

        /// <summary>
        /// 设置表达式源码
        /// </summary>
        /// <param name="sourceCode"></param>
        public void SetSourceCode(string sourceCode)
        {
            _curLexState.Reset();
            _curLexState.sourceCode = sourceCode;
            if (!string.IsNullOrEmpty(sourceCode))
            {
                _curLexState.nSrcTotal = sourceCode.Length;
                _curLexState.nSrcUnRead = _curLexState.nSrcTotal;
            }
        }
    }
}