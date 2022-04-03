using System.Data;
using System.Diagnostics;
using CDev.Content;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CDev;
public class CDevVisitor : CDevBaseVisitor<object?>
{
	public Dictionary<string, object?> Variables { get; set; } = new();
	public Dictionary<string, CDevParser.FunctionDeclarationContext> Functions { get; } = new();
	
	public CDevVisitor()
	{
		Contractor();
	}

	private void Contractor()
	{

		//console
		Variables["Write"] = new Func<object?[], object?>(Write);
		Variables["Read"] = new Func<object?[], object?>(Read);

	}

	~CDevVisitor()
	{
		Variables.Clear();
		Functions.Clear();
	}
	
	#region run

	public override object? VisitPath(CDevParser.PathContext context)
	{
		return context.GetText();
	}

	private static CDevParser.RunScriptContext? _temp;

	private static void Runt()
	{
		//creates visitor to visit
		var visitor = new CDevVisitor();

		//variables
		var place = visitor.Visit(_temp?.expresion())?.ToString();
		string content = String.Empty;
		
		//makes sure path is valid
		if(place is not null)
			content = File.ReadAllText(@place);
		
		//runs the script
		var inputStream = new AntlrInputStream(content);
		var importLexer = new CDevLexer(inputStream);

		CommonTokenStream importcommonTokenStream = new CommonTokenStream(importLexer);

		var importParser = new CDevParser(importcommonTokenStream);
		var importContext = importParser.program();
		
		visitor.Visit(importContext);
	}
	public override object? VisitRunScript(CDevParser.RunScriptContext context)
	{
		//used for Runt()
		_temp = context;
		//variables
		var place = Visit(context.expresion())?.ToString();
		string content = String.Empty;
		
		//makes sure path is valid
		if(place is not null)
			content = File.ReadAllText(@place);

		//check to execute runt or run
		if (context.RUN().GetText() == "run")
		{
			//executes the script
			var inputStream = new AntlrInputStream(content);
			var importLexer = new CDevLexer(inputStream);

			CommonTokenStream importcommonTokenStream = new CommonTokenStream(importLexer);

			var importParser = new CDevParser(importcommonTokenStream);
			var importContext = importParser.program();

			Visit(importContext);
		}else if (context.RUN().GetText() == "runt")
		{
			Thread importThread = new(Runt);
			importThread.Name = "importThread";
			importThread.Start();
		}
		
		return null;
	}

	#endregion
	

	#region Execute

	public override object? VisitIncludeCode(CDevParser.IncludeCodeContext context)
	{
		var path = Convert.ToString(Visit(context.path()));
		
		if (path is not null)
		{
			Process process = Process.Start("C:\\Coding\\CDev\\lib\\cs\\"+path+"\\"+path+"\\bin\\Debug\\net6.0\\"+path+".exe");
			Process tempProc = Process.GetProcessById(process.Id);
			tempProc.WaitForExit();
			Variables = Variables;
		}
		
		return null;
	}

	#endregion


	#region Constant Functions

	//used to remove variables//
	
	private object? Write(object?[] args)
	{
		if(args.Contains("$sbeep"))
			Console.Beep();
		if(args.Contains("$cRed"))
			Console.ForegroundColor = ConsoleColor.Red;
		if(args.Contains("$cBlack"))
			Console.ForegroundColor = ConsoleColor.Black;
		if(args.Contains("$cBlue"))
			Console.ForegroundColor = ConsoleColor.Blue;
		if(args.Contains("$cCyan"))
			Console.ForegroundColor = ConsoleColor.Cyan;
		if(args.Contains("$cGray"))
			Console.ForegroundColor = ConsoleColor.Gray;
		if(args.Contains("$cGreen"))
			Console.ForegroundColor = ConsoleColor.Green;
		if(args.Contains("$cMagenta"))
			Console.ForegroundColor = ConsoleColor.Magenta;
		if(args.Contains("$cYellow"))
			Console.ForegroundColor = ConsoleColor.Yellow;
		if(args.Contains("$cDarkBlue"))
			Console.ForegroundColor = ConsoleColor.DarkBlue;
		if(args.Contains("$cDarkCyan"))
			Console.ForegroundColor = ConsoleColor.DarkCyan;
		if(args.Contains("$cDarkGray"))
			Console.ForegroundColor = ConsoleColor.DarkGray;
		if(args.Contains("$cDarkGreen"))
			Console.ForegroundColor = ConsoleColor.DarkGreen;
		if(args.Contains("$cDarkMagenta"))
			Console.ForegroundColor = ConsoleColor.DarkMagenta;
		if(args.Contains("$cDarkRed"))
			Console.ForegroundColor = ConsoleColor.DarkRed;
		if(args.Contains("$cDarkYellow"))
			Console.ForegroundColor = ConsoleColor.DarkYellow;
		
		foreach (var arg in args)
		{
			if(Convert.ToString(arg)?[0] != '$')
				if (arg is IParseTree a)
					Console.Write(Visit(a));
				else
					Console.Write(arg);
		}
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.White;
		if(args.Contains("$beep"))
			Console.Beep();
		if(args.Contains("$end"))
			Environment.Exit(1);
		return null;
	}

	
	private object? Read(object?[] arg)
	{
		return Console.ReadLine();
	}
	
	#endregion
	
	
	#region Functions
	
	private void ExecuteFunction(CDevParser.FunctionCallContext? context, string? name)
	{
		//makes sure function is valid
		if (context is null || name is null)
			throw new Exception("Values can not be null");

		//stores values of existing variables
		Dictionary<string, object?> existing = new();
		
		//creates variables
		for (int i = 0; i < Functions[name].IDENTIFIER().Length - 1; i++)
		{
			int j = i + 1;
			var varName = Functions[name].IDENTIFIER(j).GetText();
			
			//checks if variables already exist
			if (Variables.ContainsKey(varName))
			{
				existing[varName] = Variables[varName];
			}
			
			var varValue = Visit(context.expresion(i));
			if (Functions[name].var(i).GetText() == "string")
				Variables[varName] = varValue?.ToString();
			if (Functions[name].var(i).GetText() == "int")
				Variables[varName] = Convert.ToInt32(varValue?.ToString());
			if (Functions[name].var(i).GetText() == "float")
				Variables[varName] = Convert.ToSingle(varValue?.ToString());
			if (Functions[name].var(i).GetText() == "bool")
				Variables[varName] = Convert.ToBoolean(varValue?.ToString());
			else
				Variables[varName] = varValue;
		}

		//executes code
		if (Functions[name].block().code() is not null)
		{
			//used to execute code variables
			var assiName = Functions[name].block().code().IDENTIFIER().GetText();
			if (Variables.ContainsKey(assiName))
			{
				//restores existing variable
				if (Variables[assiName] is code a)
					Visit(a.block);
			}
		}
		else
		{
			//used to execute normal blocks
			if (Functions[name].block() is not null)
				Visit(Functions[name].block());
		}

		
		for (int i = 1; i < Functions[name].IDENTIFIER().Length; i++)
		{
			Variables.Remove(Functions[name].IDENTIFIER(i).GetText());
		}
		//removes variables
		
		for (int i = 1; i < Functions[name].IDENTIFIER().Length; i++)
		{
			var varName = Functions[name].IDENTIFIER(i).GetText();
			
			if (existing.ContainsKey(varName))
			{
				Variables[varName] = existing[varName];
			}
		}
		
		var mod = Functions[name].mod().modcmd().GetText();
		
		//loop mod
		if (mod is not null)
		{
			if (mod == "loop")
			{
				ExecuteFunction(context, name);
			}
		}
	}
	public override object? VisitFunctionDeclaration(CDevParser.FunctionDeclarationContext context)
	{
		//variables
		var name = context.IDENTIFIER(0).GetText();
		Functions[name] = context;

		//makes sure its not null
		while (Functions[name] is null)
		{
			Functions[name] = context;
		}
		
		//checks is the return statement is valid
		if (Functions[name].type().GetText() == "string" || Functions[name].type().GetText() == "int" || Functions[name].type().GetText() == "float" || Functions[name].type().GetText() == "bool" )
		{
			//makes sure the return statement exits
			if (Functions[name].block().returnStatement() is null)
			{
				throw new Exception($"{Functions[name].type().GetText()} type functions must have a return statement");
			}
		}else if (Functions[name].type().GetText() == "void")
		{
			//makes sure the return statement doesnt exits
			if (Functions[name].block().returnStatement() is not null)
			{
				throw new Exception($"{Functions[name].type().GetText()} type functions can not have a return statement");
			}
			return null;
		}
		//startup mod
		if (context.mod().modcmd() is not null)
		{
			var MOD = context.mod().modcmd().GetText();
			if (MOD == "run")
			{
				Visit(context.block());
			}
		}
		
		return null;
	}
	
	
	public override object? VisitFunctionCall(CDevParser.FunctionCallContext context)
	{
		//variables
		var name = context.IDENTIFIER().GetText();
		var args = context.expresion().Select(Visit).ToArray();
		//checks if its a pre-defined function
		if (Variables.ContainsKey(name))
		{
			if (Variables[name] is Func<object?[], object?> func1)
				return func1(args);
			throw new Exception($"{name} is not a function");
		}
		//checks if the user made the function
		if (Functions.ContainsKey(name))
		{
			//checks if the function has code in it
			if (Functions[name] is null)
				throw new Exception("Unable to use a null value as a function");
			
			
			
			var mod = Functions[name].mod().modcmd().GetText();
			//thread mod
			
			
			if (mod is not null)
			{
				if (mod == "exe")
				{
					for(int i = 0; i < Convert.ToInt32(Visit(Functions[name].mod().expresion()));i++)
						ExecuteFunction(context, name);
				}
				else
				{
					ExecuteFunction(context, name);
				}
			}
			else
			{
				ExecuteFunction(context, name);
			}

			//returns null if its a void function
			if (Functions[name].block().returnStatement() is null)
				return null;
			
			var varValue = Visit(Functions[name].block().returnStatement().expresion());
			
			//converts and returns the variable
			if (Functions[name].type().GetText() == "int")
				return Convert.ToInt32(varValue);
			if(Functions[name].type().GetText() == "float")
				return Convert.ToSingle(varValue);
			if(Functions[name].type().GetText() == "bool")
				return Convert.ToBoolean(varValue);
			if(Functions[name].type().GetText() == "string")
				return Convert.ToString(varValue);
			return null;
		}
		
		throw new Exception($"Function {name} was not defined");
		
	}

	#endregion
	

	#region Variables

	//Multy
	public override object? VisitMultyAssignment(CDevParser.MultyAssignmentContext context)
	{
		//variables
		var varName = context.IDENTIFIER().GetText();
		var varValue = Visit(context.expresion(1));
		var varPlace = Convert.ToInt32(Visit(context.expresion(0)));

		if (!Variables.ContainsKey(varName))
			throw new Exception($"{varName} was not declared in this scope");
		
		//Makes sure that the correct type of value is assigned
		if(Variables[varName] is string?[] s)
			s[varPlace] = varValue?.ToString();
		else if (Variables[varName] is int?[] i)
			i[varPlace] = Convert.ToInt32(varValue);
		else if(Variables[varName] is float?[] f)
			f[varPlace] = Convert.ToSingle(varValue);
		else if(Variables[varName] is bool?[] b)
			b[varPlace] = Convert.ToBoolean(varValue);
		else if ( Variables[varName] is object?[] o)
			o[varPlace] = varValue;
		else if (Variables[varName] is Dictionary<int, int?> iL)
			iL[varPlace] = Convert.ToInt32(varValue);
		else if(Variables[varName] is Dictionary<int, float?> fL)
			fL[varPlace] = Convert.ToSingle(varValue);
		else if(Variables[varName] is Dictionary<int, bool?> bL)
			bL[varPlace] = Convert.ToBoolean(varValue);
		else if (Variables[varName] is Dictionary<int, object?> oL)
			oL[varPlace] = varValue;
		else if (Variables[varName] is Dictionary<int, string?> sL)
			sL[varPlace] = varValue?.ToString();
		else if (Variables[varName] is Dictionary<object, int?> iD)
			iD[varPlace] = Convert.ToInt32(varValue);
		else if(Variables[varName] is Dictionary<object, float?> fD)
			fD[varPlace] = Convert.ToSingle(varValue);
		else if(Variables[varName] is Dictionary<object, bool?> bD)
			bD[varPlace] = Convert.ToBoolean(varValue);
		else if (Variables[varName] is Dictionary<object, object?> oD)
			oD[varPlace] = varValue;
		else if (Variables[varName] is Dictionary<object, string?> sD)
			sD[varPlace] = varValue?.ToString();
		else
			throw new Exception($"Unknown variable type {Variables[varName]?.GetType()}");


		return null;
	}
	//multy swap
	public override object? VisitMultySwap(CDevParser.MultySwapContext context)
	{
		var name = context.IDENTIFIER(0).GetText();
		var place = Convert.ToInt32(Visit(context.expresion()));
		var value = context.IDENTIFIER(0).GetText();
		
		//checks if the variable is declared
		if (!Variables.ContainsKey(name))
			throw new Exception($"{name} was not declared in this scope");
		//checks if second variable is declared
		if (!Variables.ContainsKey(value))
			throw new Exception($"{value} was not declared in this scope");
		
		//finds what type of variable it is
		if (Variables[name] is string[] s)
		{
			var temp = s[place];
			s[place] = Variables[value] as string;
			Variables[value] = temp;
		}
		else if (Variables[name] is int[] i)
		{
			var temp = i[place];
			i[place] = Convert.ToInt32(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is float[] f)
		{
			var temp = f[place];
			f[place] = Convert.ToSingle(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is bool[] b)
		{
			var temp = b[place];
			b[place] = Convert.ToBoolean(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is object[] o)
		{
			var temp = o[place];
			o[place] = Variables[value];
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<int, int> iL)
		{
			var temp = iL[place];
			iL[place] = Convert.ToInt32(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<int, float> fL)
		{
			var temp = fL[place];
			fL[place] = Convert.ToSingle(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<int, bool> bL)
		{
			var temp = bL[place];
			bL[place] = Convert.ToBoolean(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<int, object> oL)
		{
			var temp = oL[place];
			oL[place] = Variables[value];
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<int, string> sL)
		{
			var temp = sL[place];
			sL[place] = Variables[value] as string;
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<object, int> iD)
		{
			var temp = iD[place];
			iD[place] = Convert.ToInt32(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<object, float> fD)
		{
			var temp = fD[place];
			fD[place] = Convert.ToSingle(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<object, bool> bD)
		{
			var temp = bD[place];
			bD[place] = Convert.ToBoolean(Variables[value]);
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<object, object> oD)
		{
			var temp = oD[place];
			oD[place] = Variables[value];
			Variables[value] = temp;
		}
		else if (Variables[name] is Dictionary<object, string> sD)
		{
			var temp = sD[place];
			sD[place] = Variables[value] as string;
			Variables[value] = temp;
		}
		else
			throw new Exception($"Unknown variable type {Variables[name]?.GetType()}");

		return null;
	}

	public override object? VisitMultyMultySwap(CDevParser.MultyMultySwapContext context)
	{
		var name = context.IDENTIFIER(0).GetText();
		var place = Convert.ToInt32(Visit(context.expresion(0)));
		var vName = context.IDENTIFIER(1).GetText();
		var vPlace = Convert.ToInt32(Visit(context.expresion(1)));
		
		//checks if the variable is declared
		if (!Variables.ContainsKey(name))
			throw new Exception($"{name} was not declared in this scope");
		//checks if second variable is declared
		if (!Variables.ContainsKey(vName))
			throw new Exception($"{vName} was not declared in this scope");
		
		//finds what type of variable it is
		if (Variables[name] is object[] s)
		{
			var temp = s[place];
			if (Variables[vName] is string[] vs)
			{
				s[place] = vs[vPlace];
				vs[vPlace] = temp.ToString();
			}
			else if (Variables[vName] is int[] vi)
			{
				s[place] = vi[vPlace];
				vi[vPlace] = Convert.ToInt32(temp);
			}
			else if (Variables[vName] is float[] vf)
			{
				s[place] = vf[vPlace];
				vf[vPlace] = Convert.ToSingle(temp);
			}
			else if (Variables[vName] is bool[] vb)
			{
				s[place] = vb[vPlace];
				vb[vPlace] = Convert.ToBoolean(temp);
			}
			else if (Variables[vName] is object[] vo)
			{
				s[place] = vo[vPlace];
				vo[vPlace] = temp;
			}
			else if (Variables[vName] is Dictionary<int, int> viL)
			{
				s[place] = viL[vPlace];
				viL[vPlace] = Convert.ToInt32(temp);
			}
			else if (Variables[vName] is Dictionary<int, float> vfL)
			{
				s[place] = vfL[vPlace];
				vfL[vPlace] = Convert.ToSingle(temp);
			}
			else if (Variables[vName] is Dictionary<int, bool> vbL)
			{
				s[place] = vbL[vPlace];
				vbL[vPlace] = Convert.ToBoolean(temp);
			}
			else if (Variables[vName] is Dictionary<int, object> voL)
			{
				s[place] = voL[vPlace];
				voL[vPlace] = temp;
			}
			else if (Variables[vName] is Dictionary<int, string> vsL)
			{
				s[place] = vsL[vPlace];
				vsL[vPlace] = Convert.ToString(temp);
			}
			else if (Variables[vName] is Dictionary<object, int> viO)
			{
				s[place] = viO[vPlace];
				viO[vPlace] = Convert.ToInt32(temp);
			}
			else if (Variables[vName] is Dictionary<object, float> vfO)
			{
				s[place] = vfO[vPlace];
				vfO[vPlace] = Convert.ToSingle(temp);
			}
			else if (Variables[vName] is Dictionary<object, bool> vbO)
			{
				s[place] = vbO[vPlace];
				vbO[vPlace] = Convert.ToBoolean(temp);
			}
			else if (Variables[vName] is Dictionary<object, object> voO)
			{
				s[place] = voO[vPlace];
				voO[vPlace] = temp;
			}
			else if (Variables[vName] is Dictionary<object, string> vsO)
			{
				s[place] = vsO[vPlace];
				vsO[vPlace] = Convert.ToString(temp);
			}
		}

		return null;
	}

	//arrays
	public override object? VisitArrayDeclaration(CDevParser.ArrayDeclarationContext context)
	{
		var varName = Convert.ToString(context.IDENTIFIER());
		var varPlace = Convert.ToInt32(Visit(context.expresion(0)));
		
		if(varName is not null){
			if(Variables.ContainsKey(varName))
				throw new Exception($"{varName} was already declared");
			
			//checks the type and initialises the array
			if (context.var().GetText() == "int")
			{
				Variables[varName] = new int?[varPlace];
			}
			else if (context.var().GetText() == "string")
			{
				Variables[varName] = new string?[varPlace];
			}
			else if (context.var().GetText() == "float")
			{
				Variables[varName] = new float?[varPlace];
			}
			else if (context.var().GetText() == "bool")
			{
				Variables[varName] = new bool?[varPlace];
			}
			else if (context.var().GetText() == "var")
			{
				Variables[varName] = new object?[varPlace];
			}
			else
				throw new Exception($"Unknown variable type {context.var().GetText()}");
			if(context.expresion(1) is not null)
				if (context.expresion().Length - 1 > varPlace)
					throw new Exception($"The values assigned to array {context.IDENTIFIER()} must be grater or equal to the amount of places in it");
				else
				{
					var assigntment = Visit(context.expresion(1));
					if (assigntment is Array)
					{
						if (assigntment is object[] o)
							if(Variables[varName] is object?[] vo)
								for (int i = 0; i < o.Length; i ++)
									vo[i] = o[i];
						if (assigntment is string[] s)
							if(Variables[varName] is string?[] vs)
								for (int i = 0; i < s.Length; i ++)
									vs[i] = s[i];
						if (assigntment is int[] iv)
							if(Variables[varName] is int?[] vi)
								for (int i = 0; i < iv.Length; i ++)
									vi[i] = iv[i];
						if (assigntment is float[] f)
							if(Variables[varName] is float?[] vf)
								for (int i = 0; i < f.Length; i ++)
									vf[i] = f[i];
						if (assigntment is bool[] b)
							if(Variables[varName] is bool?[] vb)
								for (int i = 0; i < b.Length; i ++)
									vb[i] = b[i];
					}
					else
					{
						for (int i = 1; i < context.expresion().Length; i++)
						{ 
							assigntment = Visit(context.expresion(i));
							if (Variables[varName] is string?[] s) 
								s[i - 1] = assigntment?.ToString();
							else if (Variables[varName] is int?[] varI) 
								varI[i - 1] = Convert.ToInt32(assigntment?.ToString());
							else if (Variables[varName] is float?[] f) 
								f[i - 1] = Convert.ToSingle(assigntment?.ToString());
							else if (Variables[varName] is bool?[] b) 
								b[i - 1] = Convert.ToBoolean(assigntment?.ToString());
							else if (Variables[varName] is object?[] o) 
								o[i - 1] = assigntment;
						}
					}
				}
		}
		return null;
	}

	//variables
	public override object? VisitVariableAssignment(CDevParser.VariableAssignmentContext context)
	{
		var varName = context.IDENTIFIER().GetText();
		var varValue = Visit(context.expresion());
		
		if(Variables[varName] is string)
			Variables[varName] = varValue?.ToString();
		else if (Variables[varName] is int?)
			Variables[varName] = Convert.ToInt32(varValue);
		else if(Variables[varName] is float?)
			Variables[varName] = Convert.ToSingle(varValue);
		else if(Variables[varName] is bool?)
			Variables[varName] = Convert.ToBoolean(varValue);
		else if (Variables[varName] is object)
			Variables[varName] = varValue;
		else if (Variables[varName] is null)
		{
			if(varValue is string)
				Variables[varName] = String.Empty;
			else if (varValue is int?)
				Variables[varName] = new int?();
			else if(varValue is float?)
				Variables[varName] = new float?();
			else if(varValue is bool?)
				Variables[varName] = new bool?();
			else if (varValue is object)
				Variables[varName] = varValue;
			else
				throw new Exception($"Unknown variable type {Variables[varName]?.GetType()}");
		}
		else
			throw new Exception($"Unknown variable type {Variables[varName]?.GetType()}");
		
		return null;
	}

	public override object? VisitVariableDeclaration(CDevParser.VariableDeclarationContext context)
	{
		var varName = context.IDENTIFIER().GetText();
		object? varValue = new();
		if(context.expresion() is not null)
			 varValue = Visit(context.expresion());
		if (context.var() is not null)
		{
			
			if (Variables.ContainsKey(varName))
				throw new Exception($"Variable {varName} is already declared");
			if (context.var().GetText() == "int")
			{
				Variables[varName] = new int?();
				Variables[varName] = 0;
				if(context.expresion() is not null)
					Variables[varName] = Convert.ToInt32(varValue);

			}
			else if (context.var().GetText() == "string")
			{
				Variables[varName] = String.Empty;
				if(context.expresion() is not null)
					Variables[varName] = Convert.ToString(varValue);
			}
			else if (context.var().GetText() == "float")
			{
				Variables[varName] = new float?();
				Variables[varName] = 0.0;
				if(context.expresion() is not null)
					Variables[varName] = Convert.ToSingle(varValue);
			}
			else if (context.var().GetText() == "bool")
			{
				Variables[varName] = new bool?();
				Variables[varName] = false;
				if(context.expresion() is not null)
					Variables[varName] = Convert.ToBoolean(varValue);
			} 
			else if (context.var().GetText() == "var")
			{
				Variables[varName] = new();
				if(context.expresion() is not null)
					Variables[varName] = varValue;
			}
			else
				throw new Exception($"Unknown variable type {context.var().GetText()}");
		}
		else
		{
			if (context.expresion() is not null)
			{ 
				Variables[varName] = varValue;
			}
			else
			{
				Variables[varName] = null;
			}
		}
		return null;
	}

	public override object? VisitVariableSwap(CDevParser.VariableSwapContext context)
	{
		var name = context.IDENTIFIER(0).GetText();
		var vName = context.IDENTIFIER(1).GetText();
		
		

		return null;
	}

	//List
	public override object? VisitListDeclaration(CDevParser.ListDeclarationContext context)
	{
		var varName = context.IDENTIFIER().GetText();
		
		if (Variables.ContainsKey(varName) && Variables[varName] is not Array)
			throw new Exception($"Variable {varName} is already declared as {Variables[varName]} type");
		
		if(Variables.ContainsKey(varName))
		{
			throw new Exception($"Array {varName} is already declared");
		}
		
		if (Variables.ContainsKey(varName))
			throw new Exception($"Variable {varName} is not an array, in correct syntax usage");
		if (context.var().GetText() == "int")
		{
			Variables[varName] = new Dictionary<int, int?>();
		}
		else if (context.var().GetText() == "string")
		{
			Variables[varName] = new Dictionary<int, string?>();
		}
		else if (context.var().GetText() == "float")
		{
			Variables[varName] = new Dictionary<int, float?>();
		}
		else if (context.var().GetText() == "bool")
		{
			Variables[varName] = new Dictionary<int, bool?>();
		}
		else if (context.var().GetText() == "var")
		{
			Variables[varName] = new Dictionary<int, object?>();
		}
		else
			throw new Exception($"Unknown variable type {context.var().GetText()}");
		if(context.expresion(0) is not null)
		{
				object? assigntment = new();
				for (int i = 0; i < context.expresion().Length; i++)
				{
					assigntment = Visit(context.expresion(i));
					if(Variables[varName] is Dictionary<int, string?> s)
						s[i] = assigntment?.ToString();
					else if (Variables[varName] is Dictionary<int, int?> varI)
						varI[i] = Convert.ToInt32(assigntment?.ToString());
					else if(Variables[varName] is Dictionary<int, float?> f)
						f[i] = Convert.ToSingle(assigntment?.ToString());
					else if(Variables[varName] is Dictionary<int, bool?> b)
						b[i] = Convert.ToBoolean(assigntment?.ToString());
					else if (Variables[varName] is Dictionary<int, object?> o)
						o[i] = assigntment;
				}
		}
		return null;
	}
	//Dictionary
	public override object? VisitDictionaryDeclaration(CDevParser.DictionaryDeclarationContext context)
	{
		
		var varName = context.IDENTIFIER().GetText();
		
		if(Variables.ContainsKey(varName))
		{
			throw new Exception($"Dictionary {varName} is already declared");
		}
		
		if (context.var().GetText() == "int")
		{
			Variables[varName] = new Dictionary<object, int?>();
		}
		else if (context.var().GetText() == "string")
		{
			Variables[varName] = new Dictionary<object, string?>();
		}
		else if (context.var().GetText() == "float")
		{
			Variables[varName] = new Dictionary<object, float?>();
		}
		else if (context.var().GetText() == "bool")
		{
			Variables[varName] = new Dictionary<object, bool?>();
		}
		else if (context.var().GetText() == "var")
		{
			Variables[varName] = new Dictionary<object, object?>();
		}

		return null;
	}

	//general
	public override object? VisitIdentifierExpresion(CDevParser.IdentifierExpresionContext context)
	{
		var varName = context.IDENTIFIER().GetText();

		if (!Variables.ContainsKey(varName))
		{
			throw new Exception($"{varName} was not declared in this scope");
		}

		return Variables[varName];
	}

	public override object? VisitConst(CDevParser.ConstContext context)
	{
		
		if (context.INTIGER() is { } i)
			return int.Parse(i.GetText());
		if (context.FLOAT() is { } f)
			return float.Parse(f.GetText());
		if (context.STRING() is { } s)
			return s.GetText()[1..^1];
		if (context.BOOL() is { } b)
			return b.GetText() == "true";
		if (context.NULL() is { })
			return null;
		throw new Exception("Unknown variable type");
	}

	public override object? VisitMultyExpresion(CDevParser.MultyExpresionContext context)
	{
		
		var varPlace = Convert.ToInt32(Visit(context.multy().expresion()));
		var varName = context.multy().IDENTIFIER().GetText();
		if (context.multy() is { })
			if(Variables[varName] is string?[] s)
				return s[varPlace];
			else if (Variables[varName] is int?[] i)
				return i[varPlace];
			else if(Variables[varName] is float?[] f)
				return f[varPlace];
			else if(Variables[varName] is bool?[] b)
				return b[varPlace];
			else if (Variables[varName] is object?[] o)
				return o[varPlace];
			else if (Variables[varName] is Dictionary<int, string?> sL)
				return sL[varPlace];
			else if (Variables[varName] is Dictionary<int, int?> iL)
				return iL[varPlace];
			else if(Variables[varName] is Dictionary<int, float?> fL)
				return fL[varPlace];
			else if(Variables[varName] is Dictionary<int, bool?> bL)
				return bL[varPlace];
			else if (Variables[varName] is Dictionary<int, object?> oL)
				return oL[varPlace];
			else if (Variables[varName] is Dictionary<object, string?> sD)
				return sD[varPlace];
			else if (Variables[varName] is Dictionary<object, int?> iD)
				return iD[varPlace];
			else if(Variables[varName] is Dictionary<object, float?> fD)
				return fD[varPlace];
			else if(Variables[varName] is Dictionary<object, bool?> bD)
				return bD[varPlace];
			else if (Variables[varName] is Dictionary<object, object?> oD)
				return oD[varPlace];
		return null;
	}

	#endregion
	

	#region Assignment
	public override object? VisitParenthesizeExpresion(CDevParser.ParenthesizeExpresionContext context)	{
		return Visit(context.expresion());
	}
	
	//functios for operations 
	private object? Multiply(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l * r;
		if (left is float lf && right is float rf)
			return lf * rf;
		if (left is int lint && right is float rFloat)
			return lint * rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat * rint;
		if (left is object  || right is object)
			return Multiply(Convert.ToInt32(left), Convert.ToInt32(right));
		if (left is string ls)
		{
			if (right is int rsi)
			{
				string returnStr = String.Empty;
				for (int i = 0; i < rsi; i ++)
				{
					returnStr += ls;
				}
				return returnStr;
			}
			throw new Exception($"{right} mus be int type");
		}

		if (right is string rs)
		{
			if (left is int lsi)
			{
				string returnStr = String.Empty;
				for (int i = 0; i < lsi; i ++)
				{
					returnStr += rs;
				}

				return returnStr;
			} 
			throw new Exception($"{left} mus be int type");
		}
		
		throw new Exception($"{left?.GetType()} {left} can not be multiplied to {right?.GetType()} {right}");
	}

	private object? Module(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l % r;
		if (left is float lf && right is float rf)
			return lf % rf;
		if (left is int lint && right is float rFloat)
			return lint % rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat % rint;
		if (left is object  || right is object)
			return Module(Convert.ToInt32(left), Convert.ToInt32(right));
		throw new Exception($"{left?.GetType()} {left} can not be moduled to {right?.GetType()} {right}");
	}

	private object? Divid(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l / r;
		if (left is float lf && right is float rf)
			return lf / rf;
		if (left is int lint && right is float rFloat)
			return lint / rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat / rint;
		if (left is object  || right is object)
			return Divid(Convert.ToInt32(left), Convert.ToInt32(right));
		throw new Exception($"{left?.GetType()} {left} can not be divided to {right?.GetType()} {right}");
	}
	private object? Add(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l + r;
		if (left is float lf && right is float rf)
			return lf + rf;
		if (left is int lint && right is float rFloat)
			return lint + rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat + rint;
		if (left is string || right is string)
			return $"{left}{right}";
		if (left is object  || right is object)
			return Add(Convert.ToInt32(left), Convert.ToInt32(right));

		throw new Exception($"{left?.GetType()} {left} can not be added to {right?.GetType()} {right}");
	}
	private object? Subtract(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l - r;
		if (left is float lf && right is float rf)
			return lf - rf;
		if (left is int lint && right is float rFloat)
			return lint - rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat - rint;
		if (left is object  || right is object)
			return Subtract(Convert.ToInt32(left), Convert.ToInt32(right));
		throw new Exception($"{left?.GetType()} {left} can not be subtracted to {right?.GetType()} {right}");
	}
	
	public override object? VisitAdditionExpresion(CDevParser.AdditionExpresionContext context)
	{
		var left = Visit(context.expresion(0));
		var right = Visit(context.expresion(1));

		var op = context.addOp().GetText();

		return op switch
		{
			"+" => Add(left, right),
			"-" => Subtract(left, right),
			_ => throw new Exception("Unknown expression")
		};
	}

	public override object? VisitMultiplicationExpresion(CDevParser.MultiplicationExpresionContext context)
	{
		var left = Visit(context.expresion(0));
		var right = Visit(context.expresion(1));

		var op = context.multOp().GetText();

		return op switch
		{
			"*" => Multiply(left, right),
			"/" => Divid(left, right),
			"%" => Module(left, right),
			_ => throw new Exception("Unknown expression")
		};
	}

	#endregion
	

	#region Statements
	//functions for statements
	private bool IsTrue(object? value)
	{
		if (value is bool b)
			return b;
		throw new Exception("Value must be boolean");
	}

	private bool IsFalse(object? value) => !IsTrue(value);
	
	public override object? VisitWhileStatement(CDevParser.WhileStatementContext context)
	{
		Func<object?, bool> condition = context.WHILE().GetText() == "while()"
				? IsTrue
				: IsFalse
			;

		if (condition(Visit(context.expresion())))
		{
			do
			{
				Visit(context.block(), 0);
			} while (condition(Visit(context.expresion())));
		}
		else
		{
			var value = context.blockElseIf();
			while (value is null)
				value = context.blockElseIf();
			Visit(value);
		}

		return null;
	}

	public override object? VisitIfStatement(CDevParser.IfStatementContext context)
	{
		Func<object?, bool> condition = context.IF().GetText() == "if()"
				? IsTrue
				: IsFalse
			;

		if (condition(Visit(context.expresion())))
		{
			Visit(context.block(), 0);
		}
		else
		{
			if (context.blockElseIf() is not null)
			{
				var value = context.blockElseIf();
				while (value is null)
					value = context.blockElseIf();
				Visit(value);
			}
		}

		return null;
	}
	
	public override object? VisitPassStatement(CDevParser.PassStatementContext context)
	{
		Func<object?, bool> condition = context.PASS().GetText() == "pass()"
				? IsTrue
				: IsFalse
			;

		if (!condition(Visit(context.expresion())))
		{

			var fileName = GlobleVariables.CompFile.Value; // args[0]?

			if (fileName is not null)
			{
				var fileContents = File.ReadAllText(fileName);

				var inputStream = new AntlrInputStream(fileContents);
				var cdevLexer = new CDevLexer(inputStream);

				CommonTokenStream commonTokenStream = new CommonTokenStream(cdevLexer);

				var cdevParser = new CDevParser(commonTokenStream);
				var cdevContext = cdevParser.program();
			
				Contractor();
				Visit(cdevContext);
				Environment.Exit(0);
			}
		}
		
		
		return null;
	}
	

	public override object? VisitRepeatStatement(CDevParser.RepeatStatementContext context)
	{
		string varName = String.Empty;
		int init = 0;
		int add = 1;

		if (context.list() is not null)
		{
			int[] list = new int[context.list().exp().Length];

			for (int i = 0; i < context.list().exp().Length; i++)
			{
				list[i] = Convert.ToInt32(Visit(context.list().exp(i)));
			}
		
			if (context.IDENTIFIER() is not null)
				varName = context.IDENTIFIER().GetText();
			else
				varName = FindVarName();
			
			for (int i = init; i < context.list().exp().Length; i += add)
			{
				if (!(varName == String.Empty))
					Variables[varName] = list[i];
				Visit(context.block(), 0);
			}
		}
		else
		{
		
			if (context.IDENTIFIER() is not null)
				varName = context.IDENTIFIER().GetText();
			else
				varName = FindVarName();
		
			if (context.expresion(1) is not null)
			{
				init = Convert.ToInt32(Visit(context.expresion(1)));
				if (context.expresion(2) is not null)
					add = Convert.ToInt32(Visit(context.expresion(2)));
			}
		
			for (int i = init; i < Convert.ToInt32(Visit(context.expresion(0))); i += add)
			{
				if (!(varName == String.Empty))
					Variables[varName] = i;
				Visit(context.block(), 0);
			}
			
			if (!(varName == String.Empty))
				Variables.Remove(varName);
		}
		return null;
	}

	private string FindVarName()
	{

		if (!Variables.ContainsKey("i"))
			return "i"; 
		if (!Variables.ContainsKey("j"))
			return "j";
		if (!Variables.ContainsKey("k"))
			return "k";
		if (!Variables.ContainsKey("o"))
			return "o";
		return String.Empty;
	}

	#endregion
	

	#region Expressions
	//functions for boolean expresions
	private bool And(object? left, object? right)
	{
		if (left is bool l && right is bool r)
			return l || r;

		throw new Exception($"{left?.GetType()} can not be compared to {right?.GetType()} {right}. They both must be boolean values");
	}
	
	private bool Or(object? left, object? right)
	{
		if (left is bool l && right is bool r)
			return l && r;

		throw new Exception($"{left?.GetType()} can not be compared to{right?.GetType()} {right}. They both must be boolean values");
	}

	//functions for comparison expresions
	private bool isEqual(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l != r;
		if (left is float lf && right is float rf)
			return lf != rf;
		if (left is int lint && right is float rFloat)
			return lint != rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat != rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}
	

	private bool Greater(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l < r;
		if (left is float lf && right is float rf)
			return lf < rf;
		if (left is int lint && right is float rFloat)
			return lint < rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat < rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}


	private bool GreaterEqual(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l <= r;
		if (left is float lf && right is float rf)
			return lf <= rf;
		if (left is int lint && right is float rFloat)
			return lint <= rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat <= rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}

	private bool NotEqual(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l == r;
		if (left is float lf && right is float rf)
			return lf == rf;
		if (left is int lint && right is float rFloat)
			return lint == rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat == rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}

	private bool Smaller(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l > r;
		if (left is float lf && right is float rf)
			return lf > rf;
		if (left is int lint && right is float rFloat)
			return lint > rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat > rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}


	private bool SmallerEqual(object? left, object? right)
	{
		if (left is int l && right is int r)
			return l >= r;
		if (left is float lf && right is float rf)
			return lf >= rf;
		if (left is int lint && right is float rFloat)
			return lint >= rFloat;
		if (left is float lfloat && right is int rint)
			return lfloat >= rint;

		throw new Exception($"{left?.GetType()} {left} can not be compared to {right?.GetType()} {right}");
	}
	#region Comparison

	public override object? VisitComparisonExpresion(CDevParser.ComparisonExpresionContext context)
	{
		var left = Visit(context.expresion(0));
		var right = Visit(context.expresion(1));

		var op = context.comperaOp().GetText();

		return op switch
		{
			"==" => isEqual(left, right),
			"!=" => NotEqual(left, right),
			">" => Greater(left, right),
			"<" => Smaller(left, right),
			">=" => GreaterEqual(left, right),
			"<=" => SmallerEqual(left, right),
			_ => throw new Exception("Unknown comparison operator")
		};
	}

	public override object? VisitNotExpresion(CDevParser.NotExpresionContext context)
	{
		var expresion = Visit(context.expresion());
		if (expresion is bool e)
			if (e)
				return false;
			else
				return true;
		throw new Exception("Expression in a not expression must be a boolean");
	}
	
	#endregion

	#region Boolean

	public override object? VisitBoolExpresion(CDevParser.BoolExpresionContext context)
	{
		var left = Visit(context.expresion(0));
		var right = Visit(context.expresion(1));

		var op = context.boolOp().GetText();

		return op switch
		{
			"&&" => And(left, right),
			"||" => Or(left, right),
			_ => throw new Exception("Unknown boolean operator")
		};
	}

	#endregion

	#endregion
	

	#region code

	private struct code
	{
		public CDevParser.BlockContext? block;

		public string? GetString()
		{
			return block?.GetText();
		}
	}

	public override object? VisitCodeDeclaration(CDevParser.CodeDeclarationContext context)
	{
		var varName = context.IDENTIFIER().GetText();
		Variables[varName] = new code();
		if (context.block().code() is not null)
		{
			var assiName = context.block().code().IDENTIFIER().GetText();
			if (Variables.ContainsKey(assiName))
			{
				if(Variables[assiName] is code aN)
					if (Variables[varName] is code bN)
					{
						bN.block = aN.block;
						Variables[varName] = bN;
					}
					else
						throw new Exception($"{assiName} can not be assigned to {varName}");
				else
					throw new Exception($"{assiName} was not declared in this scope");

			}
		}
		else
		{
			if (Variables[varName] is code a)
			{
				a.block = context.block();
				Variables[varName] = a;
			}
			
		}
		return null;
	}

	public override object? VisitCodeAssignment(CDevParser.CodeAssignmentContext context)
	{
		var varName = context.IDENTIFIER().GetText();
		if (context.block().code() is not null)
		{
			var assiName = context.block().code().IDENTIFIER().GetText();
			if (Variables.ContainsKey(assiName))
			{
				if(Variables[assiName] is code aN)
					if (Variables[varName] is code bN)
					{
						bN.block = aN.block;
						Variables[varName] = bN;
					}
			}
			else
				throw new Exception($"{assiName} was not declared in this scope");
		}

		if (context.block() is not null)
			if (Variables[varName] is code a)
			{
				a.block = context.block();
				Variables[varName] = a;
			}
		

		return null;
	}

	#endregion

	private object? Visit(CDevParser.BlockContext? block, int i)
	{
		var value = block;
		while (value is null)
			value = block;
		return Visit(value);
	}
}