using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;


[ScriptedImporter(1, "TomBen")]
public class TomBenScriptableImporter : ScriptedImporter
{

    private string charBuffer;
    private int charIndex;
    string ContentToParse;
    private ParserState state;
    private BlockState statesForBlocks;

    private List<ParsedBlock> Types;

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

    enum BlockState
    {
        type,
        cluster,
        wave,
    }
	public override void OnImportAsset(AssetImportContext ctx)
	{
        context = ctx;
		string filetext = File.ReadAllText(context.assetPath);
        ChangeState(ParserState.Outside);
		Debug.Log(filetext);
        Types = new List<ParsedBlock>();
        Holder ScriptableHolder = ScriptableObject.CreateInstance<Holder>();
        context.AddObjectToAsset("Holder", ScriptableHolder);
        context.SetMainObject(ScriptableHolder);
		ParseText(filetext);
    }

    public void ParseText(string fileContent)
    {

        ContentToParse = fileContent;

        while (!ReachedEnd())
        {
            switch (state)
            {
                case ParserState.ParsingHeader:
                {
                    Debug.Log("HeaderParseStarted!");
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }

                    charBuffer = charBuffer[0..^4];
                    Debug.Log(charBuffer);

                    Regex HeaderPattern = new Regex("(type|wave|cluster)\\s*-\\s*(\\d+)\\s*\\(([\\w\\s]+)\\)\\s*");
                    Match HeaderBlocks = HeaderPattern.Match(charBuffer);

                    if (HeaderBlocks.Success)
                    {
                        ClearBuffer();
                        while (!BufferHas("_Ben"))
                        {
                            NextChar();
                        }

                        charBuffer = charBuffer[0..^4];

                        ParsedBlock parsedHeaders = new ParsedBlock()
                        {
                            Type = HeaderBlocks.Groups[1].Value,
                            Name = HeaderBlocks.Groups[3].Value,
                            ID = int.Parse(HeaderBlocks.Groups[2].Value),
                            content = charBuffer,

                        };
                        Types.Add(parsedHeaders);
                        Debug.Log(Types.Count);

                    }
                    //Debug.Log("parsing Header");

                    break;
                }
                case ParserState.ParsingBlockBody:
                {
                    switch (statesForBlocks)
                    {
                        case BlockState.type:
                        {
                            break;
                        }
                        case BlockState.wave:
                        {
                            break;
                        }
                        case BlockState.cluster:
                        {
                            break;
                        }
                        
                    }

                    break;
                }
                case ParserState.Outside:
                {
                    Debug.Log("ParseStarted!");
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }

                    charIndex = 0;
                    ChangeState(ParserState.ParsingHeader);

                    break;
                }
            }

        }
        ParseBody();
        
    }

    private void ParseBody()
    {
        ChangeState(ParserState.ParsingBlockBody);
        charIndex = 0;
        for (int i = 0; i < Types.Count; i++)
        {
            if (Types[i].Type == "type") this.statesForBlocks = BlockState.type;
            else if (Types[i].Type == "cluster") statesForBlocks = BlockState.cluster;
            else if (Types[i].Type == "wave") statesForBlocks = BlockState.wave;
            ParseText(Types[i].content);
        }
    }
    private void type(int ID, string Name, string Content)
    {
        Enemy enemyType = ScriptableObject.CreateInstance <Enemy>();
        enemyType.ID = ID;
        enemyType.enemyName = Name;
        string[] ContentBlocks = Content.Split("!?");
        foreach (string block in ContentBlocks)
        {
            ContentBlocks = block.Split("=>");
            if (ContentBlocks[0].Contains("health"))
            {
                enemyType.health = float.Parse(ContentBlocks[1]);
            }
            else if (ContentBlocks[0].Contains("speed"))
            {
                enemyType.speed = float.Parse(ContentBlocks[1]);
            }
            else if (ContentBlocks[0].Contains("damage"))
            {
                enemyType.damage = float.Parse(ContentBlocks[1]);
            }
        }
        context.AddObjectToAsset("enemyObject", enemyType);


    }
    private void wave(int ID, string Name, string Content)
    {

        string[] WaveBlocks = Content.Split("!?");
        
        Regex Pattern = new Regex("([CT])(\\d)\\<?(\\d)?\\>?\\[?(\\d)?\\]?");
        
        
        Debug.Log(WaveBlocks.Length);
        


        for (int a = 0; a < WaveBlocks.Length - 1; a++)
        {
            Wave WaveType = ScriptableObject.CreateInstance<Wave>();

            WaveType.ID = ID;
            WaveType.Name = Name;
            Match waveGroups = Pattern.Match(WaveBlocks[a]);
            
            

            if (waveGroups.Success)
            {
                Debug.Log($"{waveGroups.Success}");
                if (waveGroups.Groups[1].Value == "C")
                {
                    WaveType.AddData(true, int.Parse(waveGroups.Groups[2].Value), waveGroups.Groups[3].Value ,waveGroups.Groups[4].Value);
                   
                }
                else if (waveGroups.Groups[1].Value == "T")
                {
                    WaveType.AddData(false, int.Parse(waveGroups.Groups[2].Value), waveGroups.Groups[3].Value , waveGroups.Groups[4].Value);

                }
                
           
            }
            
            context.AddObjectToAsset("waveObject", WaveType);
        }
    }
    
    
    private void cluster(int ID, string Name, string Content)
    {
        Cluster ClusterType = ScriptableObject.CreateInstance<Cluster>();
        ClusterType.ID = ID;
        ClusterType.clusterName = Name;


        string[] ClusterBlocks = Content.Split(("_Tom"));
        ClusterBlocks = ClusterBlocks[1].Split("!?");
        ClusterBlocks = ClusterBlocks[0].Split(":");
        ClusterType.Type = int.Parse(ClusterBlocks[0]);
        ClusterType.AmountToSpawn = int.Parse(ClusterBlocks[1]);
        
        context.AddObjectToAsset("clusterObject", ClusterType);
        
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


