using System;
using Game.Scripts.Core.Data;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Data
{
    public sealed class LocalUserIdProvider : IUserIdProvider
    {
        private const string PrefsKey = "cubeclash.userId";

        public string UserId { get; }

        public LocalUserIdProvider()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                UserId = PlayerPrefs.GetString(PrefsKey);
                return;
            }

            UserId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PrefsKey, UserId);
            PlayerPrefs.Save();
        }
    }
}
