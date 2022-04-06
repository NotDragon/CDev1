using Antlr4.Runtime;
using CDev.Content;

namespace CDev 
{
	public static class ProgramVariable
	{
		
		public static string fileName = GlobleVariables.CompFile.Value;

		public static string fileContents = File.ReadAllText(fileName);

		public static AntlrInputStream inputStream = new(fileContents);
		public static CDevLexer cdevLexer = new(inputStream);

		public static CommonTokenStream commonTokenStream = new(cdevLexer);

		public static CDevParser cdevParser = new(commonTokenStream);
		
		public static CDevParser.ExecuterContext cdevExecuterContext = cdevParser.executer();

		public static CDevVisitor visitor = new();
		public static void SetDValues()
		{
			fileContents = File.ReadAllText(fileName);
			inputStream = new(fileContents);
			cdevLexer = new(inputStream);
			commonTokenStream = new(cdevLexer);
			cdevParser = new(commonTokenStream);
			cdevExecuterContext = cdevParser.executer();
			visitor = new();
		}
	   
	}

	public class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{ 
				ProgramVariable.fileName = args[0]; 
				Run(args[0]);
			}
			else
			{
				Imersive.Console.Logger.Error("Missing argument \"file name\"");
			}
		}

		public static void Run(string fileName)
		{
			
			string fileContents = File.ReadAllText(fileName);
			
			AntlrInputStream inputStream = new(fileContents);
			CDevLexer cdevLexer = new(inputStream);
			
			CommonTokenStream commonTokenStream = new(cdevLexer);
			CDevParser cdevParser = new(commonTokenStream);
			
			CDevParser.ProgramContext cdevProgram = cdevParser.program();
			
			CDevVisitor visitor = new();

			visitor.Visit(cdevProgram);
		}
	}
}