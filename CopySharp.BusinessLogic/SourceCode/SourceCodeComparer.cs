using CopySharp.BusinessLogic.Symbols;
using CopySharp.BusinessLogic.Fingerprinting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;

namespace CopySharp.BusinessLogic.SourceCode
{
  public class SourceCodeComparer
  {
    private List<string> m_solutionFiles;
    private List<string> m_projectFiles;

    public SourceCodeComparer()
    {
      m_solutionFiles = new List<string>();
      m_projectFiles = new List<string>();
    }

    public void AppendSolutionFile(string fullName)
    {
      m_solutionFiles.Add(fullName);
    }

    public void AppendProjectFile(string fullName)
    {
      m_projectFiles.Add(fullName);
    }

    public IList<ComparationResult> CompareAll()
    {
      //Build fingerprints
      FingerprintComputer computer = new FingerprintComputer(16, 4);
      List<ComparationResult> results = new List<SourceCode.ComparationResult>();

      using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
      {
        foreach (string s in m_solutionFiles)
        {
          Solution sol = workspace.OpenSolutionAsync(s).Result;

          foreach (Project p in sol.Projects)
          {
            SymbolTreeBuilder stb = new SymbolTreeBuilder();
            stb.Build(p);
            SymbolTree stree = stb.ToCachedSymbolTree();
            
            GraphFingerprint fp = computer.Compute(stree);

            ComparationResult result = new ComparationResult()
            {
              SolutionFilePath = sol.FilePath,
              ProjectFilePath = p.FilePath,
              Fingerprint = fp
            };
            results.Add(result);
          }
          workspace.CloseSolution();
        }

        foreach (string s in m_projectFiles)
        {
          Project proj = workspace.OpenProjectAsync(s).Result;

          SymbolTreeBuilder stb = new SymbolTreeBuilder();
          stb.Build(proj);
          SymbolTree stree = stb.ToCachedSymbolTree();

          GraphFingerprint fp = computer.Compute(stree);

          ComparationResult result = new ComparationResult()
          {
            SolutionFilePath = proj.Solution == null ? string.Empty : proj.Solution.FilePath,
            ProjectFilePath = proj.FilePath,
            Fingerprint = fp
          };
          results.Add(result);
        }
      }

      return results;
    }
  }
}
