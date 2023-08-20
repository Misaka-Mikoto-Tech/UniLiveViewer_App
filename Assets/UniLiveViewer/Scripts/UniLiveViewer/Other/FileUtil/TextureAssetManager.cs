using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using NanaCiel;

namespace UniLiveViewer 
{
    public class TextureAssetManager
    {
        Texture2D _texDummy;

        public string[] VrmNames => _vrmNames;
        string[] _vrmNames;

        public IReadOnlyDictionary<string, Sprite> Thumbnails => _thumbnails;
        [SerializeField] Dictionary<string, Sprite> _thumbnails = new Dictionary<string, Sprite>();

        TextureAssetManager()
        {
            _texDummy = Resources.Load<Texture2D>("Texture/NoImage");
        }

        /// <summary>
        /// 暫定
        /// </summary>
        public async UniTask CacheThumbnails(CancellationToken cancellation)
        {
            var charaFolderPath = PathsInfo.GetFullPath(FOLDERTYPE.CHARA) + "/";

            Texture2D texture = null;
            Sprite spr = null;

            await UniTask.Delay(100, cancellationToken: cancellation);

            //全ファイル名を取得
            _vrmNames = GetVrmNames(charaFolderPath);

            for (int i = 0; i < _vrmNames.Length; i++)
            {
                if (spr) spr = null;
                if (texture) texture = null;

                //キャッシュ画像確認
                //texture = cacheThumbnails.FirstOrDefault(x => x.Key == vrmNames[i]).Value;

                spr = _thumbnails.FirstOrDefault(x => x.Key == _vrmNames[i]).Value;
                if (spr != null) continue;

                try
                {
                    //VRMファイルからサムネイルを抽出する
                    texture = await VRMExpansions.GetThumbnail(charaFolderPath + _vrmNames[i], cancellation);

                    if (texture)
                    {
                        //テクスチャ→スプライトに変換
                        spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        //ダミー画像生成
                        texture = GameObject.Instantiate(_texDummy);
                        //テクスチャ→スプライトに変換
                        spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                    }
                    //リストに追加
                    _thumbnails.Add(_vrmNames[i], spr);
#if UNITY_EDITOR
                    texture = TextureFormatter.Resize(texture);
                    //PNG保存
                    var binary = texture.EncodeToPNG();
                    var path = Path.Combine(PathsInfo.GetFullPath_ThumbnailCache() + "/", $"{_vrmNames[i]}.png");
                    File.WriteAllBytes(path, binary);
#elif UNITY_ANDROID
                    // NOTE: この処理Quesstキツイ
                    //texture = TextureFormatter.Resize(texture);

                    //PNG保存
                    var binary = texture.EncodeToPNG();
                    var path = Path.Combine(PathsInfo.GetFullPath_ThumbnailCache() + "/", $"{_vrmNames[i]}.png");
                    File.WriteAllBytes(path, binary);
#endif
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("サムネイルキャッシュ中に中断");
                    throw;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            }
            Debug.Log("サムネイルキャッシュ成功");
        }

        /// <summary>
        /// フォルダ内VRMファイル名を取得
        /// </summary>
        /// <returns></returns>
        string[] GetVrmNames(string folderPath)
        {
            string[] result = null;
            try
            {
                //VRMファイルのみ検索
                result = Directory.GetFiles(folderPath, "*.vrm", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = Path.GetFileName(result[i]);
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// VRMファイルをコピー(download→Chara)
        /// </summary>
        /// <param name="pathMy"></param>
        /// <returns></returns>
        public async UniTask CopyVRMtoCharaFolder(string folderPath,CancellationToken cancellation)
        {
            _vrmNames = GetVrmNames(folderPath);
            try
            {
                string charaFolderPath = PathsInfo.GetFullPath(FOLDERTYPE.CHARA) + "/";
                //ファイルコピー
                for (int i = 0; i < _vrmNames.Length; i++)
                {
                    File.Copy(folderPath + _vrmNames[i], charaFolderPath + _vrmNames[i], true);//上書き保存
                }

                //VRMのサムネイル画像をキャッシュする
                await CacheThumbnails(cancellation);
            }
            catch
            {
                throw;
            }
        }

        

        /// <summary>
        /// 実機にて不具合あるので次回
        /// </summary>
        //private async UniTaskVoid CacheThumbnail_test()
        //{

        //    string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
        //    string folderPath_Cache = GetFullPath_ThumbnailCache();
        //    Texture2D texture = null;

        //    if(cacheThumbnails != null && cacheThumbnails.Count > 0)
        //    {
        //        cacheThumbnails.Clear();
        //        cacheThumbnails = new Dictionary<string, Texture2D>();
        //    }

        //    await UniTask.Yield(PlayerLoopTiming.Update,cancellation_token);

        //    //全ファイル名を取得
        //    var vrmNames = GetAllVRMNames();
        //    for (int i = 0; i < vrmNames.Length; i++)
        //    {
        //        if (texture) texture = null;
        //        //キャッシュ画像確認
        //        var isCache = File.Exists(folderPath_Cache + vrmNames[i] + ".png");
        //        if (isCache)
        //        {
        //            //キャッシュ済み画像を流用
        //            texture = GetCacheThumbnail(vrmNames[i]);

        //            //リストに追加
        //            cacheThumbnails.Add(vrmNames[i], texture);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                //VRMファイルからサムネイルを抽出する
        //                texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

        //                if (texture)
        //                {
        //                    //リサイズ
        //                    texture = ResizeTexture(texture, 256, 256);
        //                    //色情報取得(CPU側の処理では色の情報やらを取得できず灰色画像になるので)
        //                    texture = GetColorInfo(texture);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //                else
        //                {
        //                    //ダミー画像生成
        //                    texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //            }
        //            catch (System.OperationCanceledException)
        //            {
        //                Debug.Log("サムネイルキャッシュ中に中断");
        //                throw;
        //            }
        //        }
        //        await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        //    }

        //    //完了した
        //    onThumbnailCompleted?.Invoke();
        //}

        /// <summary>
        /// テクスチャのリサイズ
        /// </summary>
        /// <param name="srcTexture"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        //public static Texture2D ResizeTexture(Texture2D srcTexture, int newWidth, int newHeight)
        //{
        //    //指定しないとRGBA32になってしまったので一応
        //    var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
        //    Graphics.ConvertTexture(srcTexture, resizedTexture);
        //    return resizedTexture;
        //}

        /// <summary>
        /// シェーダー側で描画してテクスチャに書き込むたぶん（大体コピペ）
        /// デプス系を無効化しないとQuest実機では動かなかったので用意
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        //public static Texture2D GetColorInfo(Texture2D texture2D)
        //{
        //    //Texture mainTexture = _renderer.material.mainTexture;
        //    //RenderTexture currentRT = RenderTexture.active;
        //    RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 0);//第三デプス
        //    //Linear→Gamma
        //    Graphics.Blit(texture2D, renderTexture, _renderer.sharedMaterial);//デプス無効化したマテリアル
        //    //RenderTexture情報→texture2Dへ
        //    RenderTexture.active = renderTexture;
        //    texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //    texture2D.Apply();

        //    //RenderTexture.ReleaseTemporary(renderTexture);


        //    //Color[] pixels = texture2D.GetPixels();
        //    //RenderTexture.active = currentRT;

        //    return texture2D;
        //}

        /// <summary>
        /// キャッシュしたサムネイルを取得
        /// </summary>
        /// <param name="filePath">.png</param>
        /// <returns></returns>
        //private static Texture2D GetCacheThumbnail(string fileName)
        //{
        //    string filePath = Path.Combine(GetFullPath_ThumbnailCache() + "/", $"{fileName}.png");
        //    byte[] bytes = File.ReadAllBytes(filePath);
        //    Texture2D texture = new Texture2D(64, 64);
        //    texture.LoadImage(bytes);
        //    return texture;
        //}
    }

}
