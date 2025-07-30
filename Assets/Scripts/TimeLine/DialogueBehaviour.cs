using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.Dialogue;
using UnityEngine;
using UnityEngine.Playables;
[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    private PlayableDirector director;
    public DialoguePiece dialoguePiece;
    
    //创建
    public override void OnPlayableCreate(Playable playable)
    {
        director = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if (Application.isPlaying)
        {
            if (dialoguePiece.hasToPause)
            {
                //暂停TimeLine 
                TimeLineManager.Instance.PauseTimelIne(director);
            }
            else
            {
                EventHandler.CallShowDialogueEvent(null);
            }
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);
        if (Application.isPlaying)
        {
            TimeLineManager.Instance.IsDone = dialoguePiece.isDone;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        EventHandler.CallShowDialogueEvent(null);
    }

    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        TimeLineManager.Instance.isPlaying = true;
    
    }

    public override void OnGraphStop(Playable playable)
    {
        AudioManager.Instance.musicPause = false;
        TimeLineManager.Instance.isPlayed = true;
        TimeLineManager.Instance.isPlaying = false;
        AudioManager.Instance.OnAfterSceneLoadedEvent();
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
      
    }
}
