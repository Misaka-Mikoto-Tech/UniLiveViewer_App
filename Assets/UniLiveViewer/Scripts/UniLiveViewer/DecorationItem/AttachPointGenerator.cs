using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //TODO:キャラ側→一括中央管理に変える
    //ちびキャラ、人以外などに対応しきれていない
    public class AttachPointGenerator : MonoBehaviour
    {
        public AttachPoint anchorPointPrefab;
        [SerializeField] List<AttachPoint> anchorList = new List<AttachPoint>();
        Animator _animator;
        Dictionary<HumanBodyBones, float> dicAttachPoint = new Dictionary<HumanBodyBones, float>();

        [SerializeField] bool _isCustomize = false;//現状SD専用

        CharaController _charaCon;
        TimelineController _timeline;

        [SerializeField] float _height = 0;//身長はとりあえず図れるが、他がうまくいかないと無意味(初期姿勢バグってる奴も直さないといけない)

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _charaCon = transform.GetComponent<CharaController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();

            Init();
        }

        void Init()
        {
            if (anchorList != null && anchorList.Count > 0) return;//Prefab対策

            //身長を図る(UI上ベース約0.15～0.35くらい)
            //height = anime.GetBoneTransform(HumanBodyBones.Head).position.y - anime.GetBoneTransform(HumanBodyBones.RightFoot).position.y;
            //直すの面倒なので座高(UI上ベース約0.07～0.12くらい)
            _height = _animator.GetBoneTransform(HumanBodyBones.Head).position.y - _animator.GetBoneTransform(HumanBodyBones.Spine).position.y;


            //約胸～頭までの距離が短ければミニキャラと認定する
            //var dir = anime.GetBoneTransform(HumanBodyBones.Head).position - anime.GetBoneTransform(HumanBodyBones.Neck).parent.position;
            //if (dir.sqrMagnitude < 0.035f) isMiniChara = true;

            //SD用
            if (_isCustomize)
            {
                dicAttachPoint = new Dictionary<HumanBodyBones, float>()
                {
                    //offset座標
                    { HumanBodyBones.LeftHand, 0f},
                    { HumanBodyBones.RightHand,  0f},
                    { HumanBodyBones.Head,0.16f},
                    { HumanBodyBones.Chest,0f},
                    //{ HumanBodyBones.Spine,0f}//腰
                };

                foreach (var e in dicAttachPoint)
                {
                    //アタッチオブジェ生成
                    var attachPoint = Instantiate(anchorPointPrefab.gameObject, transform.position, Quaternion.identity);
                    var attachPointScript = attachPoint.GetComponent<AttachPoint>();
                    attachPointScript.myCharaCon = _charaCon;

                    //パラメータ設定
                    attachPoint.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), e.Key);
                    attachPoint.transform.parent = _animator.GetBoneTransform(e.Key);
                    attachPoint.transform.localRotation = Quaternion.identity;

                    switch (e.Key)
                    {
                        case HumanBodyBones.Head:
                            attachPoint.transform.localPosition = new Vector3(0, 0, e.Value);
                            attachPoint.transform.localScale = Vector3.one * 0.5f;
                            break;
                        case HumanBodyBones.LeftHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        case HumanBodyBones.RightHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;

                        default:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.35f;
                            break;
                    }
                    anchorList.Add(attachPointScript);
                }
            }
            //一般
            else
            {
                dicAttachPoint = new Dictionary<HumanBodyBones, float>()
                {
                    //offset座標
                    { HumanBodyBones.LeftHand, 0.05f},
                    { HumanBodyBones.RightHand,  0.05f},
                    { HumanBodyBones.Head,0.1f},
                    { HumanBodyBones.Chest,0.05f},
                    { HumanBodyBones.Spine,-0.03f}//腰
                };

                foreach (var e in dicAttachPoint)
                {
                    //アタッチオブジェ生成
                    var attachPoint = Instantiate(anchorPointPrefab.gameObject, transform.position, Quaternion.identity);
                    var attachPointScript = attachPoint.GetComponent<AttachPoint>();
                    attachPointScript.myCharaCon = _charaCon;

                    //パラメータ設定
                    attachPoint.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), e.Key);
                    if (e.Key == HumanBodyBones.Chest)
                    {
                        Transform chest = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
                        if (!chest) chest = _animator.GetBoneTransform(HumanBodyBones.Neck)?.parent;
                        if (!chest) chest = _animator.GetBoneTransform(HumanBodyBones.Head)?.parent;
                        //セットする
                        attachPoint.transform.parent = chest;
                    }
                    else attachPoint.transform.parent = _animator.GetBoneTransform(e.Key);
                    attachPoint.transform.localRotation = Quaternion.identity;

                    switch (e.Key)
                    {
                        //case HumanBodyBones.Head:
                        //    attachPoint.transform.localPosition = Vector3.zero;
                        //    attachPoint.transform.position += new Vector3(0, e.Value, 0);
                        //    attachPoint.transform.localScale = Vector3.one * 0.45f;
                        //    break;
                        case HumanBodyBones.Chest:
                            if (_height >= 0.11f) attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            else attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.4f;
                            break;
                        case HumanBodyBones.LeftHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        case HumanBodyBones.RightHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        default:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            //attachPoint.transform.position += new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.4f;
                            break;
                    }
                    anchorList.Add(attachPointScript);
                }
            }

            //無効化しておく
            SetActive_AttachPoint(false);
        }

        public void Update()
        {
            //左手のアタッチポイントのアイテム数確認
            if (anchorList[0].transform.childCount == 0)
            {
                //握っていたら解除する
                if (_charaCon.keepHandL_Anime) _timeline.SwitchHandType(_charaCon, false, true);
            }
            //右手のアタッチポイントのアイテム数確認
            if (anchorList[1].transform.childCount == 0)
            {
                //握っていたら解除する
                if (_charaCon.keepHandR_Anime) _timeline.SwitchHandType(_charaCon, false, false);
            }
        }

        public void SetActive_AttachPoint(bool isActive)
        {
            foreach (var anchor in anchorList)
            {
                anchor.SetActive(isActive);
            }
        }
    }

}