using UnityEngine;

namespace UniLiveViewer.Stage.Title.Kari
{
    public class FlyAround : MonoBehaviour
    {
        [SerializeField] float _lifeTime = 10.0f;
        float _timer = 0;
        Vector3 _direction;
        Vector3 _rot;

        void Start()
        {
            _direction = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.0f, 2.5f), Random.Range(-0.5f, 0.5f));
            _rot = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * Time.deltaTime * 10;
        }

        void Update()
        {
            _direction.y += Time.deltaTime * 0.4f;
            transform.localPosition += _direction * Time.deltaTime;
            transform.localRotation *= Quaternion.Euler(_rot);

            _timer += Time.deltaTime;
            if (_lifeTime < _timer)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}