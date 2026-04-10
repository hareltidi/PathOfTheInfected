using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TidiMovementComponent2D.Animation.BlendSpaces.Playables
{
    public class TidiAnimationDriver : MonoBehaviour
    {
        [SerializeField] private int maxInputs = 8;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationLayerMixerPlayable _layerMixer;
        private AnimatorControllerPlayable _controllerPlayable;
        private AnimationPlayableOutput _output;

        private Dictionary<AnimationClip, AnimationClipPlayable> _clipMap;
        private AnimationClip[] _slotClips;
        private readonly List<AnimationClip> _uniqueClips = new();
        private readonly List<float> _uniqueWeights = new();
        private bool _initialized;
        private Animator _animator;
        private bool _hasBaseController;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            Initialize(_animator, maxInputs);
        }

        private void OnDestroy()
        {

            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _initialized = false;
        }


        public void Initialize(Animator animator, int inputCount)
        {
            if (animator == null)
            {
                Debug.LogError("TidiAnimationDriver requires an Animator reference.", this);
                return;
            }

            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            var safeInputCount = Mathf.Max(1, inputCount);

            _graph = PlayableGraph.Create("TD_AnimGraph");

            _output = AnimationPlayableOutput.Create(_graph, "AnimOutput", animator);

            _mixer = AnimationMixerPlayable.Create(_graph, safeInputCount);
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 2);

            _hasBaseController = animator.runtimeAnimatorController != null;
            if (_hasBaseController)
            {
                _controllerPlayable = AnimatorControllerPlayable.Create(_graph, animator.runtimeAnimatorController);
                _graph.Connect(_controllerPlayable, 0, _layerMixer, 0);
                _layerMixer.SetInputWeight(0, 1f);
            }

            _graph.Connect(_mixer, 0, _layerMixer, 1);
            _layerMixer.SetInputWeight(1, 0f);
            _layerMixer.SetLayerAdditive(1, false);

            _output.SetSourcePlayable(_layerMixer);
            _output.SetWeight(_hasBaseController ? 1f : 0f);

            _clipMap = new Dictionary<AnimationClip, AnimationClipPlayable>();
            _slotClips = new AnimationClip[safeInputCount];
            _mixer.SetInputCount(safeInputCount);

            for (int i = 0; i < safeInputCount; i++)
            {
                _mixer.SetInputWeight(i, 0f);
            }

            _graph.Play();
            _initialized = true;
        }

        public void Clear(bool restoreDefaultPose = true)
        {
            if (!_initialized || !_mixer.IsValid() || !_layerMixer.IsValid())
                return;

            for (int i = 0; i < _slotClips.Length; i++)
            {
                if (_mixer.GetInput(i).IsValid())
                {
                    _mixer.DisconnectInput(i);
                }

                _slotClips[i] = null;
                _mixer.SetInputWeight(i, 0f);
            }

            _layerMixer.SetInputWeight(1, 0f);
            _layerMixer.SetLayerAdditive(1, false);
            _output.SetWeight(_hasBaseController ? 1f : 0f);

            if (restoreDefaultPose && !_hasBaseController && _animator)
            {
                // Idle clips often key only part of the rig; rebind clears residual pose from punch overrides.
                _animator.Rebind();
                _animator.Update(0f);
            }
        }

        private AnimationClipPlayable GetOrCreatePlayable(AnimationClip clip)
        {
            if (_clipMap.TryGetValue(clip, out var playable))
                return playable;

            playable = AnimationClipPlayable.Create(_graph, clip);
            _clipMap.Add(clip, playable);

            return playable;
        }


        public void Apply(in BlendResult result)
        {
            Apply(in result, true);
        }

        public void Apply(in BlendResult result, bool restartClipTimes)
        {
            if (!_initialized || !_mixer.IsValid() || !_layerMixer.IsValid() || result.Samples == null)
                return;

            float requestedWeight = result.BlendWeight > 0f ? result.BlendWeight : 1f;
            bool additiveMode = result.Mode == BlendApplicationMode.Additive;

            if (additiveMode && !_hasBaseController)
            {
                // Additive needs a base stream; fallback to override when no controller is available.
                additiveMode = false;
            }

            int maxSlotCount = _slotClips.Length;

            // Merge duplicate clips so one clip playable output is never connected twice.
            _uniqueClips.Clear();
            _uniqueWeights.Clear();
            for (int i = 0; i < result.Samples.Length; i++)
            {
                var sample = result.Samples[i];
                if (!sample.Clip || sample.Weight <= 0f)
                    continue;

                int existingIndex = _uniqueClips.IndexOf(sample.Clip);
                if (existingIndex >= 0)
                {
                    _uniqueWeights[existingIndex] += sample.Weight;
                }
                else
                {
                    _uniqueClips.Add(sample.Clip);
                    _uniqueWeights.Add(sample.Weight);
                }
            }

            int count = Mathf.Min(_uniqueClips.Count, maxSlotCount);
            float totalWeight = 0f;
            for (int i = 0; i < count; i++)
            {
                totalWeight += _uniqueWeights[i];
            }

            if (count == 0 || totalWeight <= 0f)
            {
                Clear(false);
                return;
            }

            for (int i = 0; i < maxSlotCount; i++)
            {
                if (_mixer.GetInput(i).IsValid())
                {
                    _mixer.DisconnectInput(i);
                }

                _slotClips[i] = null;
                _mixer.SetInputWeight(i, 0f);
            }


            for (int i = 0; i < count; i++)
            {
                var clip = _uniqueClips[i];
                var weight = _uniqueWeights[i] / totalWeight;

                var clipPlayable = GetOrCreatePlayable(clip);
                if (restartClipTimes)
                {
                    clipPlayable.SetTime(0d);
                    clipPlayable.SetDone(false);
                }

                _mixer.ConnectInput(i, clipPlayable, 0);
                _slotClips[i] = clip;

                _mixer.SetInputWeight(i, weight);
            }

            _layerMixer.SetLayerAdditive(1, additiveMode);
            _layerMixer.SetInputWeight(1, Mathf.Clamp01(requestedWeight));
            _output.SetWeight(1f);
        }
    }
}