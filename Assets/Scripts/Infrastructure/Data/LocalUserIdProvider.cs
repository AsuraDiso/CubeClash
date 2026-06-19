using System;
using Core.Data;
using UnityEngine;

namespace Infrastructure.Data
{
    public sealed class LocalUserIdProvider : IUserIdProvider
    {
        const string PrefsKey = "cubeclash.userId";

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
