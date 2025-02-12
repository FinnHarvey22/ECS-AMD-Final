using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Unity.Entities.UniversalDelegates;


[ScriptedImporter(1, "TomBen")]
public class TomBenScriptableImporter : ScriptedImporter
{

    private string _charBuffer;
    private int _charIndex;
    string _contentToParse;
    private ParserState _state;
    private BlockState _statesForBlocks;
    private bool _duplicateData;

    private List<ParsedBlock> _types;
    private List<Waves> _wavesList;
    private List<Clusters> _clustersList;

    private Dictionary<int, Enemies> _enemyDictionary = new Dictionary<int, Enemies>();
    private Dictionary<int, Waves> _wavesDictionary = new Dictionary<int, Waves>();
    private Dictionary<int, Clusters> _clustersDictionary = new Dictionary<int, Clusters>();


    private List<ScriptableObject> _typesSo;
    private List<ScriptableObject> _clusterSo;
    private List<ScriptableObject> _wavesSo;


    private Holder _scriptableHolder;


    private AssetImportContext _context;

    private int _bodyIndex;



    private struct ParsedBlock
    {
        public string Type;
        public string Name;
        public int ID;
        public string Content;

        public override string ToString() => $"ParsedBlock(Type={Type}, ID={ID},Name=={Name}, content={Content})";
    }

    private struct Enemies
    {
        public int ID;
        [CanBeNull] public string EnemyName;
        public float? Health;
        public float? Speed;
        public float? Damage;
    }

    [System.Serializable]
    public struct Waves
    {
        public int ID;
        public string Name;
        public List<WaveData> DataForWaves;
        
        [System.Serializable]
        public struct WaveData
        {
            public bool IsCluster;
            public int ID;
            public float SpawnTime;
            public int PopulationDensity;
        }
    }

    public struct Clusters
    {
        public int ID;
        public string ClusterName;
        public List<ClusterData> DataForClusters;

        [System.Serializable]
        public struct ClusterData
        {
            public int Type;
            public int AmountToSpawn;
        }

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
        _context = ctx;
        string fileText = File.ReadAllText(_context.assetPath);
        ChangeState(ParserState.Outside);
        
        _types = new List<ParsedBlock>();
        _typesSo = new List<ScriptableObject>();
        _clusterSo = new List<ScriptableObject>();
        _wavesSo = new List<ScriptableObject>();

        _clustersList = new List<Clusters>();
        _wavesList = new List<Waves>();

        _scriptableHolder = ScriptableObject.CreateInstance<Holder>();
        _context.AddObjectToAsset("Holder", _scriptableHolder);
        _context.SetMainObject(_scriptableHolder);
        ParseText(fileText);
    }


    private void ParseText(string fileContent)
    {
        _contentToParse = fileContent;
        while (!ReachedEnd())
        {
            switch (_state)
            {
                case ParserState.ParsingHeader:
                {
                    
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }

                    _charBuffer = _charBuffer[0..^4];

                    Regex headerPattern = new Regex("(type|wave|cluster)\\s*-\\s*(\\d+)\\s*(\\(([\\w\\s]+)\\))?\\s*");
                    Match headerBlocks = headerPattern.Match(_charBuffer);

                    if (headerBlocks.Success)
                    {
                        ClearBuffer();
                        while (!BufferHas("_Ben"))
                        {
                            NextChar();
                        }

                        _charBuffer = _charBuffer[0..^4];

                        ParsedBlock parsedHeaders = new ParsedBlock()
                        {
                            Type = headerBlocks.Groups[1].Value,
                            Name = headerBlocks.Groups[3].Value,
                            ID = int.Parse(headerBlocks.Groups[2].Value),
                            Content = _charBuffer,

                        };
                        _types.Add(parsedHeaders);
                        ClearBuffer();
                    }
                    break;
                }
                case ParserState.ParsingBlockBody:
                {
                    break;
                }
                case ParserState.Outside:
                {
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }

                    _charIndex = 0;
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
        _charIndex = 0;
        for (_bodyIndex = 0; _bodyIndex < _types.Count; _bodyIndex++)
        {
            if (_types[_bodyIndex].Type == "type") _statesForBlocks = BlockState.type;
            else if (_types[_bodyIndex].Type == "cluster") _statesForBlocks = BlockState.cluster;
            else if (_types[_bodyIndex].Type == "wave") _statesForBlocks = BlockState.wave;
            ParseBody(_types[_bodyIndex].ID, _types[_bodyIndex].Name, _types[_bodyIndex].Content);
        }

        DataInput();
    }

    private void ParseBody(int id, string blockName, string content)
    {
        _charBuffer = content;
        if (blockName == "")
        {
            blockName = "Unnamed";
        }

        switch (_statesForBlocks)
        {
            case BlockState.type:
            {

                string[] typeChunks = _charBuffer.Split("!?");

                float health = 0;
                float speed = 0;
                float damage = 0;

                Regex typePattern = new Regex("(health|speed|damage)=>(\\d+)");

                for (int i = 0; i < typeChunks.Length; i++)
                {

                    Match typeBlocks = typePattern.Match(typeChunks[i]);

                    if (typeBlocks.Success)
                    {
                        if (typeBlocks.Groups[1].Value == "health")
                        {
                            if (float.TryParse(typeBlocks.Groups[2].Value, out float healthOutput)) ;
                            health = healthOutput;
                        }
                        else if (typeBlocks.Groups[1].Value == "speed")
                        {
                            if (float.TryParse(typeBlocks.Groups[2].Value, out float speedOutput)) ;
                            speed = speedOutput;
                        }
                        else if (typeBlocks.Groups[1].Value == "damage")
                        {
                            if (float.TryParse(typeBlocks.Groups[2].Value, out float damageOutput)) ;

                            damage = damageOutput;
                        }
                    }
                }

                Enemies enemy = new Enemies()
                {
                    EnemyName = blockName is "Unnamed" ? null : blockName,
                    ID = id,
                    Health = health is 0 ? null : health,
                    Speed = speed is 0 ? null : speed,
                    Damage = damage is 0 ? null : damage,

                };
                try
                {
                    _enemyDictionary.Add(id, enemy);
                }
                catch (ArgumentException)
                {
                    enemy.EnemyName ??= _enemyDictionary[id].EnemyName;
                    enemy.Health ??= (float)_enemyDictionary[id].Health;
                    enemy.Speed ??= (float)_enemyDictionary[id].Speed;
                    enemy.Damage ??= (float)_enemyDictionary[id].Damage;

                    _enemyDictionary[id] = enemy;
                }

               
                ClearBuffer();
                _charIndex = 0;
                break;
            }
            case BlockState.wave:
            {
                string[] waveBlocks = content.Split("!?");

                Regex pattern = new Regex("([CT])(\\d+)\\<?(\\d+)?\\>?\\[?(\\d+)?\\]?");

                Waves parseWaves = new Waves()
                {
                    ID = id,
                    Name = blockName,
                    DataForWaves = new List<Waves.WaveData>()
                };
                for (int a = 0; a < waveBlocks.Length; a++)
                {
                    Match waveGroups = pattern.Match(waveBlocks[a]);
                    if (waveGroups.Success)
                    {
                        if (float.TryParse(waveGroups.Groups[3].Value, out float spawnResult)) ;
                        if (int.TryParse(waveGroups.Groups[4].Value, out int popDensityResult)) ;
                        Waves.WaveData waveData;
                        if (waveGroups.Groups[1].Value == "C")
                        {
                            waveData = new Waves.WaveData()
                            {
                                IsCluster = true,
                                ID = int.Parse(waveGroups.Groups[2].Value),
                                SpawnTime = spawnResult,
                                PopulationDensity = popDensityResult,
                            };
                            parseWaves.DataForWaves.Add(waveData);

                        }
                        else if (waveGroups.Groups[1].Value == "T")
                        {
                            waveData = new Waves.WaveData()
                            {
                                IsCluster = false,
                                ID = int.Parse(waveGroups.Groups[2].Value),
                                SpawnTime = spawnResult,
                                PopulationDensity = popDensityResult,
                            };
                            parseWaves.DataForWaves.Add(waveData);

                        }

                    }

                }

                try
                {
                    _wavesDictionary.Add(id, parseWaves);
                }
                catch (ArgumentException)
                {
                    foreach (Waves.WaveData data in parseWaves.DataForWaves.ToList())
                    {
                        _wavesDictionary[id].DataForWaves.Add(data);
                    }

                    if (blockName != "Unnamed")
                    {
                        Waves tempWaves = new Waves()
                        {
                            Name = blockName,
                            ID = id,
                            DataForWaves = _wavesDictionary[id].DataForWaves
                        };
                        _wavesDictionary[id] = tempWaves;
                    }
                }

                break;
            }
            case BlockState.cluster:
            {
                string[] clusterChunks = content.Split("!?");
                
                Regex pattern = new Regex("(\\d+):(\\d+)");

                Clusters cluster = new Clusters()
                {
                    ClusterName = blockName,
                    ID = id,
                    DataForClusters = new List<Clusters.ClusterData>()
                };
                for (int i = 0; i < clusterChunks.Length - 1; i++)
                {
                    Match clusterGroups = pattern.Match(clusterChunks[i]);
                    if (clusterGroups.Success)
                    {
                        Clusters.ClusterData clusterData = new Clusters.ClusterData()
                        {
                            Type = int.Parse(clusterGroups.Groups[1].Value),
                            AmountToSpawn = int.Parse(clusterGroups.Groups[2].Value),
                        };
                        cluster.DataForClusters.Add(clusterData);
                    }
                }

                try
                {
                    _clustersDictionary.Add(id, cluster);
                }
                catch
                {
                    foreach (Clusters.ClusterData data in cluster.DataForClusters.ToList())
                    {
                        _clustersDictionary[id].DataForClusters.Add(data);
                    }

                    if (blockName != "Unnamed")
                    {
                        Clusters clusterTemp = new Clusters()
                        {
                            ClusterName = blockName,
                            ID = id,
                            DataForClusters = _clustersDictionary[id].DataForClusters
                        };

                        _clustersDictionary[id] = clusterTemp;
                    }
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DataInput()
    {
        foreach (Enemies enemies in _enemyDictionary.Values)
        {
            Enemy enemyType = ScriptableObject.CreateInstance<Enemy>();
            enemyType.ID = enemies.ID;
            enemyType.enemyName = enemies.EnemyName;
            if (enemies.Health != null) enemyType.health = (float)enemies.Health;
            if (enemies.Speed != null) enemyType.speed = (float)enemies.Speed;
            if (enemies.Damage != null) enemyType.damage = (float)enemies.Damage;
            enemyType.name = enemies.EnemyName;
            _context.AddObjectToAsset($"enemyObject {enemyType.ID}", enemyType);
            _typesSo.Add(enemyType);
        }

        foreach (Waves waves in _wavesDictionary.Values)
        {
            Wave waveType = ScriptableObject.CreateInstance<Wave>();
            waveType.ID = waves.ID;
            waveType.Name = waves.Name;
            waveType.name = waves.Name;
            /*foreach (Waves.WaveData waveDataVar in _wavesDictionary[waves.ID].DataForWaves)
            {
                waveType.AddData(waveDataVar);
            }*/
            waveType.WaveDataArray = _wavesDictionary[waves.ID].DataForWaves.ToArray();
            _wavesSo.Add(waveType);
            _context.AddObjectToAsset($"waveObject {waveType.ID}", waveType);
        }

        foreach (Clusters clusters in _clustersDictionary.Values)
        {
            Cluster clusterType = ScriptableObject.CreateInstance<Cluster>();
            clusterType.ID = clusters.ID;
            clusterType.clusterName = clusters.ClusterName;
            /*foreach (Clusters.ClusterData clusterDataVar in _clustersDictionary[clusters.ID].DataForClusters)
            {
                clusterType.AddData(clusterDataVar,_clustersDictionary.Count);
            }*/

            clusterType.clusterDatas = _clustersDictionary[clusters.ID].DataForClusters.ToArray();

            clusterType.name = clusters.ClusterName;
            _clusterSo.Add(clusterType);
            _context.AddObjectToAsset($"clusterObject {clusterType.ID}", clusterType);
        }
        
        _scriptableHolder.AddTypes(_typesSo);
        _scriptableHolder.AddCluster(_clusterSo);
        _scriptableHolder.AddWaves(_wavesSo);

    }

    private void ClearBuffer() => _charBuffer = "";

    private bool ReachedEnd() => _charIndex >= _contentToParse.Length;

    // ReSharper disable once UnusedMethodReturnValue.Local
    private char NextChar()
    {
        _charBuffer += _contentToParse[_charIndex];
        return _contentToParse[_charIndex++];
    }

    private void ChangeState(ParserState parserState)
    {
        this._state = parserState;
        ClearBuffer();
    }

    private bool BufferHas(string token) => _charBuffer.EndsWith(token);

    // ReSharper disable once UnusedMember.Local
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