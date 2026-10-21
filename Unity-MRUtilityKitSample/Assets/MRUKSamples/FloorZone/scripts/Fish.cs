// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Threading;
using System.Threading.Tasks;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// Controls fish behavior including swimming patterns, bait attraction, and hooking mechanics.
    /// Manages fish stamina, movement, and interaction with the fishing rod.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class Fish : MonoBehaviour
    {
        private const float FISH_RESET_DELAY = 3f;

        public Vector3 CenterSwimRangeStart;
        public float Speed = .5f;
        public float StartSpeed = .5f;
        public float SwimRangeStart = 1f;
        public float SwimDepth = .5f;
        public Vector3 CenterSwimRange;
        public float SwimRange;
        public float BaitAttractionLerp = .1f;
        public float MotionLerp = 5f;
        public Slider StaminaSlider;
        public RectTransform FillArea;
        public bool IsCaught;
        public float NoiseEvolution;
        public GameObject SplashPrefab;
        public GameObject SmallSplashPrefab;

        [SerializeField] private GameObject _fishVisual;
        [SerializeField] private float _swimDepthStart = .5f;
        [SerializeField] private int _rodLayerIndex;
        [SerializeField] private int _underWaterLayerIndex;
        [SerializeField] private int _waterLayerIndex;

        public Bounds newSwimBounds;
        private readonly int Caught1 = Animator.StringToHash("Caught");
        private float _fishStamina = 1;
        private float _lastSliderValue = 1;

        private FishingRod _fishingRod;
        private Animator _fishAnimator;
        private GameObject _instancedFish;

        private ParticleSystem _smallSplashParticleSystem;
        private Transform _instancedFishTransform;
        private Transform _smallSplashTransform;

        private CancellationTokenSource _cancellationTokenSource;

        public float FishStamina
        {
            get => _fishStamina;
            set
            {
                _fishStamina = value;
                SliderManager(value);
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Cache transforms and components to avoid repeated lookups
            CenterSwimRangeStart = transform.position;
            _smallSplashTransform = SmallSplashPrefab.transform;
            _cancellationTokenSource = new CancellationTokenSource();
            Initialize();
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsCaught)
            {
                StaminaSlider.gameObject.SetActive(false);
                return;
            }

            if (Speed > StartSpeed)
            {
                Speed = Mathf.Lerp(Speed, StartSpeed, .1f * Time.deltaTime);
            }

            if (_fishingRod._isFloaterInWater && !_fishingRod._fishHooked) //Gets attracted by the bait
            {
                SwimTowardsTheBaitParameters();
            }

            if (!_fishingRod._isFloaterInWater && !_fishingRod._fishHooked)
            {
                SwimFreelyParameters();
            }

            if (_fishingRod._fishHooked)
            {
                SwimWhileHookedParameters();
            }

            Move();
            AnimateSliderWhenGettingTired();
            _lastSliderValue = StaminaSlider.value;
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(CenterSwimRangeStart, Vector3.one * SwimRangeStart);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_fishingRod._fishHooked)
            {
                return;
            }

            if (!other.GetComponentInParent<FishingRod>())
            {
                return;
            }

            _fishingRod._floaterInWater.GetComponent<Rigidbody>().isKinematic = true;
            ChangeRenderLayer(_fishingRod._floaterInWater.gameObject, _underWaterLayerIndex);
            Speed = StartSpeed * 5;
            SwimRange = SwimRangeStart;
            SwimDepth = _swimDepthStart;
            _fishingRod._stringGiven = _fishingRod.GetDistanceBetweenTargetAndUndeformedTip();
            _fishingRod._fishHooked = this;
            _fishingRod._floaterInWater.transform.parent = transform;
            _fishingRod._floaterInWater.transform.localPosition = Vector3.zero;

            var splashPosition = new Vector3(other.transform.position.x, _smallSplashTransform.position.y,
                other.transform.position.z);
            Instantiate(SmallSplashPrefab, splashPosition, _smallSplashTransform.transform.rotation);
            _fishingRod.PlayPerturbedWaterSound();
        }

        private void Initialize()
        {
            if (_instancedFish == null)
            {
                _instancedFish = Instantiate(_fishVisual, transform.parent);
            }

            _instancedFishTransform = _instancedFish.transform;
            _instancedFishTransform.position = transform.position;
            _instancedFishTransform.rotation = transform.rotation;
            _instancedFish.GetComponent<FishJointDelay>().Target = transform;
            _fishAnimator = _instancedFish.GetComponent<Animator>();

            transform.position = CenterSwimRangeStart;
            _fishingRod = FindAnyObjectByType<FishingRod>();
            CenterSwimRange = CenterSwimRangeStart;
            SwimRange = SwimRangeStart;
            SwimDepth = _swimDepthStart;
            Speed = StartSpeed;
            FishStamina = 1;
            IsCaught = false;
            NoiseEvolution = 0;
            if (_fishAnimator)
            {
                _fishAnimator.enabled = false;
            }
        }

        private void AnimateSliderWhenGettingTired()
        {
            if (_fishingRod._isFloaterInWater)
            {
                StaminaSlider.gameObject.SetActive(true);
                FillArea.localScale = StaminaSlider.value < _lastSliderValue
                    ? new Vector3(1, 1 + Mathf.Sin(Time.time * 50) * 0.5f, 1)
                    : Vector3.one;
            }
            else
            {
                StaminaSlider.gameObject.SetActive(false);
            }
        }

        private void Move()
        {
            NoiseEvolution += Time.deltaTime * Speed * FishStamina * (1 / SwimRangeStart);

            var noiseX = (Mathf.PerlinNoise(NoiseEvolution, 0) - 0.5f) * SwimRange;
            var noiseZ = (Mathf.PerlinNoise(NoiseEvolution + 2, 0) - 0.5f) * SwimRange;
            var nextPos = CenterSwimRange + new Vector3(noiseX, SwimDepth, noiseZ);

            var direction = nextPos - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up), MotionLerp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, nextPos, MotionLerp * Time.deltaTime);
        }

        private void SwimTowardsTheBaitParameters()
        {
            CenterSwimRange = Vector3.Lerp(CenterSwimRange, _fishingRod._activeFloater.transform.position,
                BaitAttractionLerp * Time.deltaTime);
            SwimRange = Mathf.Lerp(SwimRange, 0.2f, BaitAttractionLerp * Time.deltaTime);
            SwimDepth = Mathf.Lerp(SwimDepth, 0.01f, BaitAttractionLerp * Time.deltaTime);
        }

        private void SwimFreelyParameters()
        {
            CenterSwimRange = Vector3.Lerp(CenterSwimRange, CenterSwimRangeStart, 5 * Time.deltaTime);
            SwimRange = Mathf.Lerp(SwimRange, SwimRangeStart, 5 * Time.deltaTime);
            SwimDepth = Mathf.Lerp(SwimDepth, _swimDepthStart, 5 * Time.deltaTime);
        }

        private void SwimWhileHookedParameters()
        {
            var fishingRodTipProjOnWater =
                new Vector3(_fishingRod._rodTip.position.x, 0, _fishingRod._rodTip.position.z);
            CenterSwimRange = Vector3.Lerp(fishingRodTipProjOnWater, CenterSwimRangeStart, FishStamina * 2);
            SwimRange = Mathf.Lerp(0.5f, SwimRangeStart, FishStamina);
            SwimDepth = Mathf.Lerp(0, _swimDepthStart * 1.2f, FishStamina);
        }

        public async void Caught()
        {
            try
            {
                if (!_fishAnimator)
                {
                    return;
                }

                IsCaught = true;
                _fishAnimator.transform.position = transform.position;
                _fishAnimator.transform.rotation = transform.rotation;

                _fishAnimator.enabled = true;
                _fishAnimator.SetTrigger(Caught1);

                ChangeRenderLayer(_fishAnimator.gameObject, default);


                var splashPosition = new Vector3(_instancedFishTransform.position.x, 0,
                    _instancedFishTransform.position.z);
                var instancedSplashPrefab =
                    Instantiate(SplashPrefab, splashPosition, SplashPrefab.transform.rotation);

                try
                {
                    await ResetFishAfterDelayAsync(instancedSplashPrefab, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when component is destroyed
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong while catching the fish:  " + e.Message);
            }
        }

        private async Task ResetFishAfterDelayAsync(GameObject splashToDestroy, CancellationToken cancellationToken)
        {
            await Task.Delay((int)(FISH_RESET_DELAY * 1000), cancellationToken);

            // Clean up objects if not cancelled
            if (!cancellationToken.IsCancellationRequested)
            {
                if (_instancedFish != null)
                {
                    DestroyImmediate(_instancedFish.gameObject);
                }

                if (splashToDestroy != null)
                {
                    Destroy(splashToDestroy);
                }

                Initialize();
            }
        }

        private static void ChangeRenderLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
            {
                t.gameObject.layer = layer;
            }
        }


        private void SliderManager(float value)
        {
            StaminaSlider.value = value;
        }
    }
}
