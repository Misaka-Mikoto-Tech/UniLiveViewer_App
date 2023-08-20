using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{ 
    /// <summary>
    /// やっぱ消すか悩み中
    /// </summary>
    public class StageSettingService
    {
        public static UserProfile UserProfile { get; private set; }

        StageSettingService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            UserProfile = FileReadAndWriteUtility.ReadJson();
        }
    }
}
