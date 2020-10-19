using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GoofMap {

    public class UIManager : MonoBehaviour {

        public Loader Loader;
        public Dropdown LevelSelectDropdown;

        public void OnEnable()
        {
            Loader.OnLoad += () =>
            {
                LevelSelectDropdown.ClearOptions();
                LevelSelectDropdown.AddOptions(Loader.LevelRoots.Keys.ToList());
            };
        }

        public void ChangeLevel(int i)
        {
            string levelName = LevelSelectDropdown.options[i].text;
            Loader.SelectLevel(levelName);
        }

    }
}
