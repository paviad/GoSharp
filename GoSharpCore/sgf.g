grammar sgf;

Collection 
	:	GameTree+
	;

GameTree 
	:	'(' Sequence GameTree? ')'
	;

Sequence 
	:	Node+
	;
	
Node	:	';' Property?
	;

Property 
	:	PropIdent PropValue+
	;


PropValue 
	:	'[' CValueType? ']'
	;
PropIdent 
	:	USTRING
	;
	
CValueType 
	:	(ValueType | Compose)
	;

ValueType 
	:	Text
	;
	
Number 	:	SINT
	;
	
Real 	:	Number ('.' INT)?
	;

Text 	:	CTEXT
	;
	
Point 	:	('a'..'z')('a'..'z')
	;
	
Compose : ValueType ':' ValueType
	;

ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;
    
    
SINT 	:	('+'|'-')? INT
	;

INT :	'0'..'9'+
    ;

USTRING :	('A'..'Z')+
	;

CTEXT
    :  ( ESC_SEQ2 | ~('\\'|']'|':') )+
    ;

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

fragment
ESC_SEQ2
    :   '\\' .
    ;
