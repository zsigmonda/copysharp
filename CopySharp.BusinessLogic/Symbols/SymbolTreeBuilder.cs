using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class SymbolTreeBuilder
  {
    private InternalSymbolTreeBuilder m_builder;

    private class InternalSymbolTreeBuilder : SymbolVisitor
    {
      private Stack<SymbolNode> m_parentNodes;
      private SymbolNode m_root;

      public SymbolNode Root
      {
        get { return m_root; }
      }

      public override void VisitNamespace(INamespaceSymbol symbol)
      {
        //Ha rendelkezésünkre áll a forráskódja, akkor belépünk - máskülönben nem mi deklaráltuk az adott szimbólumot
        if (symbol.DeclaringSyntaxReferences.Length > 0)
        {
          SymbolNode parent = m_parentNodes.Count > 0 ? m_parentNodes.Peek() : null;

          NamespaceSymbolNode nsn = new NamespaceSymbolNode(symbol, parent, null);
          nsn.IsDeclaration = false; //Ez igazából nem is értelmezett...
          if (m_root == null)
            m_root = nsn;

          m_parentNodes.Push(nsn);
          foreach (var child in symbol.GetMembers())
          {
            child.Accept(this);
          }
          m_parentNodes.Pop();
        }
      }

      public override void VisitNamedType(INamedTypeSymbol symbol)
      {
        if (symbol.DeclaringSyntaxReferences.Length > 0)
        {
          SymbolNode parent = m_parentNodes.Peek();

          NamedTypeSymbolNode ntsn = new NamedTypeSymbolNode(symbol, parent, null);
          ntsn.IsDeclaration = true;

          m_parentNodes.Push(ntsn);
          foreach (var child in symbol.GetMembers())
          {
            child.Accept(this);
          }
          m_parentNodes.Pop();

          foreach (var iface in symbol.Interfaces)
          {
            NamedTypeSymbolNode ifs = new NamedTypeSymbolNode(iface, ntsn, null);
            ntsn.IsDeclaration = false;
          }
        }
      }

      public override void VisitMethod(IMethodSymbol symbol)
      {
        //Ha nem külső refenercia - azaz megvan a forráskódunk is
        if (symbol.DeclaringSyntaxReferences.Length > 0)
        {
          SymbolNode parent = m_parentNodes.Peek();

          MethodSymbolNode msn = new MethodSymbolNode(symbol, parent);
          msn.IsDeclaration = true;

          if (symbol.ReturnType != null)
          {
            TypeSymbolNode tsn = new Symbols.TypeSymbolNode(symbol.ReturnType, msn);
            tsn.IsDeclaration = false;
          }
          
          foreach (ITypeParameterSymbol s in symbol.TypeParameters)
          {
            TypeSymbolNode tsn = new Symbols.TypeSymbolNode(s, msn);
            tsn.IsDeclaration = false;
          }
        }
      }

      public override void VisitField(IFieldSymbol symbol)
      {
        SymbolNode parent = m_parentNodes.Peek();

        FieldSymbolNode fsn = new FieldSymbolNode(symbol, parent);
        fsn.IsDeclaration = true;

        if (symbol.Type != null)
        {
          TypeSymbolNode tsn = new Symbols.TypeSymbolNode(symbol.Type, fsn);
          tsn.IsDeclaration = false;
        }
      }

      public override void VisitEvent(IEventSymbol symbol)
      {
        SymbolNode parent = m_parentNodes.Peek();

        EventSymbolNode esn = new EventSymbolNode(symbol, parent);
        esn.IsDeclaration = true;

        if (symbol.Type != null)
        {
          TypeSymbolNode tsn = new Symbols.TypeSymbolNode(symbol.Type, esn);
          tsn.IsDeclaration = false;
        }
      }

      public override void VisitProperty(IPropertySymbol symbol)
      {
        SymbolNode parent = m_parentNodes.Peek();

        PropertySymbolNode psn = new PropertySymbolNode(symbol, parent);
        psn.IsDeclaration = true;

        if (symbol.Type != null)
        {
          TypeSymbolNode tsn = new Symbols.TypeSymbolNode(symbol.Type, psn);
          tsn.IsDeclaration = false;
        }
      }

      public void Build(Project project)
      {
        m_parentNodes.Clear();

        CSharpCompilation compilation = (CSharpCompilation)project.GetCompilationAsync().Result;
        
        this.VisitNamespace(compilation.GlobalNamespace);
        //Itt már megvan minden saját deklarált szimbólum, ami metódus mellett vagy azon kívül előfordulhat
        //A többi szimbólum már csak a metódusok belsejében létezhet, azon kívül nem.

        //SyntaxTree - SemanticModel cache, hogy ne kelljen újraszámítani folyamatosan.
        //Solution-ön belül vagyunk (ugyanaz a compilation!) - a fájlnévvel azonosíthatjuk itt a SyntaxTree-ket
        //Hogy a cross-project reference-ek is működjenek, becache-elem a többi projectet is!

        List<Tuple<string, SyntaxTree, SemanticModel, SyntaxNode>> cache = new List<Tuple<string, SyntaxTree, SemanticModel, SyntaxNode>>();

        foreach (Project p in project.Solution.Projects)
        {
          CSharpCompilation comp = (CSharpCompilation)p.GetCompilationAsync().Result;
          foreach (SyntaxTree st in comp.SyntaxTrees)
          {
            SemanticModel model = comp.GetSemanticModel(st, false);
            cache.Add(new Tuple<string, SyntaxTree, SemanticModel, SyntaxNode>(st.FilePath, st, model, st.GetRoot()));
          }
        }

        //Végig kell menni minden metódus törzsén, és az ott lévő szimbólumokat is begyűjteni
        //Az enumerált node-ok referenciáit nem manipulálom
        var methods = m_root.Where(sn => sn.InnerSymbol.Kind == SymbolKind.Method);
        foreach (MethodSymbolNode node in methods)
        {
          foreach (SyntaxReference syntaxRef in node.InnerSymbol.DeclaringSyntaxReferences)
          {
            ControlFlowAnalysis cfa = null;
            DataFlowAnalysis dfa = null;
            Tuple<string, SyntaxTree, SemanticModel, SyntaxNode> item = cache.Single(t => t.Item1 == syntaxRef.SyntaxTree.FilePath);
            
            SyntaxNode referredSyntax = syntaxRef.GetSyntax();

            //Perform CFA
            BlockSyntax snCfa = referredSyntax.DescendantNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (snCfa != null)
            {
              cfa = item.Item3.AnalyzeControlFlow(snCfa);
            }
            else
            {
              StatementSyntax snCfaFrom = referredSyntax.DescendantNodes().OfType<StatementSyntax>().FirstOrDefault();
              StatementSyntax snCfaTo = referredSyntax.DescendantNodes().OfType<StatementSyntax>().LastOrDefault();
              if (snCfaFrom != null && snCfaTo != null)
              {
                cfa = item.Item3.AnalyzeControlFlow(snCfaFrom, snCfaTo);
              }
            }

            if (cfa != null)
            {
              node.EndPointIsReachable = cfa.EndPointIsReachable;
              node.ExitPointsCount = cfa.ExitPoints.Length;
              node.ReturnStatementsCount = cfa.ReturnStatements.Length;
              node.StartPointIsReachable = cfa.StartPointIsReachable;
              node.StatementsCount = referredSyntax.DescendantNodes().OfType<StatementSyntax>().Count();
              node.BlocksCount = referredSyntax.DescendantNodes().OfType<BlockSyntax>().Count();
            }

            //Perform DFA
            SyntaxNode snDfa = referredSyntax.DescendantNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (snDfa == null)
            {
              snDfa = referredSyntax.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault();
            }
            if (snDfa != null)
            {
              dfa = item.Item3.AnalyzeDataFlow(snDfa);
            }

            if (dfa != null)
            {
              foreach (ISymbol variable in dfa.VariablesDeclared)
              {
                switch (variable.Kind)
                {
                  case SymbolKind.Local:
                    {
                      LocalSymbolNode lsn = new LocalSymbolNode(variable as ILocalSymbol, node);
                      lsn.IsDeclaration = true;
                      lsn.AlwaysAssigned = dfa.AlwaysAssigned.Contains(variable);
                      lsn.Captured = dfa.Captured.Contains(variable);
                      lsn.DataFlowsIn = dfa.DataFlowsIn.Contains(variable);
                      lsn.DataFlowsOut = dfa.DataFlowsOut.Contains(variable);
                      lsn.ReadInside = dfa.ReadInside.Contains(variable);
                      lsn.ReadOutside = dfa.ReadOutside.Contains(variable);
                      lsn.WrittenInside = dfa.WrittenInside.Contains(variable);
                      lsn.WrittenOutside = dfa.WrittenOutside.Contains(variable);
                      
                      if (lsn.InnerSymbol != null && lsn.InnerSymbol.Type != null)
                      {
                        TypeSymbolNode tsn = new Symbols.TypeSymbolNode(lsn.InnerSymbol.Type, lsn);
                        tsn.IsDeclaration = false;
                      }
                      break;
                    }
                  case SymbolKind.Parameter:
                    {
                      ParameterSymbolNode psn = new ParameterSymbolNode(variable as IParameterSymbol, node);
                      psn.IsDeclaration = true;
                      psn.AlwaysAssigned = dfa.AlwaysAssigned.Contains(variable);
                      psn.Captured = dfa.Captured.Contains(variable);
                      psn.DataFlowsIn = dfa.DataFlowsIn.Contains(variable);
                      psn.DataFlowsOut = dfa.DataFlowsOut.Contains(variable);
                      psn.ReadInside = dfa.ReadInside.Contains(variable);
                      psn.ReadOutside = dfa.ReadOutside.Contains(variable);
                      psn.WrittenInside = dfa.WrittenInside.Contains(variable);
                      psn.WrittenOutside = dfa.WrittenOutside.Contains(variable);

                      if (psn.InnerSymbol != null && psn.InnerSymbol.Type != null)
                      {
                        TypeSymbolNode tsn = new Symbols.TypeSymbolNode(psn.InnerSymbol.Type, psn);
                        tsn.IsDeclaration = false;
                      }
                      break;
                    }
                }
              }
            }
          }
        }


        //Find all references mindenre, hogy felépítsük a nem-deklarált szimbólumokra mutató referenciákat
        var declaredSymbols = m_root.Where(sn => sn.IsDeclaration);
        foreach (SymbolNode node in declaredSymbols)
        {
          //node: ennek keressük a referenciáit
          //foundIn: ezek azok a symbolok, amelyben node-ra mutató ref. található
          HashSet<ISymbol> foundIn = new HashSet<ISymbol>();

          var refs = SymbolFinder.FindReferencesAsync(node.InnerSymbol, project.Solution).Result;

          foreach (ReferencedSymbol refd in refs)
          {
            foreach (ReferenceLocation refloc in refd.Locations)
            {
              SyntaxTree st = refloc.Location.SourceTree;
              Tuple<string, SyntaxTree, SemanticModel, SyntaxNode> item = cache.Single(t => t.Item1 == st.FilePath);

              //Ez a node egy symbol belsejében van - meg kell mondani, hogy melyikben!
              SyntaxNode l = item.Item4.FindNode(refloc.Location.SourceSpan);
              ISymbol s = item.Item3.GetEnclosingSymbol(refloc.Location.SourceSpan.Start);
              if (s != null)
                foundIn.Add(s);
            }
          }

          //foundIn alapján megkeresem a szimbólumhoz tartozó SymbolNode-ot, és hozzáadom gyereknek.
          foreach (ISymbol refSymbol in foundIn)
          {
            //refSymbol: ezen belül van node-ra mutató referencia

            //Keresés - melyik szimbólummal egyezik meg?
            SymbolNode containerSn = m_root.FirstOrDefault(n => n.InnerSymbol == refSymbol);

            if (containerSn != null)
            {
              //containerSn: tkp = refSymbol
              switch (node.InnerSymbol.Kind)
              {
                case SymbolKind.Method:
                  new MethodSymbolNode(node.InnerSymbol as IMethodSymbol, containerSn, null);
                  break;
                case SymbolKind.Event:
                  new EventSymbolNode(node.InnerSymbol as IEventSymbol, containerSn, null);
                  break;
                case SymbolKind.Field:
                  new FieldSymbolNode(node.InnerSymbol as IFieldSymbol, containerSn, null);
                  break;
                case SymbolKind.NamedType:
                  new NamedTypeSymbolNode(node.InnerSymbol as INamedTypeSymbol, containerSn, null);
                  break;
                case SymbolKind.Property:
                  new PropertySymbolNode(node.InnerSymbol as IPropertySymbol, containerSn, null);
                  break;
                case SymbolKind.Local:
                  new LocalSymbolNode(node.InnerSymbol as ILocalSymbol, containerSn, null);
                  break;
                case SymbolKind.Parameter:
                  new ParameterSymbolNode(node.InnerSymbol as IParameterSymbol, containerSn, null);
                  break;
              }
            }
          }
        }

      }

      public InternalSymbolTreeBuilder() : base()
      {
        m_parentNodes = new Stack<Symbols.SymbolNode>();
      }
    }

    public SymbolTree ToSymbolTree()
    {
      return new SymbolTree(m_builder.Root);
    }

    public SymbolTree ToCachedSymbolTree()
    {
      return new SymbolTree(m_builder.Root, true);
    }

    public void Build(Project project)
    {
      m_builder.Build(project);
    }

    public SymbolTreeBuilder()
    {
      m_builder = new Symbols.SymbolTreeBuilder.InternalSymbolTreeBuilder();
    }
  }
}
