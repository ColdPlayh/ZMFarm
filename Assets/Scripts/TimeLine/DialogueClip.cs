﻿using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class DialogueClip : PlayableAsset,ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.None;
    public DialogueBehaviour template = new DialogueBehaviour();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph, template);
        return playable;
    }

   
}
