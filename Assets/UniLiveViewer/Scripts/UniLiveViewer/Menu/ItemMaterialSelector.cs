using UnityEngine;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// 不具合有
    /// </summary>
    public class ItemMaterialSelector : MonoBehaviour
    {
        [SerializeField] MeshRenderer[] _quads = new MeshRenderer[8];//候補とりあえず8
        [SerializeField] Transform _currentQuad;//カーソルの役割
        Vector3 _currentQuadOffset = new Vector3(0, 0, 0.01f);//zファイ対策
        [SerializeField] TextMesh _textMesh;
        int _current = 0;
        int _limitTex;

        DecorationItemInfo _itemInfo;

        /// <summary>
        /// アイテム名、候補テクスチャをセット
        /// </summary>
        public void Initialize(DecorationItemInfo info , int languageCode)
        {
            _itemInfo = info;
            _textMesh.text = _itemInfo.ItemName[languageCode];

            if (info.RenderInfo.Length == 0)
            {
                _current = 0;
                for (int i = 0; i < _quads.Length; i++)
                {
                    if (_quads[i].gameObject.activeSelf) _quads[i].gameObject.SetActive(false);
                }
            }
            else
            {
                _current = _itemInfo.RenderInfo[0].data.textureCurrent;
                _limitTex = _itemInfo.RenderInfo[0].data.chooseableTexture.Length;

                for (int i = 0; i < _quads.Length; i++)
                {
                    if (i < _limitTex)
                    {
                        if (!_quads[i].gameObject.activeSelf) _quads[i].gameObject.SetActive(true);
                        _quads[i].material.SetTexture("_BaseMap", _itemInfo.RenderInfo[0].data.chooseableTexture[i]);
                    }
                    else
                    {
                        if (_quads[i].gameObject.activeSelf) _quads[i].gameObject.SetActive(false);
                    }
                }
            }
            //カーソル移動
            UpdateCursor();
        }

        public bool TrySetTexture(int nextCurrent)
        {
            if (nextCurrent < _limitTex && _current != nextCurrent)
            {
                _current = nextCurrent;
                _itemInfo.SetTexture(0, _current);//現状は0しかないので固定

                //カーソル移動
                UpdateCursor();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Currentへカーソル画像を移動する
        /// </summary>
        void UpdateCursor()
        {
            _currentQuad.parent = _quads[_current].transform;
            _currentQuad.transform.localPosition = _currentQuadOffset;
            _currentQuad.transform.localRotation = Quaternion.identity;
        }
    }
}