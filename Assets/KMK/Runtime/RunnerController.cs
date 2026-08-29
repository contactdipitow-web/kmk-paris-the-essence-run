using UnityEngine;

namespace KMK.EssenceRun
{
    public sealed class RunnerController : MonoBehaviour
    {
        public int Lane { get; private set; }
        public bool IsSliding { get { return _slideTimer > 0f; } }
        public bool IsAirborne { get { return transform.position.y > 0.55f; } }
        public RunnerAvatar Avatar { get; private set; }

        private KmkGame _game;
        private Rigidbody _body;
        private CapsuleCollider _collider;
        private float _targetX;
        private float _xVelocity;
        private float _verticalVelocity;
        private float _slideTimer;
        private bool _grounded = true;
        private Vector2 _touchStart;
        private bool _touchTracking;

        public void Initialize(KmkGame game)
        {
            _game = game;
            gameObject.layer = 0;

            _body = gameObject.AddComponent<Rigidbody>();
            _body.isKinematic = true;
            _body.useGravity = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            _collider = gameObject.AddComponent<CapsuleCollider>();
            _collider.radius = 0.42f;
            _collider.height = 2.35f;
            _collider.center = new Vector3(0f, 1.175f, 0f);

            GameObject avatarObject = new GameObject("Procedural 3D Tyson");
            avatarObject.transform.SetParent(transform, false);
            Avatar = avatarObject.AddComponent<RunnerAvatar>();
            Avatar.Build();

            ResetRunner();
        }

        public void ResetRunner()
        {
            Lane = 1;
            _targetX = KmkConstants.LanePosition(Lane);
            _xVelocity = 0f;
            _verticalVelocity = 0f;
            _slideTimer = 0f;
            _grounded = true;
            transform.position = new Vector3(_targetX, KmkConstants.GroundY, 0f);
            transform.rotation = Quaternion.identity;
            RestoreCollider();

            if (Avatar != null)
            {
                Avatar.ResetPose();
            }
        }

        private void Update()
        {
            HandleKeyboard();
            HandleTouch();

            float normalizedSpeed = _game == null ? 0f : Mathf.InverseLerp(0f, 23f, _game.CurrentSpeed);
            if (Avatar != null)
            {
                Avatar.Tick(normalizedSpeed, _game.State, IsAirborne, IsSliding);
            }
        }

        private void FixedUpdate()
        {
            if (_game == null || _game.State != KmkGameState.Playing)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            Vector3 position = _body.position;
            position.x = Mathf.SmoothDamp(position.x, _targetX, ref _xVelocity, 0.085f, 30f, dt);
            position.z += _game.CurrentSpeed * dt;

            if (!_grounded)
            {
                _verticalVelocity -= 25f * dt;
                position.y += _verticalVelocity * dt;
                if (position.y <= KmkConstants.GroundY)
                {
                    position.y = KmkConstants.GroundY;
                    _verticalVelocity = 0f;
                    _grounded = true;
                }
            }

            if (_slideTimer > 0f)
            {
                _slideTimer -= dt;
                if (_slideTimer <= 0f)
                {
                    _slideTimer = 0f;
                    RestoreCollider();
                }
            }

            _body.MovePosition(position);
        }

        private void HandleKeyboard()
        {
            if (_game == null || _game.State != KmkGameState.Playing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                ChangeLane(-1);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                ChangeLane(1);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                Slide();
            }
        }

        private void HandleTouch()
        {
            if (_game == null || _game.State != KmkGameState.Playing || Input.touchCount == 0)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _touchStart = touch.position;
                _touchTracking = true;
                return;
            }

            if (!_touchTracking || (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled))
            {
                return;
            }

            _touchTracking = false;
            Vector2 delta = touch.position - _touchStart;
            float threshold = Mathf.Max(42f, Screen.dpi * 0.12f);
            if (delta.magnitude < threshold)
            {
                return;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                ChangeLane(delta.x > 0f ? 1 : -1);
            }
            else if (delta.y > 0f)
            {
                Jump();
            }
            else
            {
                Slide();
            }
        }

        private void ChangeLane(int direction)
        {
            int next = Mathf.Clamp(Lane + direction, 0, 2);
            if (next == Lane)
            {
                return;
            }

            Lane = next;
            _targetX = KmkConstants.LanePosition(Lane);
            _game.NotifyLaneChange();
        }

        private void Jump()
        {
            if (!_grounded || IsSliding)
            {
                return;
            }

            _grounded = false;
            _verticalVelocity = 9.2f;
            _game.NotifyJump();
        }

        private void Slide()
        {
            if (!_grounded)
            {
                return;
            }

            _slideTimer = 0.86f;
            _collider.height = 1.05f;
            _collider.center = new Vector3(0f, 0.525f, 0f);
            _game.NotifySlide();
        }

        private void RestoreCollider()
        {
            if (_collider == null)
            {
                return;
            }

            _collider.height = 2.35f;
            _collider.center = new Vector3(0f, 1.175f, 0f);
        }

        public bool Clears(HazardKind kind)
        {
            switch (kind)
            {
                case HazardKind.JumpBarrier:
                    return IsAirborne;
                case HazardKind.SlideGate:
                    return IsSliding;
                default:
                    return false;
            }
        }

        public void HitObstacle()
        {
            _game.EndRun();
        }
    }
}
