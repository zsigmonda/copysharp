using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Immutable;
using CopySharp.BusinessLogic.Fingerprinting;

namespace CopySharp.BusinessLogic.Symbols
{
  public class SymbolTree : IEnumerable<SymbolNode>, IFingerprintableGraph
  {
    private SymbolNode m_rootNode;
    private ImmutableArray<SymbolNode> m_cache;

    internal SymbolTree(SymbolNode root, bool cached = false)
    {
      m_rootNode = root;
      if (cached)
      {
        m_cache = root.ToImmutableArray();
      }
    }

    public IEnumerator<SymbolNode> GetEnumerator()
    {
      if (m_cache != null)
      {
        return new SymbolTreeEnumerator(m_cache);
      }
      else
      {
        return new SymbolNodeEnumerator(m_rootNode);
      }
    }

    public SymbolNode Root
    {
      get { return m_rootNode; }
    }

    public bool IsCached
    {
      get { return m_cache != null; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerable<IList<IFingerprintableVertex>> GetAllPathsWithMaximumLength(int maxLength)
    {
      List<List<IFingerprintableVertex>> allPaths = new List<List<IFingerprintableVertex>>();

      foreach (SymbolNode node in this)
      {
        allPaths.AddRange(PathsWithMaximumLength(maxLength, node));
      }

      return allPaths;
    }

    private List<List<IFingerprintableVertex>> PathsWithMaximumLength(int length, SymbolNode startNode)
    {
      List<List<IFingerprintableVertex>> paths = new List<List<IFingerprintableVertex>>();

      //Útvonal, amit bejárunk
      Stack<SymbolNode> path = new Stack<Symbols.SymbolNode>();
      
      //Az útvonal egy elemén mutatja, hogy melyik elem felé mentünk
      //-1: még sehova, 0..childCount-1: - gyerek, childCount: szülő
      Stack<int> position = new Stack<int>();

      path.Push(startNode);
      position.Push(-1);
      
      //Gyökérelem bekerül
      paths.Add(new List<IFingerprintableVertex>() { startNode });

      while (path.Count > 0)
      {
        //Ha elfogyott minden bejárható elem (gyerekek + szülő), vagy elég hosszú az útvonal, elindulunk felfelé.
        while (path.Count == length || (position.Count > 0 && path.Peek().Children.Count == position.Peek()))
        {
          path.Pop();
          position.Pop();
        }

        //Ha még nem jártuk be az egész területet
        if (path.Count > 0)
        {
          SymbolNode node = path.Peek();

          //Elmozdulunk oldalra - lehet hova, hiszen fentebb szűrtünk.
          int childIdx = position.Pop();
          position.Push(++childIdx);

          //van gyerek
          if (node.Children.Count > childIdx)
          {
            if (!path.Contains(node.Children[childIdx]))
            {
              node = node.Children[childIdx];
              position.Push(-1);
              path.Push(node);

              //Látogatás történt - biztosan: path.Count <= length (különben visszaléptük volna)
              List<IFingerprintableVertex> tempList = new List<IFingerprintableVertex>();
              tempList.AddRange(path.ToList());
              paths.Add(tempList);
            }
          }
          else
          {
            //elfogyott a gyerek, jöhet a szülő
            if (node.Children.Count == childIdx)
            {
              if (node.Parent != null && !path.Contains(node.Parent))
              {
                node = node.Parent;
                position.Push(-1);
                path.Push(node);

                //Látogatás történt - biztosan: path.Count <= length (különben visszaléptük volna)
                List<IFingerprintableVertex> tempList = new List<IFingerprintableVertex>();
                tempList.AddRange(path.ToList());
                paths.Add(tempList);
              }
            }
          }
        }
      }

      return paths;
    }
  }
}
