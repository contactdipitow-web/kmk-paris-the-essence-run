using UnityEngine;

namespace KMK.EssenceRun
{
    public sealed class KmkCameraRig : MonoBehaviour
    {
        public Camera Camera { get; private set; }

        private KmkGame _game;
        private RunnerController _player;
        private Vector3 _velocity;
        private float _shake;
        private float _fovPunch;
        private Color _targetBackground;

        public void Initialize(KmkGame game, RunnerController player)
        {
            _game = game;
            _player = player;

            gameObject.tag = "MainCamera";
            Camera = gameObject.AddComponent<Camera>();
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.nearClipPlane = 0.12f;
            Camera.farClipPlane = 180f;
            Camera.fieldOfView = 58f;
            Camera.allowHDR = true;
            Camera.allowMSAA = true;
            gameObject.AddComponent<AudioListener>();

            ResetRig();
        }

        public void ResetRig()
        {
            if (_player == null)
            {
                return;
            }

            transform.position = _player.transform.position + new Vector3(0f, 3.9f, -8.2f);
            transform.LookAt(_player.transform.position + new Vector3(0f, 1.3f, 7f));
            _velocity = Vector3.zero;
            _shake = 0f;
            _fovPunch = 0f;
        }

        private void LateUpdate()
        {
            if (_player == null || _game == null)
            {
                return;
            }

            Vector3 playerPosition = _player.transform.position;
            Vector3 offset;
            Vector3 lookTarget;
            float targetFov;

            if (_game.State == KmkGameState.Menu)
            {
                float orbit = Mathf.Sin(Time.unscaledTime * 0.32f) * 1.25f;
                offset = new Vector3(orbit, 3.4f, -7.0f);
                lookTarget = playerPosition + new Vector3(0f, 1.2f, 3.2f);
                targetFov = 54f;
            }
            else if (_game.State == KmkGameState.GameOver)
            {
                offset = new Vector3(2.0f, 2.8f, -5.8f);
                lookTarget = playerPosition + new Vector3(0f, 1.1f, 1.4f);
                targetFov = 51f;
            }
            else
            {
                float laneSway = playerPosition.x * 0.20f;
                offset = new Vector3(laneSway, 3.8f + playerPosition.y * 0.12f, -8.1f);
                lookTarget = playerPosition + new Vector3(0f, 1.35f, 8.8f);
                targetFov = Mathf.Lerp(57f, 68f, Mathf.InverseLerp(10.5f, 23f, _game.CurrentSpeed));
            }

            _shake = Mathf.MoveTowards(_shake, 0f, Time.unscaledDeltaTime * 2.8f);
            _fovPunch = Mathf.MoveTowards(_fovPunch, 0f, Time.unscaledDeltaTime * 18f);
            Vector3 randomShake = Random.insideUnitSphere * _shake;
            Vector3 desiredPosition = playerPosition + offset + randomShake;
            float smoothTime = _game.State == KmkGameState.Playing ? 0.09f : 0.18f;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime, 60f, Time.unscaledDeltaTime);

            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-8.5f * Time.unscaledDeltaTime));
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, targetFov + _fovPunch, 1f - Mathf.Exp(-7f * Time.unscaledDeltaTime));
            Camera.backgroundColor = Color.Lerp(Camera.backgroundColor, _targetBackground, 1f - Mathf.Exp(-2.4f * Time.unscaledDeltaTime));
        }

        public void ApplyTheme(ThemePalette palette, bool immediate)
        {
            _targetBackground = palette.Sky;
            if (Camera != null && immediate)
            {
                Camera.backgroundColor = palette.Sky;
            }
        }

        public void PunchCollect()
        {
            _fovPunch = Mathf.Min(3.2f, _fovPunch + 1.15f);
            _shake = Mathf.Min(0.11f, _shake + 0.035f);
        }

        public void PunchHit()
        {
            _fovPunch = -6f;
            _shake = 0.65f;
        }
    }
}
