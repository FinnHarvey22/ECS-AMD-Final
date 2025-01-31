using System;
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

    private int bodyIndex;

    private bool finishedParse;

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
        
        Debug.Log(state);
		Debug.Log(filetext);
        Types = new List<ParsedBlock>();
        Holder scriptableHolder = ScriptableObject.CreateInstance<Holder>();
        context.AddObjectToAsset("Holder", scriptableHolder);
        context.SetMainObject(scriptableHolder);
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

                    Regex headerPattern = new Regex("(type|wave|cluster)\\s*-\\s*(\\d+)\\s*\\(([\\w\\s]+)\\)\\s*");
                    Match headerBlocks = headerPattern.Match(charBuffer);

                    if (headerBlocks.Success)
                    {
                        ClearBuffer();
                        while (!BufferHas("_Ben"))
                        {
                            NextChar();
                        }

                        charBuffer = charBuffer[0..^4];

                        ParsedBlock parsedHeaders = new ParsedBlock()
                        {
                            Type = headerBlocks.Groups[1].Value,
                            Name = headerBlocks.Groups[3].Value,
                            ID = int.Parse(headerBlocks.Groups[2].Value),
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
                            Debug.Log("ParsingBody");
                            while (!finishedParse)
                            {
                                Debug.Log(charIndex);
                                NextChar();
                                if (ReachedEnd())
                                {
                                    finishedParse = true;
                                }
                            }

                            string[] typeChunks = ContentToParse.Split("!?");

                            float health = 0;
                            float speed = 0;
                            float damage = 0;


                            Debug.Log(charBuffer);

                            Regex typePattern = new Regex("(health|speed|damage)=>(\\d+)");

                            for (int i = 0; i > typeChunks.Length - 1; i++)
                            {

                                Match typeBlocks = typePattern.Match(charBuffer);

                                if (typeBlocks.Success)
                                {
                                    Debug.Log($"Header block = {typeBlocks.Groups[1].Value}");
                                    if (typeBlocks.Groups[1].Value == "health")
                                    {
                                        health = float.Parse(typeBlocks.Groups[2].Value);
                                        ClearBuffer();
                                    }
                                    else if (typeBlocks.Groups[1].Value == "speed")
                                    {
                                        speed = float.Parse(typeBlocks.Groups[2].Value);
                                        ClearBuffer();
                                    }
                                    else if (typeBlocks.Groups[1].Value == "damage")
                                    {
                                        damage = float.Parse(typeBlocks.Groups[2].Value);
                                        ClearBuffer();
                                    }

                                }
                               
                                Debug.Log("Reached End");
                                ClearBuffer();
                                charIndex = 0;
                                Enemy enemyType = ScriptableObject.CreateInstance<Enemy>();
                                enemyType.ID = Types[bodyIndex].ID;
                                enemyType.enemyName = Types[bodyIndex].Name;
                                enemyType.health = health;
                                enemyType.speed = speed;
                                enemyType.damage = damage;
                                context.AddObjectToAsset("enemyObject", enemyType);
                                
                            }

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
                        Debug.Log(charBuffer);
                    }

                    charIndex = 0;
                    ChangeState(ParserState.ParsingHeader);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        ParseBody();
        
    }

    private void ParseBody()
    {
       
        ChangeState(ParserState.ParsingBlockBody);
        charIndex = 0;
        for (bodyIndex = 0; bodyIndex < Types.Count; bodyIndex++)
        {
            if (Types[bodyIndex].Type == "type") statesForBlocks = BlockState.type;
            else if (Types[bodyIndex].Type == "cluster") statesForBlocks = BlockState.cluster;
            else if (Types[bodyIndex].Type == "wave") statesForBlocks = BlockState.wave;
            ParseText(Types[bodyIndex].content);
        }

        finishedParse = true;
    }
    private void type(int ID, string Name, string Content)
    {
        Enemy enemyType = ScriptableObject.CreateInstance <Enemy>();
        enemyType.ID = ID;
        enemyType.enemyName = Name;
        
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
        Debug.Log(state);
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


