using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuSettings : MonoBehaviour
    {
        Button_Base[] btnE = new Button_Base[5];

        [SerializeField] SceneMenuAnchor[] _sceneAnchor;

        [Header("アタッチ不要")]
        [SerializeField] Transform[] _actionParentButton;

        //LiveScene用
        Material _matMirrore;
        //VIEWER用
        BackGroundController _backGroundCon;

        void Start()
        {
            // Title分を除外で-1
            var current = (int)SceneChangeService.GetSceneType - 1;

            //シーン応じて有効化を切り替える
            for (int i = 0; i < _sceneAnchor.Length; i++)
            {
                if (i == current)
                {
                    if (!_sceneAnchor[i].gameObject.activeSelf) _sceneAnchor[i].gameObject.SetActive(true);
                }
                else if (_sceneAnchor[i].gameObject.activeSelf)
                {
                    _sceneAnchor[i].gameObject.SetActive(false);
                }
            }

            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                _actionParentButton = new Transform[5];
                _actionParentButton[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                _actionParentButton[1] = GameObject.FindGameObjectWithTag("LaserGun").transform;
                _actionParentButton[2] = GameObject.FindGameObjectWithTag("FloorMirror").transform;
                _actionParentButton[3] = GameObject.FindGameObjectWithTag("SonicBoom").transform;
                _actionParentButton[4] = GameObject.FindGameObjectWithTag("ManualUI").transform;

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
                _actionParentButton[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
                _actionParentButton[3].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
                _actionParentButton[4].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);

                _matMirrore = _actionParentButton[2].GetComponent<MeshRenderer>().material;
                _matMirrore.SetFloat("_Smoothness", FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                _actionParentButton = new Transform[3];
                _actionParentButton[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                _actionParentButton[1] = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
                _actionParentButton[2] = GameObject.FindGameObjectWithTag("WaterAnchor").transform;

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
                _actionParentButton[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
                _actionParentButton[2].transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                _actionParentButton = new Transform[1];
                _actionParentButton[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
                _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                //_actionParentButton = new Transform[2];?
            }
            else if (SceneChangeService.GetSceneType == SceneType.FANTASY_VILLAGE)
            {
                //未実装
            }

            //シーン別専用ボタンの割り当て
            for (int i = 0; i < _actionParentButton.Length; i++)
            {
                btnE[i] = _actionParentButton[current].transform.GetChild(i).GetComponent<Button_Base>();
            }
        }

        void OnEnable()
        {
            //各種有効化状態にボタンを合わせる
            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                btnE[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
                btnE[1].isEnable = _actionParentButton[1].gameObject.activeSelf;
                btnE[3].isEnable = _actionParentButton[3].gameObject.activeSelf;
                btnE[4].isEnable = _actionParentButton[4].gameObject.activeSelf;

                btnE[2].isEnable = (_matMirrore.GetFloat("_Smoothness") == 1.0f ? true : false);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                btnE[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
                btnE[1].isEnable = _actionParentButton[1].gameObject.activeSelf;
                btnE[2].isEnable = _actionParentButton[2].transform.GetChild(0).gameObject.activeSelf;
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                btnE[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                btnE[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                btnE[0].isEnable = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
                btnE[1].isEnable = FileReadAndWriteUtility.UserProfile.StepSE;
            }
            else if (SceneChangeService.GetSceneType == SceneType.FANTASY_VILLAGE)
            {
                //未実装
            }
        }
    }
}
