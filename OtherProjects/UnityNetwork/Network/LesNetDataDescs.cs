using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum VariableType
{
    Numeric,
    String,
    Double,
    Bool,
}

[ProtoBuf.ProtoContract]
public class Variable : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public VariableType type;
    [ProtoBuf.ProtoMember(2)]
    public Int64 ivalue;
    [ProtoBuf.ProtoMember(3)]
    public double fvalue;
    [ProtoBuf.ProtoMember(4)]
    public string svalue;
    [ProtoBuf.ProtoMember(5)]
    public bool bvalue;

    public Variable(long value)
    {
        type = VariableType.Numeric;
        ivalue = value;
    }

    public Variable(double value)
    {
        type = VariableType.Double;
        fvalue = value;
    }

    public Variable(string value)
    {
        type = VariableType.String;
        svalue = value;
    }

    public Variable(bool value)
    {
        type = VariableType.Bool;
        bvalue = value;
    }
}

// 目前用来测试用的结构
[ProtoBuf.ProtoContract]
public class BevNodeDesc : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public Int32 node_type;
    [ProtoBuf.ProtoMember(2)]
    public string node_name;
    [ProtoBuf.ProtoMember(3)]
    public BevNodeDesc pre_cond;
    [ProtoBuf.ProtoMember(4)]
    public BevNodeDesc suf_cond;
    [ProtoBuf.ProtoMember(5)]
    public Dictionary<string, Variable> args = new Dictionary<string, Variable>();
    [ProtoBuf.ProtoMember(6)]
    public List<BevNodeDesc> childs = new List<BevNodeDesc>();

    public void AddParamPair(string key, Object value)
    {
        if (value.GetType().IsEnum)
            AddParamPair(key, (Int32)value);
        else
            AddParamPair(key, value.ToString());
    }

    public void AddParamPair(string key, long value)
    {
        args.Add(key, new Variable(value));
    }
    public void AddParamPair(string key, string value)
    {
        args.Add(key, new Variable(value));
    }
    public void AddParamPair(string key, double value)
    {
        args.Add(key, new Variable(value));
    }

    public void AddParamPair(string key, bool value)
    {
        args.Add(key, new Variable(value));
    }

    public byte[] Build()
    {
        var cmd_stream = new System.IO.MemoryStream();
        ProtoBuf.Serializer.Serialize(cmd_stream, this);
        return cmd_stream.ToArray();
    }
}

//====================================================================

public enum LesBlockType
{
    BNone = 0,   // 无阻挡
    BStatic = 1, // 静态阻挡
}

[ProtoBuf.ProtoContract]
public class MapBlockData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public byte[] blocks;
    [ProtoBuf.ProtoMember(2)]
    public Int32 map_width;
    [ProtoBuf.ProtoMember(3)]
    public Int32 map_height;
    [ProtoBuf.ProtoMember(4)]
    public Int32 off_x;
    [ProtoBuf.ProtoMember(5)]
    public Int32 off_y;

    public MapBlockData()
    {
    }

    public MapBlockData(Int32 _width, Int32 _height, Int32 _off_x, Int32 _off_y, byte[] _blocks)
    {
        blocks = _blocks;
        map_width = _width;
        map_height = _height;
        off_x = _off_x;
        off_y = _off_y;
    }

    public byte[] Build()
    {
        var cmd_stream = new System.IO.MemoryStream();
        ProtoBuf.Serializer.Serialize(cmd_stream, this);
        return cmd_stream.ToArray();
    }
}

[ProtoBuf.ProtoContract]
public class BattleLevelCampTeamData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public string Name;
    [ProtoBuf.ProtoMember(2)]
    public Int32 InstId;
    [ProtoBuf.ProtoMember(3)]
    public SpawnStrategy EnemiesSpawnStrategy;
    [ProtoBuf.ProtoMember(4)]
    public float SpawnTime;
    [ProtoBuf.ProtoMember(5)]
    public List<UnitBornPointData> UnitBornPoints;
    [ProtoBuf.ProtoMember(6)]
    public BattleLevelObstaclesOperation InObstacles;
    [ProtoBuf.ProtoMember(7)]
    public BattleLevelObstaclesOperation OutObstacles;
    [ProtoBuf.ProtoMember(8)]
    public Position Pos;
    [ProtoBuf.ProtoMember(9)]
    public float Dir;
    [ProtoBuf.ProtoMember(10)]
    public Position CubeSize;
}

[ProtoBuf.ProtoContract]
public class StrongholdSignData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public string Prefab;
    [ProtoBuf.ProtoMember(2)]
    public Position Pos;
    [ProtoBuf.ProtoMember(3)]
    public Position Rot;
    [ProtoBuf.ProtoMember(4)]
    public Position Scale;
    [ProtoBuf.ProtoMember(5)]
    public Int32 InstId;
    [ProtoBuf.ProtoMember(6)]
    public bool CanOccupied;        // 所属父级据点是否可占领
    [ProtoBuf.ProtoMember(7)]
    public LesStrongholdType Type;  // 所属父级据点类型
}

[ProtoBuf.ProtoContract]
public class BattleLevelTrigger : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public TriggerShape TriggerShapeType;
    [ProtoBuf.ProtoMember(2)]
    public Position CubeSize;
    [ProtoBuf.ProtoMember(3)]
    public float Radius;
    [ProtoBuf.ProtoMember(4)]
    public Position Pos;
    [ProtoBuf.ProtoMember(5)]
    public float Dir;
    [ProtoBuf.ProtoMember(6)]
    public Position FixPos;
}

[ProtoBuf.ProtoContract]
public class BattleLevelObstaclesData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public Int32 InstId;
    [ProtoBuf.ProtoMember(2)]
    public Int32 ObstaclesLenght;
    [ProtoBuf.ProtoMember(3)]
    public bool Enabled;
    [ProtoBuf.ProtoMember(4)]
    public Position Pos;
    [ProtoBuf.ProtoMember(5)]
    public float Dir;
}

[ProtoBuf.ProtoContract]
public class BattleLevelObstaclesOperation : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public List<BattleLevelObstaclesData> list;
    [ProtoBuf.ProtoMember(2)]
    public List<bool> operation;
}

// 据点基类
[ProtoBuf.ProtoContract]
public class BattleLevelStrongholdBase : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public string Name;
    [ProtoBuf.ProtoMember(2)]
    public float OccupationTime;
    [ProtoBuf.ProtoMember(3)]
    public int CanOccupationUnitType;
    [ProtoBuf.ProtoMember(4)]
    public LesCamp Camp;
    [ProtoBuf.ProtoMember(5)]
    public LesStrongholdType Type;
    [ProtoBuf.ProtoMember(6)]
    public UnitBornPointData Avatar;
    [ProtoBuf.ProtoMember(7)]
    public Position Pos;
    [ProtoBuf.ProtoMember(8)]
    public float Dir;
    [ProtoBuf.ProtoMember(9)]
    public bool CanOccupied;
    [ProtoBuf.ProtoMember(10)]
    public Position ColliderPos;
    [ProtoBuf.ProtoMember(11)]
    public float ColliderRadius;
    [ProtoBuf.ProtoMember(12)]
    public Int32 InstId;
    [ProtoBuf.ProtoMember(13)]
    public StrongholdSignData RedSign;
    [ProtoBuf.ProtoMember(14)]
    public StrongholdSignData BlueSign;
    [ProtoBuf.ProtoMember(15)]
    public StrongholdSignData NoneSign;
    [ProtoBuf.ProtoMember(16)]
    public BattleLevelTrigger TriggerInfo;
}

// 复活据点
[ProtoBuf.ProtoContract]
public class BattleLevelRebornStronghold : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public RebornCylinderData RedRebornPoint;
    [ProtoBuf.ProtoMember(2)]
    public RebornCylinderData BlueRebornPoint;
    [ProtoBuf.ProtoMember(3)]
    public RebornCylinderData NoneRebornPoint;
}

// 资源据点
[ProtoBuf.ProtoContract]
public class BattleLevelResourceStronghold : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public float MaxResources;
    [ProtoBuf.ProtoMember(2)]
    public float ObtainResourceSpeed;
}

// 兵营据点
[ProtoBuf.ProtoContract]
public class BattleLevelBarracksStronghold : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public BarrackWakeupEnum DispatchtroopsCondition;
    [ProtoBuf.ProtoMember(2)]
    public float StartDispatchtroopsTime;
    [ProtoBuf.ProtoMember(3)]
    public SpawnStrategy TeamSpawnStrategy;
    [ProtoBuf.ProtoMember(4)]
    public float FixedTime;
    [ProtoBuf.ProtoMember(5)]
    public bool IsLoop;
    [ProtoBuf.ProtoMember(6)]
    public bool DisplayTeamCount;
    [ProtoBuf.ProtoMember(7)]
    public EndSpawnCondition EndCondition;
    [ProtoBuf.ProtoMember(8)]
    public int EndCount;
    [ProtoBuf.ProtoMember(9)]
    public List<BattleLevelCampTeamData> TeamList;
    [ProtoBuf.ProtoMember(10)]
    public Int32 EnemyInstId;
    [ProtoBuf.ProtoMember(11)]
    public Int32 TeamInstId;
    [ProtoBuf.ProtoMember(12)]
    public Int32 TrapInstId;
}

[ProtoBuf.ProtoContract]
public class BattleLevelTrapData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public Int32 TrapTriggerId;
    [ProtoBuf.ProtoMember(2)]
    public LesCamp Camp;
    [ProtoBuf.ProtoMember(3)]
    public bool IsLoop;
    [ProtoBuf.ProtoMember(4)]
    public float LoopIntervalTime;
    [ProtoBuf.ProtoMember(5)]
    public Position Pos;
    [ProtoBuf.ProtoMember(6)]
    public float Dir;
    [ProtoBuf.ProtoMember(7)]
    public bool RandomPosSpawn;
    [ProtoBuf.ProtoMember(8)]
    public Position SpawnPoint;
    [ProtoBuf.ProtoMember(9)]
    public float SpawnRadius;
    [ProtoBuf.ProtoMember(10)]
    public float InTime;
    [ProtoBuf.ProtoMember(11)]
    public float OutTime;
    [ProtoBuf.ProtoMember(12)]
    public ActionTriggerEnum StartTrigger;
    [ProtoBuf.ProtoMember(13)]
    public ActionTriggerEnum ExitTrigger;
    [ProtoBuf.ProtoMember(14)]
    public bool WaitLastCompleted;
    [ProtoBuf.ProtoMember(15)]
    public bool JustOnce;
    [ProtoBuf.ProtoMember(16)]
    public Position ColliderLength;
    [ProtoBuf.ProtoMember(17)]
    public Int32 TrapInstId;
    [ProtoBuf.ProtoMember(18)]
    public bool TimedCtrl;
    [ProtoBuf.ProtoMember(19)]
    public bool EnableOnStart;
    [ProtoBuf.ProtoMember(20)]
    public float EnterTime;
    [ProtoBuf.ProtoMember(21)]
    public float ExitTime;
    [ProtoBuf.ProtoMember(22)]
    public bool OnceCollision;
    [ProtoBuf.ProtoMember(23)]
    public float Radius;
}

// 触发器数据
[ProtoBuf.ProtoContract]
public class BattleLevelTriggerData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public UInt32 TriggerKey;
    [ProtoBuf.ProtoMember(2)]
    public Int32 TriggerId;
    [ProtoBuf.ProtoMember(3)]
    public Position Pos;
    [ProtoBuf.ProtoMember(4)]
    public float Dir;
    [ProtoBuf.ProtoMember(5)]
    public LesCamp Camp;
    [ProtoBuf.ProtoMember(6)]
    public Int32 Hp;
    [ProtoBuf.ProtoMember(7)]
    public Int32 MaxHp;
}

// 机关组
[ProtoBuf.ProtoContract]
public class BattleLevelTrapGroupStronghold : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public BarrackWakeupEnum DispatchtroopsCondition;
    [ProtoBuf.ProtoMember(2)]
    public Int32 EnemyInstId;
    [ProtoBuf.ProtoMember(3)]
    public Int32 TeamInstId;
    [ProtoBuf.ProtoMember(4)]
    public float StartDispatchtroopsTime;
    [ProtoBuf.ProtoMember(5)]
    public SpawnStrategy ActivatorSpawnStrategy;
    [ProtoBuf.ProtoMember(6)]
    public float FixedTime;
    [ProtoBuf.ProtoMember(7)]
    public List<BattleLevelTrapData> TrapList;
    [ProtoBuf.ProtoMember(8)]
    public bool IsLoop;
    [ProtoBuf.ProtoMember(9)]
    public Int32 TrapInstId;
    [ProtoBuf.ProtoMember(10)]
    public bool IsFollow;
}

[ProtoBuf.ProtoContract]
public class BattleLevelStronghold : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public BattleLevelStrongholdBase Base;
    [ProtoBuf.ProtoMember(2)]
    public BattleLevelRebornStronghold Reborns;
    [ProtoBuf.ProtoMember(3)]
    public BattleLevelResourceStronghold Resources;
    [ProtoBuf.ProtoMember(4)]
    public BattleLevelBarracksStronghold Barracks;
    [ProtoBuf.ProtoMember(5)]
    public BattleLevelTrapGroupStronghold TrapGroup;
}

[ProtoBuf.ProtoContract]
public class VictoryConditionData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public BattleFinshConditions FinshConditions;
    [ProtoBuf.ProtoMember(2)]
    public string LabelName;
    [ProtoBuf.ProtoMember(3)]
    public int UnitID = 0;
    [ProtoBuf.ProtoMember(4)]
    public float Count = 1;
    [ProtoBuf.ProtoMember(5)]
    public LesCamp Camp;
    [ProtoBuf.ProtoMember(6)]
    public Int32 TeamInstId;
    [ProtoBuf.ProtoMember(7)]
    public Position Pos;
    [ProtoBuf.ProtoMember(8)]
    public float Radius = 1;
    [ProtoBuf.ProtoMember(9)]
    public Int32 UnitInstId;
    [ProtoBuf.ProtoMember(10)]
    public CompareSymbolVictioryCondition CompareSymbol;
    [ProtoBuf.ProtoMember(11)]
    public Int32 BuildingInstId;
    [ProtoBuf.ProtoMember(12)]
    public float ResourceCount;
    [ProtoBuf.ProtoMember(13)]
    public LesCamp Camp2;
}

[ProtoBuf.ProtoContract]
public class VictoryConditionGroupData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public List<VictoryConditionData> Conds;
}

[ProtoBuf.ProtoContract]
public class RebornCylinderData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public Position Pos;
    [ProtoBuf.ProtoMember(2)]
    public float Dir;
    [ProtoBuf.ProtoMember(3)]
    public float Radius;
}

[ProtoBuf.ProtoContract]
public class UnitBornPointData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public Int32 InstId;
    [ProtoBuf.ProtoMember(2)]
    public LesBornType BornType;
    [ProtoBuf.ProtoMember(3)]
    public LesCamp Camp;
    [ProtoBuf.ProtoMember(4)]
    public int UnitID;
    [ProtoBuf.ProtoMember(5)]
    public int AiID;
    [ProtoBuf.ProtoMember(6)]
    public int Level;
    [ProtoBuf.ProtoMember(7)]
    public bool InHide;
    [ProtoBuf.ProtoMember(8)]
    public bool IsLoop;
    [ProtoBuf.ProtoMember(9)]
    public List<Position> TargetPotorlPoints;
    [ProtoBuf.ProtoMember(10)]
    public Position DefensePoint;
    [ProtoBuf.ProtoMember(11)]
    public Position Pos;
    [ProtoBuf.ProtoMember(12)]
    public float Dir;
    [ProtoBuf.ProtoMember(13)]
    public bool Mirror;
}

[ProtoBuf.ProtoContract]
public class SpawnPointRootData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public List<RebornCylinderData> RedRebornCylinders;
    [ProtoBuf.ProtoMember(2)]
    public List<RebornCylinderData> BlueRebornCylinders;
    [ProtoBuf.ProtoMember(3)]
    public List<RebornCylinderData> NoneRebornCylinders;

    [ProtoBuf.ProtoMember(4)]
    public List<UnitBornPointData> RedBornPoints;
    [ProtoBuf.ProtoMember(5)]
    public List<UnitBornPointData> BlueBornPoints;
    [ProtoBuf.ProtoMember(6)]
    public List<UnitBornPointData> NoneBornPoints;
}

[ProtoBuf.ProtoContract]
public class BattleLevelSpawnObstructs : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public bool UseSpawnObstruct;
    [ProtoBuf.ProtoMember(2)]
    public string SpawnObstructprfab;
    [ProtoBuf.ProtoMember(3)]
    public List<Position> SpawnObstructsPos;
    [ProtoBuf.ProtoMember(4)]
    public List<Position> SpawnObstructsRot;
}

[ProtoBuf.ProtoContract]
public class BattleLevelRootData : ICommand
{
    [ProtoBuf.ProtoMember(1)]
    public bool IsCountdown = false;
    [ProtoBuf.ProtoMember(2)]
    public float CountdownSecond = 0;
    [ProtoBuf.ProtoMember(3)]
    public float RebornTime = 3f;
    [ProtoBuf.ProtoMember(4)]
    public float RebornPenalize = 0f;
    [ProtoBuf.ProtoMember(5)]
    public bool CanReborn = false;   
    [ProtoBuf.ProtoMember(6)]
    public List<VictoryConditionGroupData> WinVictoryConditions;
    [ProtoBuf.ProtoMember(7)]
    public List<VictoryConditionGroupData> FailureVictoryConditions;
    [ProtoBuf.ProtoMember(8)]
    public SpawnPointRootData SpawnPointRoot;
    [ProtoBuf.ProtoMember(9)]
    public List<BattleLevelStronghold> Strongholds;
    [ProtoBuf.ProtoMember(10)]
    public BattleLevelSpawnObstructs SpawnObstructs;
    
    public byte[] Build()
    {
        var cmd_stream = new System.IO.MemoryStream();
        ProtoBuf.Serializer.Serialize(cmd_stream, this);
        return cmd_stream.ToArray();
    }
}

/////////////////////////////////////////////////////////////////////////////////////
[ProtoBuf.ProtoContract]
public class MessageData
{
    [ProtoBuf.ProtoMember(1)]
    public byte[] packet;
    [ProtoBuf.ProtoMember(2)]
    public Int32 offset;

    public MessageData(byte[] _packet, Int32 _offset)
    {
        packet = _packet;
        offset = _offset;
    }
}

[ProtoBuf.ProtoContract]
public class MessageList
{
    [ProtoBuf.ProtoMember(1)]
    public List<MessageData> list = new List<MessageData>(128);
    [ProtoBuf.ProtoMember(2)]
    public Int32 sequence;

    public void Add(MessageData data)
    {
        list.Add(data);
    }

    public int Count() 
    {
        return list.Count;
    }

    public void Clear()
    {
        list.Clear();
    }

    public byte[] Build()
    {
        var cmd_stream = new System.IO.MemoryStream();
        ProtoBuf.Serializer.Serialize(cmd_stream, this);
        return cmd_stream.ToArray();
    }

    public void Save(string path)
    {
        System.IO.File.WriteAllBytes(path, this.Build());
    }
}