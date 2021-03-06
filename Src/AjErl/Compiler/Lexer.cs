﻿namespace AjErl.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Lexer
    {
        private static string operators = "=+-*/!";
        private static string[] operators2 = new string[] { "->", "==" };
        private static string[] operators3 = new string[] { "=:=" };
        private static string separators = ".,{}[]()|:;";
        private static string[] separators2 = new string[] { "#{" };
        private TextReader reader;
        private Stack<int> chars = new Stack<int>();
        private Stack<Token> tokens = new Stack<Token>();

        public Lexer(string text)
            : this(new StringReader(text))
        {
        }

        public Lexer(TextReader reader)
        {
            this.reader = reader;
        }

        public Token NextToken()
        {
            if (this.tokens.Count > 0)
                return this.tokens.Pop();

            int ich = this.NextCharSkippingWhiteSpaces();

            if (ich == -1)
                return null;

            char ch = (char)ich;

            if (operators2.Any(op => op[0] == ch))
            {
                int ich2 = this.NextChar();

                if (ich2 >= 0)
                {
                    string op2 = ch.ToString() + ((char)ich2).ToString();

                    int ich3 = this.NextChar();

                    if (ich3 >= 0)
                    {
                        string op3 = op2 + ((char)ich3).ToString();

                        if (operators3.Contains(op3))
                            return new Token(op3, TokenType.Operator);

                        this.PushChar(ich3);
                    }

                    if (operators2.Contains(op2))
                        return new Token(op2, TokenType.Operator);

                    this.PushChar(ich2);
                }
            }

            if (separators2.Any(op => op[0] == ch))
            {
                int ich2 = this.NextChar();

                if (ich2 >= 0)
                {
                    string sep2 = ch.ToString() + ((char)ich2).ToString();

                    if (separators2.Contains(sep2))
                        return new Token(sep2, TokenType.Separator);

                    this.PushChar(ich2);
                }
            }

            if (operators.Contains(ch))
                return new Token(ch.ToString(), TokenType.Operator);
            if (separators.Contains(ch))
                return new Token(ch.ToString(), TokenType.Separator);

            if (char.IsDigit(ch))
                return this.NextInteger(ch);

            if (ch == '"')
                return this.NextString();

            if (IsNameChar(ch))
                return this.NextName(ch);

            throw new ParserException(string.Format("Unexpected '{0}'", ch));
        }

        public void PushToken(Token token)
        {
            this.tokens.Push(token);
        }

        private static bool IsNameChar(char ch)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '@')
                return true;

            return false;
        }

        private Token NextName(char ch)
        {
            string value = ch.ToString();
            TokenType type = char.IsUpper(ch) || ch == '_' ? TokenType.Variable : TokenType.Atom;
            int ich;

            for (ich = this.NextChar(); ich != -1 && IsNameChar((char)ich); ich = this.NextChar())
                value += (char)ich;

            this.PushChar(ich);

            return new Token(value, type);
        }

        private Token NextInteger(char ch)
        {
            string value = ch.ToString();
            int ich;

            for (ich = this.NextChar(); ich != -1 && char.IsDigit((char)ich); ich = this.NextChar())
                value += (char)ich;

            if (ich != -1 && (char)ich == '.')
                return this.NextReal(value);

            this.PushChar(ich);

            return new Token(value, TokenType.Integer);
        }

        private Token NextReal(string integer)
        {
            string value = integer + ".";

            int ich;

            for (ich = this.NextChar(); ich != -1 && char.IsDigit((char)ich); ich = this.NextChar())
                value += (char)ich;

            this.PushChar(ich);

            if (value.EndsWith("."))
            {
                this.PushChar('.');
                return new Token(value.Substring(0, value.Length - 1), TokenType.Integer);
            }

            return new Token(value, TokenType.Real);
        }

        private Token NextString()
        {
            string value = string.Empty;
            int ich;

            for (ich = this.NextChar(); ich != -1 && (char)ich != '"'; ich = this.NextChar())
                value += (char)ich;

            if (ich == -1)
                throw new ParserException("unclosed string");

            return new Token(value, TokenType.String);
        }

        private int NextChar()
        {
            int ich = this.NextSimpleChar();

            if (ich >= 0 && (char)ich == '%')
                for (ich = this.NextSimpleChar(); ich != -1 && (char)ich != '\n';)
                    ich = this.NextSimpleChar();

            return ich;
        }

        private int NextSimpleChar()
        {
            if (this.chars.Count > 0)
                return this.chars.Pop();

            return this.reader.Read();
        }

        private int NextCharSkippingWhiteSpaces()
        {
            int ich = this.NextChar();

            while (ich != -1 && char.IsWhiteSpace((char)ich))
                ich = this.NextChar();

            return ich;
        }

        private void PushChar(int ich)
        {
            this.chars.Push(ich);
        }
    }
}

