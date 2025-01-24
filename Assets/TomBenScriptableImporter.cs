using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "TomBen")]
public class TomBenParser : MonoBehaviour
{
	private string charBuffer;
	private int charIndex;
	string ContentToParse;
	private ParserState state;

	struct ParsedBlock
	{
		public string Type;
		public string Name;
		public int ID;
		public string content;

		public override string ToString() => $"ParsedBlock(Type={Type}, ID={ID},Name=={Name}, content={content})";
		
	}

	enum ParserState
	{
		Outside,
		ParsingHeader,
		ParsingBlockBody,
	};
	public void ParseText(string fileContent)
	{
		ContentToParse = fileContent;

		while (!ReachedEnd())
		{
			NextChar();
			if (BufferHasAny("type", "cluster", "wave", "_Tom", "_Wave"))
			{

			}
		}

		switch(state)
		{
			case ParserState.ParsingHeader:
				{
					break;
				}
			case ParserState.ParsingBlockBody:
				{
					break;
				}
			case ParserState.Outside:
				{
					break;
				}
		}

	}

	private void ClearBuffer() => charBuffer = "";

	private bool ReachedEnd() => charIndex >= ContentToParse.Length;

	private char NextChar()
	{
		charBuffer += ContentToParse[charIndex];
		return ContentToParse[charIndex ++];
	}

	private void ChangeState(ParserState state)
	{
		this.state = state;
		ClearBuffer();
	}

	private bool BufferHas(string token) => charBuffer.EndsWith(token);

	private bool BufferHasAny(params string[] tokens)
	{
		foreach (var token in tokens)
		{
			if (BufferHas(token))
			{
				return true;
			}
		}
		return false;
	}

	

} 

public class TomBenScriptableImporter : ScriptedImporter
{
	TomBenParser tomBenParser;
	public override void OnImportAsset(AssetImportContext ctx)
	{
		string filetext = File.ReadAllText(ctx.assetPath);

		tomBenParser.ParseText(filetext);


		ctx.AddObjectToAsset("enemyObject", enemyInstance);
		ctx.SetMainObject(enemyInstance);
	}

	
}


