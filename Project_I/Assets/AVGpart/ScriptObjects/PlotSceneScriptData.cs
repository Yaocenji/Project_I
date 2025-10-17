using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [System.Serializable]
    public class SinglePlotScriptData
    {
        public string plotSceneName;
        public string plotSceneFilePath;
    }
    
    [CreateAssetMenu(fileName = "PlotScene_ACTs", menuName = "GameData_ScriptableObjects/PlotScene_ACTs")]
    public class PlotSceneScriptData : ScriptableObject
    {
        public List<SinglePlotScriptData> scripts;
    }
}