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

    private List<ParsedBlock> Types;
    private List<ParsedBlock> Waves;
    private List<ParsedBlock> Clusters;

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
        Holder ScriptableHolder = ScriptableObject.CreateInstance<Holder>();
        context.AddObjectToAsset("Holder",ScriptableHolder);
        context.SetMainObject(ScriptableHolder);

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
                                    ParsedBlock parsedType = new ParsedBlock();
                                    //Debug.Log("Contains type");
                                    parsedType.Type = chunks2[0];
                                    chunks2 = chunks2[1].Split("(");
                                    parsedType.ID = int.Parse(chunks2[0]);
                                    chunks2 = chunks2[1].Split(")");
                                    parsedType.Name = chunks2[0];
                                    parsedType.content = chunks2[1];

                                    type(parsedType.ID,parsedType.Name,parsedType.content);
                                    //Types.Add(parsedType);
                                    
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
                                    //Clusters.Add(ParsedCluster);
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
                                    //Waves.Add(ParsedWave);
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
        
        Regex Pattern = new Regex("([CT])(\\d)(\\<\\d\\>)?(\\[\\d\\])?");


        for (int a = 0; a < WaveBlocks.Length; a++)
        {
            Wave WaveType = ScriptableObject.CreateInstance<Wave>();

            WaveType.ID = ID;
            WaveType.name = Name;
            Match waveGroups = Pattern.Match(WaveBlocks[a]);

            if (waveGroups.Success)
            {
                Wave.waveData ParsedWave = new Wave.waveData();
                if (waveGroups.Groups[1].Value == "C")
                {
                    ParsedWave.isCluster = true;
                }

                    ParsedWave.ID = int.Parse(waveGroups.Groups[2].Value);
                //ParsedWave.SpawnTime = int.Parse(waveGroups.Groups[3].Value);
                //ParsedWave.PopulationDensity = int.Parse(waveGroups.Groups[4].Value);
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


