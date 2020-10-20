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
        public GameObject LoadingScreen;

        public void OnEnable()
        {
            LoadingScreen.SetActive(true);

            Loader.OnLoad += () =>
            {
                LevelSelectDropdown.ClearOptions();
                LevelSelectDropdown.AddOptions(Loader.LevelRoots.Keys.ToList());
                LoadingScreen.SetActive(false);
            };
        }

        public void ChangeLevel(int i)
        {
            string levelName = LevelSelectDropdown.options[i].text;
            Loader.SelectLevel(levelName);
        }

    }
}
