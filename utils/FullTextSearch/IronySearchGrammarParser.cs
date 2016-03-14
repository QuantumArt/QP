using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Quantumart.QP8.Utils.FullTextSearch
{
	/// <summary>
	/// Реализация <see cref="ISearchGrammarParser"/> с использованием Irony
	/// </summary>
	public class IronySearchGrammarParser : ISearchGrammarParser
	{
		Parser _parser;
		IStopWordList stopList;

		public IronySearchGrammarParser(IStopWordList stopList)
		{
			if (stopList == null)
				throw new ArgumentNullException("stopList");
			this.stopList = stopList;

			_parser = new Parser(new LanguageData(new IronySearchGrammar()));
		}

		/// <summary>
		/// <see cref="IronySearchGrammarParser.TryParse"/>
		/// </summary>        
		public bool TryParse(string queryString, out string result)
		{
			result = null;
			if (String.IsNullOrWhiteSpace(queryString))
				return true;

			var parserTree = _parser.Parse(queryString);
			if (parserTree.Status != ParseTreeStatus.Parsed || parserTree.Root == null)
				return false;

			result = ConvertQuery(parserTree.Root);
			return true;
		}

		private string ConvertQuery(ParseTreeNode node)
		{
			return ConvertQuery(node, IronyTermType.Inflectional);
		}

		private string ConvertQuery(ParseTreeNode node, IronyTermType type)
        {
            string result = "";
			string p1, p2;
            // Note that some NonTerminals don't actually get into the AST tree, 
            // because of some Irony's optimizations - punctuation stripping and 
            // transient nodes elimination. For example, ParenthesizedExpression - parentheses 
            // symbols get stripped off as punctuation, and child expression node 
            // (parenthesized content) replaces the parent ParenthesizedExpression node
            switch (node.Term.Name)
            {
                case "BinaryExpression":
					string opSym = string.Empty;
					string op = node.ChildNodes[1].FindTokenAndGetText().ToLower();
					string sqlOp = "";
					switch (op)
					{
						case "":
						case "&":
						case "and":
							sqlOp = " AND ";
							type = IronyTermType.Inflectional;
							break;
						case "-":
							sqlOp = " AND NOT ";
							break;
						case "|":
						case "or":
							sqlOp = " OR ";
							break;
					}

					p1 = ConvertQuery(node.ChildNodes[0], type);
					p2 = ConvertQuery(node.ChildNodes[2], type);
					if (String.IsNullOrEmpty(p1) && String.IsNullOrEmpty(p2))
						result = null;
					else if (String.IsNullOrEmpty(p1))
						result = "(" + p2 + ")";
					else if (String.IsNullOrEmpty(p2))
						result = "(" + p1 + ")";
					else
						result = "(" + p1 + sqlOp + p2 + ")";
					break;

                case "PrimaryExpression":
					p1 = ConvertQuery(node.ChildNodes[0], type);					
                    result = String.IsNullOrEmpty(p1) ? null : "(" + p1 + ")";
                    break;

                case "ProximityList":
                    type = IronyTermType.Exact;

                    string[] tmp = new string[node.ChildNodes.Count];
                    for (int i = 0; i < node.ChildNodes.Count; i++)
                    {
                        tmp[i] = ConvertQuery(node.ChildNodes[i], type);
                    }                    
					var pList = tmp.Where(t => !String.IsNullOrEmpty(t)).ToArray();
					if (pList.Length == 0)
						result = null;
					else
						result = "(" + string.Join(" NEAR ", pList.Select(t => String.Format("\"{0}\"", t))) + ")";
                    type = IronyTermType.Inflectional;
                    break;

                case "Phrase":
                    result = '"' + node.Token.ValueString + '"';
                    break;

                case "ThesaurusExpression":                    
                    result = " FORMSOF (THESAURUS, \"" + node.ChildNodes[1].Token.ValueString + "\") ";
                    break;

                case "ExactExpression":
                    result = " \"" + node.ChildNodes[1].Token.ValueString + "\" ";
                    break;

                case "Term":
                    switch (type)
                    {
                        case IronyTermType.Inflectional:
                            result = node.Token.ValueString;
                            if (result.EndsWith("*"))
                                result = "\"" + result + "\"";
                            else                                
                                result = !stopList.ContainsWord(result) ? " FORMSOF (INFLECTIONAL, \"" + result + "\") " : null;
                            break;
                        case IronyTermType.Exact:
                            result = node.Token.ValueString;

                            break;
                    }
                    break;

                // This should never happen, even if input string is garbage
                default:
                    throw new ApplicationException("Converter failed: unexpected term: " +
                        node.Term.Name + ". Please investigate.");

            }
            return result;
        }
	}
}
