using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 网络客户端输入.
/// </summary>
public class LesSyncInputCell
{
    public int FrameIdx = 0;                        // 帧Index.
    public bool JoyStickInMove = false;             // 摇杆状态.
    public Vector2 JoyStickValue = Vector2.zero;    // 摇杆值.
    public int SkillIndex = -1;                     // 技能输入.
}

public class LesSyncInputCellMap : Dictionary<int, LesSyncInputCell> { }


/// <summary>
/// 网络帧输入缓存.
/// 1. 本地客户端截取玩家的操作输入,并且放到输入队列.
/// 2. 本地定时往服务器发送输入操作队列.
/// 3. 服务器会定时将本地玩家的操作转发下来，保存到队列.
/// 4. 客户端定时执行&模拟服务器数据.
/// </summary>
public class LesSyncInputCaches
{
    //protected LesSyncInputCellMap mFrameInputCachesMap = new LesSyncInputCellMap(); // 本地玩家操作记录.
    protected Dictionary<int, LesSyncInputCell> mFrameInputCachesMap = new Dictionary<int, LesSyncInputCell>();
    protected Dictionary<int, NotifyPlayersFrameInputCmd> mDispatchedFromServer = new Dictionary<int, NotifyPlayersFrameInputCmd>();
    protected Queue<NotifyPlayersFrameInputCmd> mServerCmdQueues = new Queue<NotifyPlayersFrameInputCmd>();
    protected int mLastServerFrame = -1;

    protected int mClientFrameIdx = -1;

    protected const float TimeGapSync = 1 / 12f;
    protected float mTimeSinceLastSync = 0;
    protected float mTimeSinceLastJoyStickMove = float.MinValue;

    protected float mTimeScale = 1f;

    public LesSyncInputCaches()
    {
    }

    /// <summary>
    /// 获取一帧数据，如果有，返回，如果没有，则新建一个并返回.
    /// </summary>
    public LesSyncInputCell GetCurrentFrameDataByIdx()
    {
        LesSyncInputCell frameData = null;
        if (!mFrameInputCachesMap.ContainsKey(mClientFrameIdx))
        {
            frameData = new LesSyncInputCell();
            mFrameInputCachesMap[mClientFrameIdx] = frameData;
        }
        else
            frameData = mFrameInputCachesMap[mClientFrameIdx];
        return frameData;
    }

    /// <summary>
    /// 获取一帧数据，如果有，返回，如果没有，则新建一个并返回.
    /// </summary>
    public LesSyncInputCell GetFrameDataByIdx(int frameIdx)
    {
        LesSyncInputCell frameData = null;
        if (!mFrameInputCachesMap.ContainsKey(frameIdx))
        {
            frameData = new LesSyncInputCell();
            mFrameInputCachesMap[frameIdx] = frameData;
        }
        else
            frameData = mFrameInputCachesMap[frameIdx];
        return frameData;
    }

    public void Reset()
    {
        mFrameInputCachesMap.Clear();
        mTimeSinceLastSync = 0;
        mLastServerFrame = -1;
        mTimeScale = 1f;
    }

    public void DoFixedUpdate(float timeDiff)
    {
        mTimeSinceLastSync += timeDiff;
        //if (mTimeSinceLastSync >= TimeGapSync)
        {
            mTimeSinceLastSync = 0;

            // 判断是否有按键或者摇杆操作如果都没有的话 这里可以不用发送数据给服务器
            foreach (var input in mFrameInputCachesMap)
            {
                //var curInput = GetFrameDataByIdx(Time.frameCount);
                if (input.Value.SkillIndex != -1 || input.Value.JoyStickInMove)
                {
                    var info = new LesFrameInputData(input.Value.JoyStickInMove ? input.Value.JoyStickValue : Vector2.zero
                                                    , TimeGapSync
                                                    , input.Value.SkillIndex);
                    LesGameCore.Instance.Notify(new RequestPlayerFrameInputCmd(info));
                }
            }
            mFrameInputCachesMap.Clear();
        }
    }


    public void DrawGUIStuff()
    {
        GUI.Label(new Rect(20, 50, 500, 100), "ClientFrameIdx :" + mClientFrameIdx.ToString());
        GUI.Label(new Rect(20, 150, 500, 100), "LastServerFrame :" + mLastServerFrame.ToString());
        GUI.Label(new Rect(20, 250, 500, 100), "CurrentMoveDelta :" + mCurrentMoveDelta.ToString());
    }

    public void validateTimeScale()
    {

    }


    protected Vector2 mCurrentMoveDelta = Vector2.zero;

    /// <summary>
    /// Update & dispatch all the events.
    /// </summary>
    public void DoUpdate(float timeDiff)
    {
        if (LesGameCore.Instance.CurrentGameState != GameState.State_Gaming)
            return;

        //while (mClientFrameIdx < mLastServerFrame)
        if (mServerCmdQueues.Count > 0)
        {
            NotifyPlayersFrameInputCmd cmd = mServerCmdQueues.Dequeue();
            //if (mDispatchedFromServer.ContainsKey(mClientFrameIdx))
            if (cmd != null)
            {
                foreach (var cmdData in cmd.ctrls)
                {
                    LesUnit unit = LesBattleField.Instance.GetUnit(cmdData.key.ToString());
                    if (!unit || unit.IsDead || unit.MarkAsVanished || !unit.gameObject.activeInHierarchy)
                        continue; // 角色特殊情况下pass掉.

                    foreach (var inputCmd in cmdData.frames)
                    {
                        mCurrentMoveDelta = new Vector2(inputCmd.x, inputCmd.y);
                        unit.ControlUnitMoveByLockStep(new Vector2(inputCmd.x, inputCmd.y), timeDiff);
                        if (inputCmd.skill == 0)
                        {
                            unit.DoNormalAttack();
                        }
                        else if (inputCmd.skill == 1
                                || inputCmd.skill == 2
                                || inputCmd.skill == 3
                                || inputCmd.skill == 4)
                        {
                            unit.DoSkill(inputCmd.skill);
                        }
                    }
                }
            }
        }
        mClientFrameIdx++;
    }


    public void AddFrameDataFromServer(NotifyPlayersFrameInputCmd inputCmd)
    {
        if (!mDispatchedFromServer.ContainsKey(inputCmd.frame))
        {
            mDispatchedFromServer[inputCmd.frame] = inputCmd;
            mLastServerFrame = Mathf.Max(mLastServerFrame, inputCmd.frame);
        }
        mServerCmdQueues.Enqueue(inputCmd);
    }


    public void OnLevelStart()
    {
        mClientFrameIdx = 0;
        mDispatchedFromServer.Clear();
        mServerCmdQueues.Clear();
    }

    public Dictionary<int, LesSyncInputCell> FrameInputCachesMap { get { return mFrameInputCachesMap; } }
    public Dictionary<int, NotifyPlayersFrameInputCmd> DispatchedFromServer { get { return mDispatchedFromServer; } }
    public int ClientFrameIdx { get { return mClientFrameIdx; } set { mClientFrameIdx = value; } }
}

