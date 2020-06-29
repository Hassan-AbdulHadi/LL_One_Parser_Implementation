## Recursive descent parser with a single look-ahead
### A humble attempt to code a parser!
**This is a small-One week- project for a Compilers course**

**Grammar:**
```
ClassDef --> class id { Stmt* }
Stmt --> ClassDef ;|StructDef ;|OtherDef ;|VoidFunDef ; |e
StructDef --> struct id { Stmt* }
OtherDef --> KeyWords id ArrFun ; // array or function
ArrFun --> [ Term ]|( Pram ) |e
VoidFunDef --> void id ( Pram )
Pram --> Keywords id PramList* |e
PramList --> , pram|e
KeyWords --> int|double|char|bool
Term --> id | digit |e
```
**some notes:**
- Language C# dotnet.
- Lexical analyzer splits by white spaces so you have to separate tokens with white spaces, however, it would be easy to integrate an actual lexical analyzer with this implementation.
- This parser will parse a basic C++ like class, with nested classes, nested structures, class within a structure, int , char, double, bool  data-members, int, double, char, bool arrays as data-members  and void, int, char, double, bool function signatures (for future inline declaration)
An example of what the parser should accept:
```
class myclass { 
void foo ( int a , char b , double c ) ;
struct mystruct 
{
	class myclass2 
	{
		int foo2 ( ) ;
		struct mystruct2
		{
			void foo3 ( int a ) ;
			class myclass3
			{
			} ; 
		} ;
	} ;
	int a ;
	int b [ x ] ;
} ;
}
```
- The Grammar is completely hand-written so it might not be the best grammar you could see.

- If you are intimidated by the ( * ) notation in the grammar see the book (Parsing Techniques: A Practical Guide/chapter 2) or simply google Extended grammar.

- Most of the implementation ideas come from the book: Language Implementation Patterns an excellent book for implementing parsers by Terence Parr. Download from [here](https://github.com/bmihovski/software-development-ebooks-1/blob/master/%5BLanguage%20Implementation%20Patterns%20Create%20Your%20Own%20Domain-Specific%20and%20General%20Programming%20Languages%20(Pragmatic%20Programmers)%20Kindle%20Edition%20by%20Terence%20Parr%20-%202010%5D.pdf).

- I have put tons of comments based on the instructor request.


