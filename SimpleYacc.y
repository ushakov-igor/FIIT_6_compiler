%{
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
    public Node root;
%}

%output = SimpleYacc.cs

%using ProgramTree

%namespace SimpleParser

%union { 
			public double dVal; 
			public bool bVal;
			public int iVal; 
			public string sVal; 
			public Node nVal;
			public ExprNode eVal;
			public StatementNode stVal;
			public BlockNode blVal;
       }

%token BEGIN END ASSIGN SEMICOLON COMMA FOR PLUS MINUS MULT DIV LPAR RPAR WHILE IF ELSE INPUT PRINT
%token VAR OR AND EQUAL NOTEQUAL LESS GREATER EQGREATER EQLESS GOTO COLON BOOL

%token <iVal> INUM 
%token <dVal> RNUM 
%token <sVal> ID
%token <bVal> BOOL

%type <eVal> expr ident A B C E T F exprlist
%type <stVal> assign statement for while if input print var labelstatement goto
%type <blVal> stlist block progr

%type <eVal> exprlist
%type <varVal> varlist

%%

progr   : stlist { root = $1; }
		;

stlist	: statement 
		{ 
			$$ = new BlockNode($1); 
		}
		| stlist statement 
		{
			$1.Add($2); 
			$$ = $1;
		} 
		;

statement: assign SEMICOLON { $$ = $1; }
		| block { $$ = $1; }
		| for { $$ = $1; }
		| while { $$ = $1; }
		| if { $$ = $1; }
		| input SEMICOLON { $$ = $1; }
		| print SEMICOLON { $$ = $1; }
		| var SEMICOLON { $$ = $1; }
		| labelstatement { $$ = $1; }
		| goto SEMICOLON { $$ = $1; }
		;

ident 	: ID { $$ = new IdNode($1); }
		;
	
assign 	: ident ASSIGN expr { $$ = new AssignNode($1 as IdNode, $3); }
		;

expr	: expr OR A { $$ = new BinOpNode($1, $3, OpType.OR); }
		| A { $$ = $1; }
		;

A		: A AND B { $$ = new BinOpNode($1, $3, OpType.AND); }
		| B { $$ = $1; }
		;

B		: B EQUAL C { $$ = new BinOpNode($1, $3, OpType.EQUAL); }
		| B NOTEQUAL C { $$ = new BinOpNode($1, $3, OpType.NOTEQUAL); }
		| C { $$ = $1; }
		;

C		: C GREATER E { $$ = new BinOpNode($1, $3, OpType.GREATER); }
		| C LESS E { $$ = new BinOpNode($1, $3, OpType.LESS); }
		| C EQGREATER E { $$ = new BinOpNode($1, $3, OpType.EQGREATER); }
		| C EQLESS E { $$ = new BinOpNode($1, $3, OpType.EQLESS); }
		| E { $$ = $1; }
		;

E		: E PLUS T  { $$ = new BinOpNode($1, $3, OpType.PLUS); }
		| E MINUS T { $$ = new BinOpNode($1, $3, OpType.MINUS); }
		| T { $$ = $1; }
		;

T		: T MULT F { $$ = new BinOpNode($1, $3, OpType.MULT); }
		| T DIV F { $$ = new BinOpNode($1, $3, OpType.DIV); }
		| F { $$ = $1; }
		;

F		: ident { $$ = $1 as IdNode; }
		| INUM { $$ = new IntNumNode($1); }
		| LPAR expr RPAR { $$ = $2; }
		| BOOL { $$ = new BoolValNode($1); }
		;

block	: BEGIN stlist END { $$ = $2; }
		;

for		: FOR ident ASSIGN expr COMMA expr statement { $$ = new ForNode($2 as IdNode, $4, $6, $7); }
		;

while	: WHILE expr statement { $$ = new WhileNode($2, $3); }
		;

if		: IF expr statement ELSE statement { $$ = new IfNode($2, $3, $5); }
		| IF expr statement { $$ = new IfNode($2, $3); }
		;

input	: INPUT LPAR ident RPAR { $$ = new InputNode($3 as IdNode); }
		;

exprlist : expr { $$ = new ExprListNode($1); }
		| exprlist COMMA expr { $1.Add($3); $$ = $1; }
		;

print	: PRINT LPAR exprlist RPAR { $$ = new PrintNode($3); }
		;

varlist	: ident { $$ = new VarNode($1 as IdNode); }
		| varlist COMMA ident { $1.Add($3 as IdNode); $$ = $1; }
		;

var		: VAR varlist { $$ = $2; }
		;

goto	: GOTO INUM { $$ = new GoToNode($2); }
		;

labelstatement	: INUM COLON statement { $$ = new LabelStatementNode($1, $3); }
		;
%%