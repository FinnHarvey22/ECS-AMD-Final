using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


[ScriptedImporter(1, "TomBen")]
public class TomBenScriptableImporter : ScriptedImporter
{

    private string charBuffer;
    private int charIndex;
    string ContentToParse;
    private ParserState state;
    private BlockState statesForBlocks;
    private bool DuplicateData;

    private List<ParsedBlock> Types;
    private List<Waves> WavesList;
    private List<Clusters> ClustersList;

    private Dictionary<int, Enemies> _enemyDictionary = new Dictionary<int, Enemies>();
    private Dictionary<int, Waves> _wavesDictionary = new Dictionary<int, Waves>();
    private Dictionary<int, Clusters> _clustersDiction = new Dictionary<int, Clusters>();
    

    private List<ScriptableObject> TypesSO;
    private List<ScriptableObject> ClusterSO;
    private List<ScriptableObject> WavesSO;


    private Holder scriptableHolder;
    

    private AssetImportContext context;

    private int bodyIndex;

    private bool finishedParse;

   private struct ParsedBlock
    {
        public string Type;
        public string Name;
        public int ID;
        public string content;

        public override string ToString() => $"ParsedBlock(Type={Type}, ID={ID},Name=={Name}, content={content})";
    }

    private struct Enemies
    {
        public int ID;
        public string EnemyName;
        public float Health;
        public float Speed;
        public float Damage;
    }
    
    [System.Serializable]
    public struct Waves
    {
        public int ID;
        public string Name;
        public List<WaveData> _dataForWaves;
        public struct WaveData
        {
            public bool IsCluster;
            public int ID;
            public float SpawnTime;
            public int PopulationDensity;
        }
    }

    private struct Clusters
    {
        public int ID;
        public string ClusterName;
        public int Type;
        public int AmountToSpawn;
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
		string fileText = File.ReadAllText(context.assetPath);
        ChangeState(ParserState.Outside);
        
        Debug.Log(state);
		Debug.Log(fileText);
        Types = new List<ParsedBlock>();
        TypesSO = new List<ScriptableObject>();
        ClusterSO = new List<ScriptableObject>();
        WavesSO = new List<ScriptableObject>();
        
        ClustersList = new List<Clusters>();
        WavesList = new List<Waves>();
        
        scriptableHolder = ScriptableObject.CreateInstance<Holder>();
        context.AddObjectToAsset("Holder", scriptableHolder);
        context.SetMainObject(scriptableHolder);
		ParseText(fileText);
    }

    private void ParseText(string fileContent)
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
                        Debug.Log(charBuffer);
                        NextChar();
                    }
                    Debug.Log("Found Tom");

                    charBuffer = charBuffer[0..^4];
                    Debug.Log(charBuffer);

                    Regex headerPattern = new Regex("(type|wave|cluster)\\s*-\\s*(\\d+)\\s*(\\(([\\w\\s]+)\\))?\\s*");
                    Match headerBlocks = headerPattern.Match(charBuffer);

                    if (headerBlocks.Success)
                    {
                        ClearBuffer();
                        while (!BufferHas("_Ben"))
                        {
                            NextChar();
                        }
                        
                        
                        Debug.Log("Found Ben");

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
                        ClearBuffer();
                        

                    }


                    //Debug.Log("parsing Header");

                    break;
                }
                case ParserState.ParsingBlockBody:
                {
                    break;
                }
                case ParserState.Outside:
                {
                    Debug.Log("ParseStarted!");
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }
                    Debug.Log("Found Tom");

                    charIndex = 0;
                    ChangeState(ParserState.ParsingHeader);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        FinishFirstParse();
        
    }

    private void FinishFirstParse()
    {
       
        ChangeState(ParserState.ParsingBlockBody);
        charIndex = 0;
        for (bodyIndex = 0; bodyIndex < Types.Count; bodyIndex++)
        {
            if (Types[bodyIndex].Type == "type") statesForBlocks = BlockState.type;
            else if (Types[bodyIndex].Type == "cluster") statesForBlocks = BlockState.cluster;
            else if (Types[bodyIndex].Type == "wave") statesForBlocks = BlockState.wave;
            //ParseText(Types[bodyIndex].content);
            
            ParseBody(Types[bodyIndex].ID, Types[bodyIndex].Name, Types[bodyIndex].content);
        }

        finishedParse = true;
    }

    private void ParseBody(int ID, string Name, string Content)
    {
        charBuffer = Content;

        switch (statesForBlocks)
        {
            case BlockState.type:
            {
                Debug.Log($" charbuffer = {charBuffer}");

                string[] typeChunks = charBuffer.Split("!?");

                float health = 0;
                float speed = 0;
                float damage = 0;


                Debug.Log($" typechunks = {typeChunks.Length}");

                Regex typePattern = new Regex("(health|speed|damage)=>(\\d+)");

                for (int i = 0; i < typeChunks.Length - 1; i++)
                {
                    Debug.Log($" typechunks = {typeChunks[i]}");

                    Match typeBlocks = typePattern.Match(typeChunks[i]);

                    if (typeBlocks.Success)
                    {
                        Debug.Log($"Header block = {typeBlocks.Groups[1].Value}");
                        if (typeBlocks.Groups[1].Value == "health")
                        {
                            health = float.Parse(typeBlocks.Groups[2].Value);

                        }
                        else if (typeBlocks.Groups[1].Value == "speed")
                        {
                            speed = float.Parse(typeBlocks.Groups[2].Value);
                        }
                        else if (typeBlocks.Groups[1].Value == "damage")
                        {
                            damage = float.Parse(typeBlocks.Groups[2].Value);
                        }

                    }
                }
                Debug.Log("Dupe checker");

                Enemies enemy = new Enemies()
                {
                    EnemyName = Name,
                    //ID = ID,
                    Health = health,
                    Speed = speed,
                    Damage = damage,

                };
                try
                {
                    _enemyDictionary.Add(ID, enemy);
                }
                catch (ArgumentException)
                {
                    _enemyDictionary[ID] = enemy;
                }

                        
                Debug.Log("Reached End");
                ClearBuffer();
                charIndex = 0;
                Enemy enemyType = ScriptableObject.CreateInstance<Enemy>();
                enemyType.ID = ID;
                enemyType.enemyName = _enemyDictionary[ID].EnemyName;
                enemyType.health = _enemyDictionary[ID].Health;
                enemyType.speed = _enemyDictionary[ID].Speed;
                enemyType.damage = _enemyDictionary[ID].Damage;
                enemyType.name = _enemyDictionary[ID].EnemyName;
                context.AddObjectToAsset($"enemyObject {enemyType.ID}", enemyType);
                TypesSO.Add(enemyType);
                break;
            }
            case BlockState.wave:
            {
                string[] WaveBlocks = Content.Split("!?");
                int savedIndex = 0;

                Waves temp;


                Regex Pattern = new Regex("([CT])(\\d)\\<?(\\d)?\\>?\\[?(\\d)?\\]?");

                Debug.Log(WaveBlocks.Length);

                
                Waves parseWaves = new Waves()
                {
                    ID = ID,
                    Name = Name,
                    _dataForWaves = new List<Waves.WaveData>()
                };
                
                
                try
                {
                    _wavesDictionary.Add(ID, parseWaves);
                }
                catch (ArgumentException)
                {
                    
                }
    

               
                for (int a = 0; a < WaveBlocks.Length - 1; a++)
                {
                    Waves.WaveData waveData;
                    Match waveGroups = Pattern.Match(WaveBlocks[a]);
                    if (waveGroups.Success)
                    {
                        if (float.TryParse(waveGroups.Groups[3].Value, out float spawnResult)) ;
                        if (int.TryParse(waveGroups.Groups[4].Value, out int PopDensityResult)) ;

                        Debug.Log($"{waveGroups.Success}");
                        if (waveGroups.Groups[1].Value == "C")
                        {
                            waveData = new Waves.WaveData()
                            {
                                IsCluster = true,
                                ID = int.Parse(waveGroups.Groups[2].Value),
                                SpawnTime = spawnResult,
                                PopulationDensity = PopDensityResult,

                            };
                            parseWaves._dataForWaves.Add(waveData);
                        }
                        else if (waveGroups.Groups[1].Value == "T")
                        {
                            waveData = new Waves.WaveData()
                            {
                                IsCluster = false,
                                ID = int.Parse(waveGroups.Groups[2].Value),
                                SpawnTime = spawnResult,
                                PopulationDensity = PopDensityResult,

                            };
                            parseWaves._dataForWaves.Add(waveData);
                            
                        }
                        
                    }

                   
                    try
                    {
                        _wavesDictionary.Add(ID, parseWaves);
                    }
                    catch (ArgumentException)
                    {
                        Debug.Log(parseWaves._dataForWaves.Count);
                        foreach (Waves.WaveData data in parseWaves._dataForWaves.ToList())
                        {
                            _wavesDictionary[ID]._dataForWaves.Add(data);
                        }
                    }
                    
                    Wave WaveType = ScriptableObject.CreateInstance<Wave>();
                    WaveType.ID = ID;
                    WaveType.Name = _wavesDictionary[ID].Name;
                    WaveType.name = _wavesDictionary[ID].Name;
                    foreach (Waves.WaveData waveDataVar in _wavesDictionary[ID]._dataForWaves)
                    { 
                        WaveType.AddData(waveDataVar);
                    }

                    WavesSO.Add(WaveType);
                    context.AddObjectToAsset($"waveObject {WaveType.ID}", WaveType);
                }

                break;
            }
            case BlockState.cluster:
            {
                int type = 0;
                int AmountToSpawn = 0;
                Regex Pattern = new Regex("(\\d+):(\\d+)");

                Match clusterGroups = Pattern.Match(charBuffer);
                if (clusterGroups.Success)
                {
                    type = int.Parse(clusterGroups.Groups[1].Value);
                    AmountToSpawn = int.Parse(clusterGroups.Groups[2].Value);
                }
                
                int savedIndex = -0;

                if (ClustersList.Count != 0)
                {
                    for (int index = -0; index < ClustersList.Count; index++)
                    {
                        Debug.Log("Dupe checker");
                        if (ClustersList[index].ID == ID)
                        {
                            Clusters clustersList = ClustersList[index];
                            clustersList.ClusterName = Name;
                            clustersList.ID = ID;
                            clustersList.Type = type;
                            clustersList.AmountToSpawn = AmountToSpawn;
                            ClustersList[index] = clustersList;
                            savedIndex = index;
                            break;

                        }
                        Clusters cluster = new Clusters()
                        {
                            ClusterName = Name,
                            ID = ID,
                            AmountToSpawn = AmountToSpawn,
                            Type = type
             

                        };
                        ClustersList.Add(cluster);
                        savedIndex = ClustersList.LastIndexOf(cluster);
                    }
                }
                else if (ClustersList.Count == 0)
                {
                    Clusters cluster = new Clusters()
                    {
                        ClusterName = Name,
                        ID = ID,
                        AmountToSpawn = AmountToSpawn,
                        Type = type
                    };
                    ClustersList.Add(cluster);
                    savedIndex = ClustersList.LastIndexOf(cluster);
                }
                Cluster clusterType = ScriptableObject.CreateInstance<Cluster>();
                clusterType.ID = ClustersList[savedIndex].ID;
                clusterType.clusterName = ClustersList[savedIndex].ClusterName;
                clusterType.AddData(ClustersList[savedIndex].Type, ClustersList[savedIndex].AmountToSpawn);
                clusterType.name = ClustersList[savedIndex].ClusterName;
                ClusterSO.Add(clusterType);
                context.AddObjectToAsset($"clusterObject {clusterType.ID}", clusterType);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        scriptableHolder.AddTypes(TypesSO);
        scriptableHolder.AddCluster(ClusterSO);
        scriptableHolder.AddWaves(WavesSO);
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


