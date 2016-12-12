using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class LocalSymbolNode : SymbolNode
  {
    public ILocalSymbol InnerSymbol
    {
      get { return (ILocalSymbol)base.InnerSymbol; }
    }

    internal LocalSymbolNode(ILocalSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      ;
    }

    public override uint GetLabel()
    {
      return (uint)((AlwaysAssigned ? (1 << 8) : 0) +
        (Captured ? (1 << 7) : 0) +
        (DataFlowsIn ? (1 << 6) : 0) +
        (DataFlowsOut ? (1 << 5) : 0) +
        (ReadInside ? (1 << 4) : 0) +
        (ReadOutside ? (1 << 3) : 0) +
        (WrittenInside ? (1 << 2) : 0) +
        (WrittenOutside ? 1 : 0)) + 1024;
    }

    internal bool AlwaysAssigned { get; set; }
    internal bool Captured { get; set; }
    internal bool DataFlowsIn { get; set; }
    internal bool DataFlowsOut { get; set; }
    internal bool ReadInside { get; set; }
    internal bool ReadOutside { get; set; }
    internal bool WrittenInside { get; set; }
    internal bool WrittenOutside { get; set; }
  }
}
