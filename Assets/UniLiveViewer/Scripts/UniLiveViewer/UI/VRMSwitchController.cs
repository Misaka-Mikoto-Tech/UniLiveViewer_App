using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VRM.FirstPersonSample;
using NanaCiel;

namespace UniLiveViewer
{
    //[RequireComponent(typeof(FileAccessManager))]
    public class VRMSwitchController : MonoBehaviour
    {
        [SerializeField] private Transform[] pageTransform;
        [SerializeField] private Transform displayAnchor;

        [Space(1)]
        [Header("＜1ページ＞")]
        [SerializeField] private TextMesh textDirectory;
        [SerializeField] private VRMRuntimeLoader_Custom runtimeLoader;//サンプルをそのまま利用する
        [SerializeField] private Button_Base btnPrefab;
        private List<TextMesh> btnTexts = new List<TextMesh>();
        [SerializeField] private Transform btnParent;
        [SerializeField] private LoadAnimation anime_Loading;

        [Space(1)]
        [Header("＜2ページ＞")]
        [SerializeField] private Transform vrmPresetAnchor;//マテリアル調整時用のキャラ座標アンカー
        [SerializeField] private RollSelector rollSelector_Material;
        [SerializeField] private Button_Switch[] btn_SuefaceType = new Button_Switch[2];
        [SerializeField] private Button_Switch[] btn_RenderFace = new Button_Switch[3];
        [SerializeField] private SliderGrabController slider_Transparent = null;
        [SerializeField] private Button_Base btn_AllReset;
        [SerializeField] private Button_Base btn_SetOK;
        private GameObject vrmModel;
        private MaterialConverter converter;

        [Header("＜アタッチャー＞")]
        [SerializeField] private ComponentAttacher_VRM attacherPrefab;

        [Header("＜その他＞")]
        //特殊表情用サウンド
        [SerializeField] private AudioClip[] specialFaceAudioClip;
        //クリックSE
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//ボタン音,読み込み音,クリック音                               
        //VRM読み込み時イベント
        public event Action<CharaController> VRMAdded;
        //ファイルアクセスとサムネの管理
        private FileAccessManager fileManager;
        //private Dictionary<string, Sprite> dicVRMSprite = new Dictionary<string, Sprite>();
        //当たり判定
        private VRMTouchColliders touchCollider = null;

        private int currentPage = 0;
        private CancellationToken cancellation_token;
        private int[] randomBox;
        private string[] vrmNames;

        private void Awake()
        {
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
            touchCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<VRMTouchColliders>();

            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            //コールバック登録・・・2ページ目
            foreach (var e in btn_SuefaceType)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            foreach (var e in btn_RenderFace)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            slider_Transparent.ValueUpdate += MaterialSetting_TransparentColor;
            rollSelector_Material.onTouch += MaterialInfoUpdate;
            btn_AllReset.onTrigger += MaterialSetting_AllReset;
            btn_SetOK.onTrigger += MaterialSetting_SetOK;

            cancellation_token = this.GetCancellationTokenOnDestroy();

            //サムネ用ボタンの生成
            CreateThumbnailButtons(cancellation_token).Forget();
        }

        /// <summary>
        /// サムネ用の空ボタン生成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTaskVoid CreateThumbnailButtons(CancellationToken token)
        {

            Vector2 btnPos;
            Button_Base btn;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    //座標調整
                    btnPos.x = -0.3f + (j * 0.15f);
                    btnPos.y = 0 - (i * 0.15f);

                    //生成
                    btn = Instantiate(btnPrefab);

                    //親設定
                    btn.transform.parent = btnParent;
                    btn.transform.localPosition = new Vector3(btnPos.x, btnPos.y, 0);
                    btn.transform.localRotation = Quaternion.identity;

                    //コールバック登録
                    btn.onTrigger += (b) => LoadVRM(b).Forget();

                    //テキストメッシュをリストに加える
                    btnTexts.Add(btn.transform.GetChild(1).GetComponent<TextMesh>());
                    btn = null;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>
        /// UIの表示状態を変更
        /// </summary>
        /// <param name="isHide"></param>
        public void SetUIView(bool isHide)
        {
            if (isHide)
            {
                if (displayAnchor.gameObject.activeSelf) displayAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!displayAnchor.gameObject.activeSelf) displayAnchor.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// カレントページで開き直す（初期化）
        /// </summary>
        public void initPage()
        {
            //UIを表示
            SetUIView(false);

            //ページスイッチ
            for (int i = 0; i < pageTransform.Length; i++)
            {
                if (currentPage == i)
                {
                    if (!pageTransform[i].gameObject.activeSelf) pageTransform[i].gameObject.SetActive(true);
                }
                else
                {
                    if (pageTransform[i].gameObject.activeSelf) pageTransform[i].gameObject.SetActive(false);
                }
            }

            //各初期化処理
            switch (currentPage)
            {
                case 0:
                    if (fileManager.isSuccess)
                    {
                        //フォルダパスの表示を更新
                        textDirectory.text = $"({FileAccessManager.folderPath_Custom})";

                        //ローディングアニメーションを無効状態
                        anime_Loading.gameObject.SetActive(false);

                        //サムネボタンアンカーを有効状態
                        btnParent.gameObject.SetActive(true);

                        //VRM選択ボタンを生成する
                        SetThumbnail(cancellation_token).Forget();
                    }
                    break;
                case 1:
                    //VRMが存在していればUIを更新
                    if (vrmPresetAnchor.GetChild(0))
                    {
                        //回転セレクターにマテリアル名のリストを渡す
                        List<string> strList = new List<string>();
                        foreach (var e in converter.materials)
                        {
                            strList.Add(e.name);
                        }
                        rollSelector_Material.init(strList);

                        //表示を更新
                        MaterialInfoUpdate();
                    }
                    break;
            }
        }



        /// <summary>
        /// VRMの数だけサムネボタンを生成する
        /// </summary>
        private async UniTaskVoid SetThumbnail(CancellationToken token)
        {
            //一旦全部非表示
            for (int i = 0; i < btnTexts.Count; i++)
            {
                if (btnTexts[i].transform.parent.gameObject.activeSelf)
                {
                    btnTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }

            if (randomBox == null)
            {
                //全VRMファイル名を取得
                var array = fileManager.GetAllVRMNames();
                //最大15件に丸める
                if (array.Length > 15)
                {
                    vrmNames = array.Take(15).ToArray();
                }
                else
                {
                    vrmNames = array;
                }
                //ランダム配列を設定
                randomBox = new int[vrmNames.Length];
                for (int i = 0; i < randomBox.Length; i++) randomBox[i] = i;
                randomBox = Shuffle(randomBox);
            }
            await UniTask.Delay(10, cancellationToken: token);

            Button_Base baseButton;
            Sprite spr = null;
            //Texture2D texture = null;
            int index = 0;

            //必要なボタンのみ有効化して設定する
            for (int i = 0; i < vrmNames.Length; i++)
            {
                //ランダムなボタン順
                index = randomBox[i];

                UniTask.Void(async () =>
                {
                    baseButton = btnTexts[index].transform.parent.GetComponent<Button_Base>();

                    if (!baseButton.gameObject.activeSelf)
                    {
                        baseButton.gameObject.SetActive(true);
                    }
                    //オブジェクト名を変更
                    baseButton.name = vrmNames[index];
                    //ボタンの表示名を変更
                    btnTexts[index].text = vrmNames[index];
                    //文字サイズを調整する
                    btnTexts[index].fontSize = btnTexts[index].text.FontSizeMatch(500, 25, 40);

                    //サムネイルを取得
                    try
                    {
                        spr = FileAccessManager.cacheThumbnails[vrmNames[index]];

                        if (spr)
                        {
                            //サムネの容量で有無を判定
                            //float size = texture.GetRawTextureData().LongLength;
                            //サムネイル無し判定
                            //if (size < 10)
                            //    {
                            //        //defaultのまま
                            //    }
                            //    //サムネイル有り判定
                            //    else
                            //    {
                            //    }

                            //スプライトをセット
                            baseButton.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
                            //baseButton.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", texture);
                        }
                        else
                        {
                            //defaultのまま
                        }
                    }
                    catch
                    {
                        if (!spr)
                        {
                            //Debug.Log("ロジックエラー。アプリを立ち上げ後にキャッシュ画像を削除した？");
                            //対策としてボタンを非表示
                            if (btnTexts[index].transform.parent.gameObject.activeSelf)
                            {
                                btnTexts[index].transform.parent.gameObject.SetActive(false);
                            }
                        }
                    }
                    finally
                    {
                        spr = null;

                        if (i % 2 == 0)
                        {
                            //時間差で読み込み音を鳴らす
                            await UniTask.Delay(500, cancellationToken: token);
                            //yield return new WaitForSeconds(0.5f);
                            audioSource.PlayOneShot(Sound[1]);
                            //StartCoroutine(DelaySound());
                        }
                    }
                });

                //時間差で読み込ませる
                if (i % 2 == 0) await UniTask.Delay(250, cancellationToken: token);
            }
        }

        /// <summary>
        /// ランダムシャッフル（ランダムな2要素を交換→シャッフルされない要素もありえる）
        /// </summary>
        /// <param name="num"></param>
        int[] Shuffle(int[] inputArray)
        {
            for (int i = 0; i < inputArray.Length; i++)
            {
                int temp = inputArray[i];
                int randomIndex = UnityEngine.Random.Range(0, inputArray.Length);
                inputArray[i] = inputArray[randomIndex];
                inputArray[randomIndex] = temp;
            }
            return inputArray;
        }

        //private void UniTask_Test(Vector2 setPos,string fileName)
        //{

        //    //名前照合で登録済みのスプライトがあれば取得
        //    Sprite spr = dicVRMSprite.FirstOrDefault(x => x.Key == fileName).Value;
        //    if (spr)
        //    {
        //        //ボタンのSpriteRenderにセット
        //        btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
        //    }
        //    else
        //    {
        //        //VRMファイルからサムネのテクスチャを取得
        //        //string fullPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA) + vrmNames[i];
        //        //string fullPath = FileAccessManager.GetFullPath_ThumbnailCash() +  + ".png";
        //        var texture = FileAccessManager.GetCacheThumbnail(fileName);
        //        //var texture = await runtimeLoader.GetThumbnail(fullPath);

        //        if (texture)
        //        {
        //            float size = texture.GetRawTextureData().LongLength;

        //            //サムネイル無し判定
        //            if (size < 10)
        //            {
        //                //defaultのスプライトを取得
        //                spr = btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite;
        //            }
        //            //サムネイル有り判定
        //            else
        //            {
        //                //テクスチャ→スプライトに変換
        //                spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //                //ボタンのSpriteRenderにセット
        //                btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("プログラムエラー。キャッシュ無し");

        //            //defaultのスプライトを取得
        //            //spr = btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite;
        //        }
        //        //名前とサムネを登録する
        //        dicVRMSprite.Add(fileName, spr);
        //    }
        //    //コールバック登録
        //    btn.GetComponent<Button_Base>().onTrigger += OpenVRM;
        //}

        /// <summary>
        /// VRMを読み込む
        /// </summary>
        /// <param name="btn">該当サムネボタン</param>
        private async UniTaskVoid LoadVRM(Button_Base btn)
        {
            //重複クリックできないようにボタンを無効化
            btnParent.gameObject.SetActive(false);

            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            //ローディングアニメーションを一旦非表示
            //if (anime_Loading.gameObject.activeSelf) anime_Loading.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);

            //ローディングアニメーション開始
            anime_Loading.gameObject.SetActive(true);

            try
            {
                //VRM設定
                await SetVRM(btn, cancellation_token);
            }
            catch (Exception e)
            {
                Debug.Log("ロード失敗:" + e);
            }
            finally
            {
                //ローディングアニメーション終了
                anime_Loading.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// ※vrmModel取得後に非同期にかかわらずwaitを挟むとモデルの揺れ物がおかしな状態になる原因不明
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask SetVRM(Button_Base btn, CancellationToken token)
        {
            try
            {
                //SampleUIを有効化
                runtimeLoader.gameObject.SetActive(true);

                await UniTask.Delay(10, cancellationToken: token);

                //指定パスのVRMのみ読み込む
                //string fileName = btn.transform.GetChild(1).GetComponent<TextMesh>().text;
                string fileName = btn.transform.name;
                string fullPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA) + fileName;

                //if (vrmModel) vrmModel = null;
                vrmModel = await runtimeLoader.OnOpenClicked_VRM(fullPath, token);

                //キャンセル確認
                token.ThrowIfCancellationRequested();

                //最低限の設定
                vrmModel.name = fileName;
                vrmModel.tag = "Grab_Chara";
                vrmModel.layer = LayerMask.NameToLayer("GrabObject");
                var characon = vrmModel.AddComponent<CharaController>();

                //Meshが消える対策
                var meshs = characon.GetComponentsInChildren<SkinnedMeshRenderer>();
                //Bounds bounds;
                foreach (var mesh in meshs)
                {
                    //上手くいかない
                    //bounds = mesh.bounds;
                    //bounds.Expand(Vector3.one);
                    //mesh.bounds = bounds;

                    //良くないがこれで
                    mesh.updateWhenOffscreen = true;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //各種component追加
                var attacher = Instantiate(attacherPrefab.gameObject).GetComponent<ComponentAttacher_VRM>();
                await attacher.Init(vrmModel.transform, touchCollider,token);

                Destroy(attacher.gameObject);

                //マテリアルコンバーターの追加
                if (converter) converter = null;
                converter = vrmModel.AddComponent<MaterialConverter>();
                converter.InitMaterials();

                //読み込んだVRM
                if (characon)
                {
                    // TODO:シェーダーどうするか

                    //調整が必要なマテリアルがあるか
                    if (converter.materials != null && converter.materials.Count > 0)
                    {
                        //VRMをプリセットアンカーに移動
                        vrmModel.transform.parent = vrmPresetAnchor;
                        vrmModel.transform.localPosition = Vector3.zero;
                        vrmModel.transform.localRotation = Quaternion.identity;

                        //マテリアル設定ページへ
                        currentPage = 1;
                        initPage();
                    }
                    else
                    {
                        //ページを最初へ
                        currentPage = 0;

                        //早すぎると揺れものが半端な位置で固まる(正常な位置に落ち着くまでインスタンス化も禁止)
                        await UniTask.Delay(700, cancellationToken: token);

                        //VRM追加した
                        VRMAdded?.Invoke(characon);

                        vrmModel.gameObject.SetActive(false);//無効化しておく

                        //UIを非表示にする
                        SetUIView(true);

                        vrmModel = null;//管理を解除
                    }

                }
            }
            catch
            {
                if (vrmModel) Destroy(vrmModel);
                vrmModel = null;

                //ページを最初へ
                currentPage = 0;

                //サンプルUIを無効状態に(親がDisableだとVRMの各Awakeが走らない)
                runtimeLoader.gameObject.SetActive(false);

                //UIを非表示にする
                SetUIView(true);

                Debug.Log("VRM読み込みエラー");
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// マテリアルの表示情報を更新する
        /// </summary>
        private void MaterialInfoUpdate()
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            int current = rollSelector_Material.current;
            var type = (MaterialConverter.SurfaceType)converter.materials[current].GetFloat("_Surface");
            var face = (MaterialConverter.RenderFace)converter.materials[current].GetFloat("_Cull");
            var color = converter.materials[current].GetColor("_BaseColor");

            //buttonに反映
            if (type == MaterialConverter.SurfaceType.Opaque)
            {
                btn_SuefaceType[0].isEnable = true;
                btn_SuefaceType[1].isEnable = false;
                //スライダー無効化
                slider_Transparent.gameObject.SetActive(false);
            }
            else if (type == MaterialConverter.SurfaceType.Transparent)
            {
                btn_SuefaceType[0].isEnable = false;
                btn_SuefaceType[1].isEnable = true;
                //スライダー有効化
                slider_Transparent.gameObject.SetActive(true);
                slider_Transparent.Value = color.a;
            }
            //buttonに反映
            if (face == MaterialConverter.RenderFace.Front)
            {
                btn_RenderFace[0].isEnable = true;
                btn_RenderFace[1].isEnable = false;
                btn_RenderFace[2].isEnable = false;
            }
            else if (face == MaterialConverter.RenderFace.Back)
            {
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[1].isEnable = true;
                btn_RenderFace[2].isEnable = false;
            }
            else if (face == MaterialConverter.RenderFace.Both)
            {
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[1].isEnable = false;
                btn_RenderFace[2].isEnable = true;
            }
        }

        /// <summary>
        /// マテリアル設定を変更
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_Change(Button_Base btn)
        {
            int current = rollSelector_Material.current;

            if (btn == btn_SuefaceType[0])
            {
                converter.SetSurface(current, MaterialConverter.SurfaceType.Opaque);
            }
            else if (btn == btn_SuefaceType[1])
            {
                converter.SetSurface(current, MaterialConverter.SurfaceType.Transparent);
            }
            else if (btn == btn_RenderFace[0])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Front);
            }
            else if (btn == btn_RenderFace[1])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Back);
            }
            else if (btn == btn_RenderFace[2])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Both);
            }

            //UI表示を更新
            MaterialInfoUpdate();
        }

        /// <summary>
        /// マテリアルの透明色を設定
        /// </summary>
        private void MaterialSetting_TransparentColor()
        {
            //透明を更新
            converter.SetColor_Transparent(rollSelector_Material.current, slider_Transparent.Value);
        }

        /// <summary>
        /// マテリアル設定をリセット
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_AllReset(Button_Base btn)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            //マテリアルをリセット
            converter.ResetMaterials();

            //UI表示を更新
            MaterialInfoUpdate();
        }

        /// <summary>
        /// マテリアル設定の確定
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_SetOK(Button_Base btn)
        {
            //ページを最初へ
            currentPage = 0;

            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            vrmModel.transform.parent = runtimeLoader.transform;
            vrmModel.transform.localPosition = Vector3.zero;
            vrmModel.transform.localRotation = Quaternion.identity;

            vrmModel.gameObject.SetActive(false);//無効化しておく

            //UIを非表示にする
            SetUIView(true);

            //VRM追加した
            VRMAdded?.Invoke(vrmModel.GetComponent<CharaController>());

            vrmModel = null;//管理を解除
        }
    }
}