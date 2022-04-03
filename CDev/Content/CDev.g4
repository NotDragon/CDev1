grammar CDev;

program: line* EOF;
executer: 'includes''{' (includeStatement)* '}';

line: comment
    | statement 
    | ifStatement 
    | whileStatement 
    | repeatStatement 
    | block 
    | functionDeclaration
    | langDefinition 
    | runStatement
    ;

COMMENTSINGLE: '//'[ \t\r\n]+ -> skip;
COMMENTMULTY: '/*'[ \t\r\n]+ -> skip;
comment: COMMENTSINGLE | COMMENTMULTY '*/';

langDefinition: 'lang' IDENTIFIER langBlock;
langBlock: '{' langLine* '}';
langLine: line | replace | func;

replace: 'replace' IDENTIFIER IDENTIFIER;
func: 'func' IDENTIFIER block;
using: 'using' IDENTIFIER langBlock #usingLang;

runStatement: RUN '<'expresion'>' #runScript;
RUN: 'run' | 'runt';

includeStatement: INCLUDE '<'path'>' ';' #includeCode;
INCLUDE: 'include';

statement: (declration | assignment | swapOp | declration | passStatement | functionCall)';';

passStatement: PASS '(' expresion ')';
PASS: 'pass';

ifStatement: IF '(' expresion ')' block ('else' blockElseIf)?;
IF: 'if';

repeatStatement: REPEAT '(' ('['list']' | expresion) (':' IDENTIFIER ('=' expresion (';' expresion)?)? )?')' block;
exp: expresion;
list: exp(','exp)*;
REPEAT: 'repeat';

blockElseIf: block | ifStatement;

whileStatement: WHILE '(' expresion ')'  block ('else' blockElseIf)?;

WHILE: 'while' | 'until';

assignment: IDENTIFIER '=' expresion #VariableAssignment |  IDENTIFIER'['expresion']' '=' expresion #MultyAssignment | IDENTIFIER '='  block #codeAssignment;
swapOp: IDENTIFIER ':' IDENTIFIER #VariableSwap | IDENTIFIER'['expresion']' ':' IDENTIFIER #MultySwap | IDENTIFIER'['expresion']' ':' IDENTIFIER'['expresion']' #MultyMultySwap;

declration: 'def' IDENTIFIER(':'var)? ('=' expresion)?                                                    #VariableDeclaration
          | 'def' IDENTIFIER(':'var)?'['expresion']'  ('=' '{' expresion(','expresion)* '}')?             #ArrayDeclaration 
          | 'def' IDENTIFIER(':'var)?'[]' ('=' '{' expresion(','expresion)* '}')?                         #ListDeclaration 
          | 'dictionary' IDENTIFIER'['var']'                                                              #DictionaryDeclaration 
          | 'code' IDENTIFIER ('=' block)?                                                                #codeDeclaration
          ;


functionDeclaration: type IDENTIFIER(':' mod)? '(' (var IDENTIFIER (','var IDENTIFIER)*)? ')' block;
mod: modcmd expresion;
modcmd: 'exe' | 'run' | 'loop';

var: 'int' | 'string' | 'float' | 'bool' | 'null' | 'var';
functionCall: IDENTIFIER '(' (expresion (',' expresion)*)? ')';

expresion: const     						#constExpresion
         | multy                            #MultyExpresion
         | IDENTIFIER	                    #identifierExpresion
         | functionCall						#functionCallExpresion
         | '(' expresion ')'				#parenthesizeExpresion
         | '!' expresion					#notExpresion
         | expresion multOp expresion		#multiplicationExpresion
         | expresion addOp expresion		#additionExpresion
         | expresion comperaOp expresion	#comparisonExpresion
         | expresion boolOp expresion		#boolExpresion
         | path                             #pathExpresion
         ; 

multOp: '*' | '/' | '%';
addOp: '+' | '-';
comperaOp: '==' | '!=' | '>' | '<' | '>=' | '<=';
boolOp: BOOL_OP;

BOOL_OP: '&&' | '||';

const:  BOOL | STRING | FLOAT | INTIGER | NULL; 
multy: IDENTIFIER'['expresion']';
code: '%'IDENTIFIER;

type: 'int'| 'void' | 'string' | 'float' | 'bool' | 'var';

INTIGER: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;
STRING: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');
BOOL: 'true' | 'false';
NULL: 'null';

block: '{' ((line* returnStatement?) | code)'}' ;
returnStatement: 'return' expresion;

WS: [ \t\r\n]+ -> skip;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
path:(. | '.' | '\\')*?;