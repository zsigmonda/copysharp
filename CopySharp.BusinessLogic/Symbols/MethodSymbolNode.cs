using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class MethodSymbolNode : SymbolNode
  {
    public IMethodSymbol InnerSymbol
    {
      get { return (IMethodSymbol)base.InnerSymbol; }
    }

    internal MethodSymbolNode(IMethodSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      ;
    }

    public override uint GetLabel()
    {
      return (uint)(
        (InnerSymbol.IsAbstract ? (1 << 24) : 0) +
        (InnerSymbol.IsAsync ? (1 << 23) : 0) +
        (InnerSymbol.IsStatic ? (1 << 22) : 0) +
        (StartPointIsReachable ? (1 << 21) : 0) +
        (EndPointIsReachable ? (1 << 20) : 0) +
        (ReturnStatementsCount) +
        (ExitPointsCount) +
        (StatementsCount) +
        (BlocksCount)) + (1 << 30);
    }

    internal bool StartPointIsReachable { get; set; }
    internal bool EndPointIsReachable { get; set; }
    internal int ReturnStatementsCount { get; set; }
    internal int ExitPointsCount { get; set; }
    internal int StatementsCount { get; set; }
    internal int BlocksCount { get; set; }
  }
}
