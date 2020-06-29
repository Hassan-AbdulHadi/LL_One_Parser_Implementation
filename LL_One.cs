using System;
using System.Text.RegularExpressions;

namespace LLOneParser
{

    class MainClass
    {
        //LL recursive parser with a single-lookahead implementation

        //Grammar:
        //ClassDef--> class id { Stmt* }
        //Stmt-->ClassDef ;|StructDef ;|OtherDef ;|VoidFunDef ; |e
        //StructDef-->struct id { Stmt* }
        //OtherDeff--> KeyWords id ArrFun ; // array or function
        //ArrFun--> [ Term ]|( Pram ) |e
        //VoidFunDef--> void id ( Pram )
        //Pram--> Keywords id PramList* |e 
        //PramList--> , pram|e
        //KeyWords--> int|double|char|bool
        //Term-->id | digit |e

        //First & Follow functions that we are going to need:
        //first(ClassDef)={class}
        //first(Stmt)={class,struct,int,double,char,bool,void}
        //follow(Stmt)={ } }
        //first(StructDef)={struct}
        //first(OtherDef)={int,double,char,bool}
        //first(Pram)={int,double,char,bool}
        //follow(pram)={ ) }
        //first(PramList)={ , }
        //follow(pramList)=follow(pram)={ ) }
        //follow(ArrFun)={ ; }



       static void Main(string[] args)
       {
            string input = "class myclass { void foo ( ) ; int a ; double b [ ] ; struct mystruct { int a ; int b ; void foo2 ( int a ) ; class myclass2 { int g [ 2 ] ; double foo3 ( char b , char c ) ; } ; } ; } ";
            try
            {
                Parser myparser = new Parser(LexicalAnalyzer.GetLexemes(input));
                Console.WriteLine(myparser.Parse());
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }

    /// <summary>
    /// The parser Class consists of multiple private functions 
    /// with one public function named (Parse) which will invoke the other functions.
    /// call the parse function to do the actual parsing.
    /// </summary>
    class Parser
    {
        string[] buffer;
        string look_ahead;
        int buffer_pointer;
        int buffer_length;

        public Parser(string[] input)
        {
            this.buffer = input;
            this.buffer_pointer = 0;
            this.buffer_length = buffer.Length;
            this.look_ahead = buffer[buffer_pointer + 1];
        }
        public bool Parse()
        {
            if (ClassDef() && look_ahead == "$")
                return true;
            else
                throw new Exception("Syntax Error!");
        }

        private bool ClassDef()
        {
            //ClassDef-- > class id { Stmt* }

            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "class"))
                if(buffer_pointer < buffer_length && IsValidId(buffer[buffer_pointer]))
                {
                    Consume();//IsValidId doesn't Consume the current token
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "{"))
                    {
                        if (look_ahead == "}")
                        {
                            //if the lookahead token is "}" which is the follow of Stmt 
                            //that means we have an empty class -which is a valid case since Stmt also preduces null-
                            //move to the next token(Consume) and return true to the caller.
                            Consume();
                            return true;
                        }
                        while(look_ahead == "class" | look_ahead =="struct"|look_ahead == "int" | look_ahead == "double"|look_ahead == "char" | look_ahead == "bool" | look_ahead == "void")
                        {
                            //while the lookahead token is the first of Stmt-which means another Stmt is coming-
                            if (Stmt())
                            {
                                if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "}"))
                                    return true;
                                else
                                    continue;
                            }
                            else
                                return false;
                        }
                    }
                }
            return false;
        }

        private bool Stmt()
        {
            //Stmt-- > ClassDef;| StructDef;| OtherDef;| VoidFunDef; | e

            //we are Making parsing decision-prediction here based on the first of ClassDef,StructDef OtherDef
            //on which rule to invoke-expand, by checking against the lookahead token
            if (look_ahead == "class")
            {
                if (ClassDef())
                {
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], ";"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;

            }
            

            if (look_ahead == "struct")
            { 
                if (StructDef())
                {
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], ";"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }


            if (look_ahead == "int" | look_ahead == "double" | look_ahead == "char" | look_ahead == "bool")
            { 
                if( OtherDef())
                {
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], ";"))
                        return true;
                    else
                        return false;
                }
                else
                     return false;
            }

            if (look_ahead =="void")
            { 
                if (VoidFunDef())
                { 
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], ";"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            return false;
        }
        private bool StructDef()
        {
            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "struct"))
                if (buffer_pointer < buffer_length && IsValidId(buffer[buffer_pointer]))
                {
                    Consume();
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "{"))
                    {
                        if (look_ahead == "}")
                        {
                            Consume();
                            return true;
                        }

                        while (look_ahead == "class" | look_ahead == "struct" | look_ahead == "int" | look_ahead == "double" | look_ahead == "char" | look_ahead == "bool" | look_ahead == "void")
                        {
                            if (Stmt())
                            {
                                if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "}"))
                                    return true;
                                else
                                    continue;
                            }
                            else
                                return false;

                        }
                    }
                }
            return false;
        }

        private bool OtherDef()
        {
            //OtherDef--> KeyWords id ArrFun ;
            if (Keywords())
                if (buffer_pointer < buffer_length && IsValidId(buffer[buffer_pointer]))
                {
                    Consume(); //a workaround, the IsValidId-function does not Consume the token 

                    //since ArrayFun can produce null, here we are deciding -based on its follow-
                    //whether to expand it or not
                    if (look_ahead == ";")
                        return true;

                    else
                        return ArrFun();
                }
                    

            return false; 
        }
        private bool ArrFun()
        {
            //ArrFun-- > [Term] | (Pram) | e

            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "["))
                if (look_ahead == "]")//Term preduces null 
                {
                    Consume();
                    return true;
                }
                else
                { 
                    if (buffer_pointer < buffer_length && Term())
                        if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "]"))
                            return true;
                }

            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "("))
                if (look_ahead == ")")//Pram preduces null 
                {
                    Consume();
                    return true;
                }
                else
                    return Pram();

            return false;
        }
        public bool VoidFunDef()
        {
            //VoidFunDef--> void id ( Pram )

            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "void"))
                if (buffer_pointer < buffer_length && IsValidId(buffer[buffer_pointer]))
                {
                    Consume();
                    if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], "("))
                        if (look_ahead == ")")
                        {
                            Consume();
                            return true;
                        }
                    if (Pram())
                            return true;
                }
               return false;
        }

        private bool Pram()
        {
            //Pram--> Keywords id PramList* |e 

            if (buffer_pointer < buffer_length && Keywords())
                if (buffer_pointer < buffer_length && IsValidId(buffer[buffer_pointer]))
                {
                    Consume();
                    if(look_ahead==")")
                    {
                        Consume();
                        return true;
                    }
                    else
                        if (PramList())
                            return true;
                }
                return false;
        }

        private bool PramList()
        {
            //PramList--> , pram|e

            if (buffer_pointer < buffer_length && Match(buffer[buffer_pointer], ","))
                return Pram();

            return false;
        }

        private bool Keywords()
        {
            string[] Keywords = { "int", "double", "char", "bool" };
            foreach (string keyword in Keywords)
            {
                if (Match(buffer[buffer_pointer], keyword))
                    return true;
            }
            return false;
        }

        private bool Term()
        {
            if(IsValidId(buffer[buffer_pointer]) | IsInteger(buffer[buffer_pointer]))
            {
                Consume();
                return true;
            }
            return false;
        }

        ///////////////// Supporting functions /////////////////
    
        private bool Match(string input, string target)
        {
            if (input == target)
            {
                Consume();
                return true;
            }
            return false;
        }

        private void Consume()
        {
            if (buffer.Length - 1 == buffer_pointer)//change the lookahead
                look_ahead = "$";
                //if we reached the end of the buffer, set the lookahead token to $
            else
                look_ahead = buffer[buffer_pointer + 1];

            Console.WriteLine(look_ahead);
            buffer_pointer++; //and Consume the token 
        }


        private bool IsValidId(string input)//helper function to check if the given token is a valid ID
        {
            return Regex.IsMatch(input, @"^([a-z]|[A-Z]|_)([a-z]|[A-Z]|[0-9]|_)*$");
        }

        static bool IsInteger(string input)//helper function to check if the given input-string is an integer
        {
            if (input.Length == 0)
                return false;
            foreach (char c in input)
            {

                if (char.IsDigit(c))
                    continue;
                else
                    return false;
            }
            return true;
        }

    }

    class LexicalAnalyzer
    {
        //Lexical analyzer siplts by white spaces for know 
        public static string[] GetLexemes(string input)
        {
            char[] separators = { ' ', '\r' };
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
