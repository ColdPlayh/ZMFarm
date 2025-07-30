using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPC/NPC Schedule", order = 0)]
public class ScheduleDataList_SO : ScriptableObject
{
        public List<ScheduleDetails> scheduleList;
}
