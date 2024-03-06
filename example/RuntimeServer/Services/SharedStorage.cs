namespace Saffron.Runtime.Services;

using System;
using System.Collections.Generic;

public class SharedStorage
{
    private static readonly Lazy<SharedStorage> _lazyStorage =
        new Lazy<SharedStorage>(() => new SharedStorage());

    public Dictionary<string, StorageValue> localStorage = new();
    public Dictionary<string, StorageValue> sessionStorage = new();

    private SharedStorage() { }

    public static SharedStorage GetStorage()
    {
        return _lazyStorage.Value;
    }
}
