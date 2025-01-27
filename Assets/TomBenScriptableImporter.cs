using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using UnityEngine.Rendering.HighDefinition;
using System.Text.RegularExpressions;
using System.Collections.Generic;


[ScriptedImporter(1, "TomBen")]
public class TomBenScriptableImporter : ScriptedImporter
{

    private string charBuffer;
    private int charIndex;
    string ContentToParse;
    private ParserState state;

    private List<string> Types;

    private AssetImportContext context;

   public struct ParsedBlock
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
	public override void OnImportAsset(AssetImportContext ctx)
	{
        context = ctx;
		string filetext = File.ReadAllText(context.assetPath);
        ChangeState(ParserState.Outside);
		Debug.Log(filetext);
		ParseText(filetext);
    }
    public void ParseText(string fileContent)
    {

        Debug.Log("ParseStarted!");
        ContentToParse = fileContent;

        while (!ReachedEnd())
        {
            NextChar();
            switch (state)
            {
                case ParserState.ParsingHeader:
                    {
                        //Debug.Log("parsing Header");
                        if (ReachedEnd()) 
                        { 
                            string[] chunks = charBuffer.Split("_Ben");

                            foreach (string block in chunks)
                            {
                                Debug.Log($"{block}");
                                if (block.Contains("type"))
                                {
                                    string[] chunks2 = block.Split("-");
                                    ParsedBlock ParsedType = new ParsedBlock();
                                    //Debug.Log("Contains type");
                                    ParsedType.Type = chunks2[0];
                                    chunks2 = chunks2[1].Split("(");
                                    ParsedType.ID = int.Parse(chunks2[0]);
                                    chunks2 = chunks2[1].Split(")");
                                    ParsedType.Name = chunks2[0];
                                    ParsedType.content = chunks2[1];

                                    type(ParsedType.ID,ParsedType.Name,ParsedType.content);
                                    
                                }
                                else if (block.Contains("cluster"))
                                {
                                    string[] chunks2 = block.Split("-");
                                    ParsedBlock ParsedCluster = new ParsedBlock();
                                    ParsedCluster.Type = chunks2[0];
                                    chunks2 = chunks2[1].Split("(");
                                    ParsedCluster.ID = int.Parse(chunks2[0]);
                                    chunks2 = chunks2[1].Split(")");
                                    ParsedCluster.Name = chunks2[0];
                                    ParsedCluster.content = chunks2[1];

                                    cluster(ParsedCluster.ID, ParsedCluster.Name, ParsedCluster.content);
                                }
                                else if (block.Contains("wave"))
                                {
                                    string[] chunks2 = block.Split("-");
                                    ParsedBlock ParsedWave = new ParsedBlock();
                                    ParsedWave.Type = chunks2[0];
                                    chunks2 = chunks2[1].Split("(");
                                    ParsedWave.ID = int.Parse(chunks2[0]);
                                    chunks2 = chunks2[1].Split(")");
                                    ParsedWave.Name = chunks2[0];
                                    ParsedWave.content = chunks2[1];

                                    wave(ParsedWave.ID, ParsedWave.Name, ParsedWave.content); 
                                }
                                else
                                {
                                    Debug.Assert(false, "Invalid Type");
                                }
                            }
                            charIndex = 0;
                            ChangeState(ParserState.ParsingBlockBody);
                            
                        }
                        break;
                    }
                case ParserState.ParsingBlockBody:
                    {

                        break;
                    }
                case ParserState.Outside:
                    {
                        if (BufferHasAny("type", "cluster", "wave"))
                        {
                            charIndex = 0;
                            ChangeState(ParserState.ParsingHeader);
                        }

                        break;
                    }
            }
        }
    }
    
    private void type(int ID, string Name, string Content)
    {
        Debug.Log(ToString());
    }
    private void wave(int ID, string Name, string Content)
    {
        Debug.Log(ID + Name + Content);
    }
    
    private void cluster(int ID, string Name, string Content)
    {
        Debug.Log(ID + Name + Content);
    }

    private void ClearBuffer() => charBuffer = "";

    private bool ReachedEnd() => charIndex >= ContentToParse.Length;

    private char NextChar()
    {
        charBuffer += ContentToParse[charIndex];
        return ContentToParse[charIndex++];
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


