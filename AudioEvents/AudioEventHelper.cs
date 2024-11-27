using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach
namespace UnitySimplified.Audio
{
    public sealed partial class AudioEventHelper : MonoBehaviour
    {
        private readonly List<Operation> _operations = new();
        private CancellationTokenSource _taskCancellationTokenSource;

        public bool IsPlaying { get; private set; }
        public bool IsStopping { get; private set; }
        public float Volume { get; internal set; } = 1;

        internal void Initialize(AudioEvent context)
        {
            _operations.Clear();
            //if (_target)
            //{
            //    // Do something if previous exists?
            //}
            //_target = target;

            //int lastIndex = 0;
            //for (int i = 0, j = 0; i < target.Entries.Length; i++)
            //{
            //    if (!target.Entries[i].Enabled || !target.Entries[i].Clip)
            //        continue;

            //    if (_controls.Count > j)
            //        _controls[j].Reinitialize(target.Entries[i]);
            //    else _controls.Add(new EntrySoundController(this, target.Entries[i], onInstantiateEntry.Invoke()));
            //    lastIndex = ++j;
            //}

            //for (int i = lastIndex; i < _controls.Count; i++)
            //    _controls[i].Reset();
        }

        private void OnDisable() => StopImmediately();

        public void Play()
        {
            if (IsPlaying)
                return;
            IsPlaying = true;
            for (int i = 0; i < _operations.Count; i++)
                _operations[i].Play();

            _taskCancellationTokenSource = new CancellationTokenSource();
            Task updateTask = new(FrameUpdate, _taskCancellationTokenSource.Token);
            updateTask.RunSynchronously();
        }
        public void Stop()
        {
            if (!IsPlaying || IsStopping)
                return;
            IsStopping = true;
            for (int i = _operations.Count - 1; i >= 0; i--)
                _operations[i].Stop();
        }
        public void StopImmediately()
        {
            if (!IsPlaying)
                return;
            IsStopping = false;
            IsPlaying = false;
            for (int i = _operations.Count - 1; i >= 0; i--)
                _operations[i].StopImmediately();
        }
        public async void FrameUpdate()
        {
            while (true)
            {
                await Task.Yield();
                if (!IsPlaying)
                    return;
                bool canStop = true;
                for (int i = _operations.Count - 1; i >= 0; i--)
                {
                    _operations[i].Update();
                    if (_operations[i].IsPlaying)
                        canStop = false;
                }

                if (canStop)
                    StopImmediately();
            }
        }

        public void AppendOperation(Operation operation) => _operations.Add(operation);
    }
}