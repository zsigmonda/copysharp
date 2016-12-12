using System;
using CopySharp.BusinessLogic;
using CopySharp.BusinessLogic.SourceCode;
using System.Collections.Generic;

namespace CopySharp.Console
{
  public class Program
  {
    public static void Main(string[] args)
    {
      try
      {
        CommandLineOptions options = new Console.CommandLineOptions();
        if (CommandLine.Parser.Default.ParseArguments(args, options))
        {
          SourceCodeComparer sourceCodeComparer = new SourceCodeComparer();
          if (options.SolutionFilePaths != null)
          {
            foreach (string s in options.SolutionFilePaths)
            {
              if (System.IO.File.Exists(s))
              {
                sourceCodeComparer.AppendSolutionFile(s);
              }
              else
              {
                System.Console.WriteLine("File not found: {0}", s);
              }
            }
          }
          if (options.ProjectFilePaths != null)
          {
            foreach (string s in options.ProjectFilePaths)
            {
              if (System.IO.File.Exists(s))
              {
                sourceCodeComparer.AppendProjectFile(s);
              }
              else
              {
                System.Console.WriteLine("File not found: {0}", s);
              }
            }
          }

          DateTime start = DateTime.Now;
          IList<ComparationResult> result = sourceCodeComparer.CompareAll();

          ResultsTemplate resultsTemplate = new Console.ResultsTemplate(result, DateTime.Now, DateTime.Now - start);
          string outputContent = resultsTemplate.TransformText();

          System.IO.File.WriteAllText(options.OutputFilePath, outputContent);
          System.Console.WriteLine("Source code comparation finished. Output written to {0}", options.OutputFilePath);
        }
      }
      catch (Exception ex)
      {
        System.Console.WriteLine("An unhandled exception occured during execution.\n\n{0}\n\n{1}\n\nExiting...", ex.Message, ex.StackTrace);
      }
    }
  }
}
