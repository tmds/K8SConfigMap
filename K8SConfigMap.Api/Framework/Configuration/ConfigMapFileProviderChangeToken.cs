using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace K8SConfigMap.Api.Framework.Configuration
{
    public sealed class ConfigMapFileProviderChangeToken : IChangeToken, IDisposable
    {
        private readonly string _filePath;
        private readonly int _detectChangeIntervalMilliseconds;
        private readonly object _lock = new();
        private Timer _timer;
        private bool _hasChanged;
        private string _lastCheckSum = "";
        private List<CallbackRegistration> _registeredCallbacks = new();

        public ConfigMapFileProviderChangeToken(string filePath, int detectChangeIntervalMilliseconds)
        {
            _filePath = filePath;
            _detectChangeIntervalMilliseconds = detectChangeIntervalMilliseconds;
        }

        #region IChangeToken Methods

        public bool HasChanged => _hasChanged;
        public bool ActiveChangeCallbacks => true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var rc = _registeredCallbacks;
            if (rc == null)
            {
                throw new ObjectDisposedException(nameof(_registeredCallbacks));
            }

            var registration = new CallbackRegistration(callback, state, (cb) => rc.Remove(cb));
            rc.Add(registration);

            return registration;
        }

        #endregion

        public void Start()
        {
            lock (_lock)
            {
                if (_timer == null && File.Exists(_filePath))
                {
                    _timer = new Timer(CheckForChanges);
                    _timer.Change(0, _detectChangeIntervalMilliseconds);
                }
            }
        }

        private void CheckForChanges(object state)
        {
            var checkSum = GetFileChecksum(_filePath);
            if (_lastCheckSum != checkSum)
            {
                var rc = _registeredCallbacks;
                if (rc != null)
                {
                    var count = rc.Count;
                    for (var i = 0; i < count; i++)
                    {
                        rc[i].Notify();
                    }
                }

                _hasChanged = true;
                _lastCheckSum = checkSum;
            }
        }

        private static string GetFileChecksum(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(md5.ComputeHash(stream));
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _registeredCallbacks, null);

            Timer localTimer = null;
            lock (_lock)
            {
                localTimer = Interlocked.Exchange(ref _timer, null);
            }

            if (localTimer != null)
            {
                localTimer.Dispose();
            }
        }

        private class CallbackRegistration : IDisposable
        {
            private Action<object> _callback;
            private object _state;
            private Action<CallbackRegistration> _unregister;

            public CallbackRegistration(Action<object> callback, object state, Action<CallbackRegistration> unregister)
            {
                _callback = callback;
                _state = state;
                _unregister = unregister;
            }

            public void Notify()
            {
                var ls = _state;
                var lc = _callback;
                if (lc != null)
                {
                    lc.Invoke(ls);
                }
            }


            public void Dispose()
            {
                var lu = Interlocked.Exchange(ref _unregister, null);
                if (lu != null)
                {
                    lu(this);
                    _callback = null;
                    _state = null;
                }
            }
        }
    }
}