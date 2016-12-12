using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopySharp.BusinessLogic.SourceCode;

namespace CopySharp.Console
{
  partial class ResultsTemplate
  {
    private IList<ComparationResult> m_comparationResults;
    private DateTime m_generatedOn;
    private TimeSpan m_elapsedTime;

    public ResultsTemplate(IList<ComparationResult> comparationResults, DateTime generatedOn, TimeSpan elapsedTime)
    {
      m_comparationResults = comparationResults;
      m_generatedOn = generatedOn;
      m_elapsedTime = elapsedTime;
    }
  }
}
