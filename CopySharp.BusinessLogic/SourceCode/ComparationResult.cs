using CopySharp.BusinessLogic.Fingerprinting;

namespace CopySharp.BusinessLogic.SourceCode
{
  public struct ComparationResult
  {
    public GraphFingerprint Fingerprint { get; set; }
    public string SolutionFilePath { get; set; }
    public string ProjectFilePath { get; set; }
  }
}
